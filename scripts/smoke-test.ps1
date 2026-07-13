# End-to-end smoke test for the LOVE.NET microservices stack.
# Prereq: docker compose up -d --build (all containers healthy).
# Works on Windows PowerShell 5.1 (uses curl.exe for multipart forms).

$ErrorActionPreference = 'Stop'
$gateway = 'http://localhost:8080'
$script:failures = 0

function Step($ok, $message) {
    if ($ok) { Write-Host "  OK   $message" -ForegroundColor Green }
    else { Write-Host "  FAIL $message" -ForegroundColor Red; $script:failures++ }
}

function Poll($description, [scriptblock]$probe, $timeoutSeconds = 90) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try { $result = & $probe; if ($result) { return $result } } catch { }
        Start-Sleep -Seconds 3
    }
    return $null
}

Write-Host "1. Health checks" -ForegroundColor Cyan
foreach ($port in 8080..8086) {
    try { $h = Invoke-RestMethod "http://localhost:$port/health" -TimeoutSec 10 } catch { $h = $_.Exception.Message }
    Step ($h -eq 'Healthy') "service on :$port is healthy ($h)"
}

Write-Host "2. Register two users (multipart form via gateway)" -ForegroundColor Cyan
$suffix = -join ((97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
$userA = "smokea$suffix"; $userB = "smokeb$suffix"
foreach ($name in @($userA, $userB)) {
    $status = curl.exe -s -o NUL -w "%{http_code}" -X POST "$gateway/api/identity/register" `
        -F "Email=$name@smoke.test" -F "Password=smoke123" -F "ConfirmPassword=smoke123" `
        -F "UserName=$name" -F "Bio=smoke test user" -F "Birthdate=1995-05-05" `
        -F "CountryId=30" -F "GenderId=1" -F "CityId=5808"
    Step ($status -eq '201') "registered $name (HTTP $status)"
}

Write-Host "3. Confirm emails directly in identity-db (test shortcut for the email loop)" -ForegroundColor Cyan
# SQL goes via stdin so PowerShell does not eat the quoted identifiers.
'UPDATE "AspNetUsers" SET "EmailConfirmed"=true WHERE "Email" LIKE ''%@smoke.test'';' |
    docker exec -i lovenet-identity-db psql -U lovenet -d lovenet_identity -q
Step $? "smoke users email-confirmed"

Write-Host "4. Login both users" -ForegroundColor Cyan
function Login($name) {
    Invoke-RestMethod -Uri "$gateway/api/identity/login" -Method Post -ContentType 'application/json' `
        -Body (@{ email = "$name@smoke.test"; password = 'smoke123' } | ConvertTo-Json)
}
$a = Login $userA; $b = Login $userB
Step ($a.token.Length -gt 100) "login $userA -> JWT"
Step ($b.token.Length -gt 100) "login $userB -> JWT"
$headersA = @{ Authorization = "Bearer $($a.token)" }
$headersB = @{ Authorization = "Bearer $($b.token)" }

Write-Host "5. Event pipeline: profiles propagate into the Matching deck" -ForegroundColor Cyan
$deck = Poll "deck contains $userB" {
    $d = Invoke-RestMethod -Uri "$gateway/api/dating" -Headers $headersA
    if (($d | Where-Object { $_.id -eq $b.id })) { $d }
}
Step ($null -ne $deck) "$userB appeared in $userA's swipe deck (identity -> profile -> matching events)"

Write-Host "6. Mutual like creates a match" -ForegroundColor Cyan
$like1 = Invoke-RestMethod -Uri "$gateway/api/dating/like/$($b.id)" -Method Post -Headers $headersA
Step (-not $like1.isMatch) "first like: no match yet"
$like2 = Invoke-RestMethod -Uri "$gateway/api/dating/like/$($a.id)" -Method Post -Headers $headersB
Step ($like2.isMatch -and $like2.user.id -eq $a.id) "reciprocal like: match created, matched user returned"

$matches = Invoke-RestMethod -Uri "$gateway/api/dating/matches" -Method Post -Headers $headersA `
    -ContentType 'application/json' -Body (@{ userId = $a.id; page = 1 } | ConvertTo-Json)
$roomId = ($matches.matches | Where-Object { $_.id -eq $b.id }).roomId
Step ($matches.totalMatches -ge 1 -and $roomId) "match list returns roomId $roomId"

Write-Host "7. Chat room provisioned from the match event" -ForegroundColor Cyan
$room = Poll "room row in chat-db" {
    $raw = "SELECT count(*) FROM rooms WHERE `"RoomId`"='$roomId';" |
        docker exec -i lovenet-chat-db psql -U lovenet -d lovenet_chat -t -A 2>$null
    if ($raw -match '^[1-9]') { $true }
}
Step ($null -ne $room) "rooms row exists for $roomId"

Write-Host "8. Realtime hub end-to-end (join, typing, send, echo, read receipt) via websocket" -ForegroundColor Cyan
$messageText = "smoke $suffix"
docker cp "$PSScriptRoot\hub-smoke.js" lovenet-frontend:/app/hub-smoke.js | Out-Null
$hubOut = docker exec -e GATEWAY_URL=http://gateway:8080 -e "TOKEN=$($a.token)" -e "TOKEN_B=$($b.token)" -e "ROOM_ID=$roomId" -e "MESSAGE_TEXT=$messageText" lovenet-frontend node hub-smoke.js 2>&1
Step ("$hubOut" -match 'HUB_OK') "hub echoed ReceiveMessage with server id"
Step ("$hubOut" -match 'PEER_OK') "peer saw typing + message and its MarkRead produced a live ReadReceipt ($(("$hubOut" | Select-String 'PEER_OK.*').Matches.Value))"

Write-Host "9. Message persisted by Chat via Kafka consumer" -ForegroundColor Cyan
$history = Poll "message in history" {
    $h = Invoke-RestMethod -Uri "$gateway/api/chat" -Method Post -Headers $headersA `
        -ContentType 'application/json' -Body (@{ roomId = $roomId; page = 1 } | ConvertTo-Json)
    if ($h.totalMessages -ge 1 -and ($h.messages | Where-Object { $_.text -eq $messageText })) { $h }
}
Step ($null -ne $history) "chat history contains the hub message (durable via chat.message-sent)"

Write-Host "9b. Conversation summaries reflect the read state (Kafka-persisted watermark)" -ForegroundColor Cyan
$summary = Poll "summaries for $roomId" {
    $s = Invoke-RestMethod -Uri "$gateway/api/chat/summaries" -Method Post -Headers $headersB `
        -ContentType 'application/json' -Body (@{ roomIds = @($roomId) } | ConvertTo-Json)
    $row = $s | Where-Object { $_.roomId -eq $roomId }
    if ($row -and $row.unreadCount -eq 0 -and $row.lastMessageText -eq $messageText) { $row }
}
Step ($null -ne $summary) "userB's summary: unreadCount=0 after MarkRead, last message present"
$summaryA = Invoke-RestMethod -Uri "$gateway/api/chat/summaries" -Method Post -Headers $headersA `
    -ContentType 'application/json' -Body (@{ roomIds = @($roomId) } | ConvertTo-Json)
Step ($null -ne ($summaryA | Where-Object { $_.roomId -eq $roomId }).peerLastReadAt) "userA sees peerLastReadAt (read receipt persisted)"

Write-Host "10. Public chatrooms" -ForegroundColor Cyan
$chatrooms = Invoke-RestMethod -Uri "$gateway/api/chat/chatrooms" -Headers $headersA
Step ($chatrooms.Count -ge 6) "seeded chatrooms returned ($($chatrooms.Count))"

Write-Host "11. Admin: promote $userA, aggregate stats via gateway fan-out" -ForegroundColor Cyan
"INSERT INTO `"AspNetUserRoles`" (`"UserId`",`"RoleId`") SELECT '$($a.id)', `"Id`" FROM `"AspNetRoles`" WHERE `"Name`"='Administrator' ON CONFLICT DO NOTHING;" |
    docker exec -i lovenet-identity-db psql -U lovenet -d lovenet_identity -q
$aAdmin = Login $userA
$stats = Invoke-RestMethod -Uri "$gateway/api/admin/dashboard" -Headers @{ Authorization = "Bearer $($aAdmin.token)" }
Step ($stats.usersCount -ge 8 -and $stats.matchesCount -ge 1 -and $stats.messagesCount -ge 1) `
    "stats aggregated: users=$($stats.usersCount) matches=$($stats.matchesCount) messages=$($stats.messagesCount)"
$unauthStatus = curl.exe -s -o NUL -w "%{http_code}" "$gateway/api/admin/dashboard" -H "Authorization: Bearer $($b.token)"
Step ($unauthStatus -eq '403') "non-admin gets 403 from admin stats"

Write-Host ""
if ($script:failures -eq 0) {
    Write-Host "SMOKE TEST PASSED" -ForegroundColor Green
    exit 0
}
Write-Host "SMOKE TEST FAILED ($script:failures failures)" -ForegroundColor Red
exit 1
