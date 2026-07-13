<h1 align="center">LOVE.NET</h1>
<p align="center"> React SPA dating application — now running on event-driven .NET 8 microservices
    <br>
</p>

## 📝 Table of Contents
- [About](#about)
- [Architecture](#architecture)
- [How the Components Communicate](#communication)
- [Getting Started](#getting_started)
- [Database Placement](#database_placement)
- [Tests](#tests)
- [Built Using](#built_using)

## 🧐 About <a name = "about"></a>
`LOVE.NET` gives you opportunity to match with people around the world and chat easily in real time.
Some accounts, countries, cities, genders are loaded after the first start of the application — through the same Kafka event pipeline production traffic uses.

This repository is a **refactoring test driven by Claude Fable 5** (Anthropic's Claude Code): the original layered monolith was rebuilt into seven event-driven services designed to scale the realtime chat workload horizontally, and the SPA was rewritten on the 2026 React stack. The pre-refactor monolith is preserved on the [`monolith-snapshot`](../../tree/monolith-snapshot) branch.

## 🏗️ Architecture <a name = "architecture"></a>

```
React SPA ──► Gateway (YARP) ──► Identity │ Profile │ Matching │ Chat │ Realtime (SignalR) │ Notifications
                                     │         │         │        │        │                    │
                                  Postgres  Postgres  Postgres Postgres  Redis               Postgres
                                     └─────────┴────── Kafka (outbox → topics → inbox) ────────┘
```

* Authorized SignalR hub with a **Redis backplane** and Redis presence — no in-process state
* **Durable-before-display** chat: Kafka ack (keyed by room for ordering) before any client renders the message
* **Transactional outbox + inbox dedupe** on every service boundary, dead-letter queue for poison messages
* **Database-per-service** with event-carried read models — no cross-service joins
* Gateway with rate limiting, websocket proxying, and a resilient admin-stats fan-out
* Structured logs + OpenTelemetry traces (optional Jaeger), correlation ids across HTTP and Kafka hops

The full design — topics, delivery semantics, resiliency patterns, scaling story — is in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## 🔌 How the Components Communicate <a name = "communication"></a>

**SPA → Gateway (HTTP + WebSocket).** The React app talks only to the YARP gateway on `:8080`: REST under `/api/*` with a JWT bearer header (silent refresh via an HttpOnly cookie), and one SignalR WebSocket at `/chat` (the JWT rides the `access_token` query parameter). The gateway proxies both, applies CORS and per-IP rate limits, and serves `/api/admin/dashboard` itself by fanning out to each service's `/internal/stats` with timeouts, retries and circuit breakers.

**Synchronous (HTTP).** That gateway fan-out is the *only* service-to-service HTTP in the system — services never call each other for business flows, so no service can stall another.

**Asynchronous (Kafka).** Everything cross-service is an event. Producers write to a transactional **outbox** table in the same DB transaction as their state change; a relay publishes to Kafka; consumers dedupe via an **inbox** table (at-least-once delivery, effectively-once processing) and dead-letter poison messages after bounded retries. Keys guarantee per-user / per-room ordering:

| Topic | Producer → Consumers | Purpose |
|---|---|---|
| `identity.user-registered` | Identity → Profile, Notifications | build the profile aggregate, contacts |
| `identity.user-banned` | Identity → Profile | mirror moderation state |
| `profile.user-updated` (enriched) | Profile → Matching, Chat, Identity, Notifications | read models: swipe deck, chat avatars, login response |
| `matching.match-created` | Matching → Chat, Realtime, Notifications | provision the room, push the match toast, send emails |
| `chat.message-sent` (8 partitions, key = roomId) | Realtime → Chat | durable persistence of hub messages |
| `chat.room-read` | Realtime → Chat | read-receipt watermarks |
| `chat.room-provisioned` | Chat → Realtime | public-room join authorization |
| `notifications.email-requested` | Identity → Notifications | verification / reset emails |

**Redis (Realtime only).** The SignalR hub is stateless: the **Redis backplane** fans `ReceiveMessage`/`MatchReceived` broadcasts across hub replicas, room presence lives in Redis hashes, and the public-room set in a Redis set — so any number of Realtime instances behave as one. Realtime has no database; it publishes straight to Kafka (acks=all) *before* broadcasting, so no client ever sees a message the broker didn't accept.

**PostgreSQL (database-per-service).** Each service owns its schema and is the only writer to it; what other services need arrives as events and lands in local read models — there are no cross-service joins or shared tables.

**External services.** Identity/Profile/Chat upload images to **Cloudinary** over HTTPS; Notifications delivers mail through **SendGrid**. Auth everywhere is the same HS512 **JWT** issued by Identity and validated locally by every service and the hub — no auth round-trips.

## 🏁 Getting Started <a name = "getting_started"></a>

### Run the project

```
cp .env.example .env
```
```
docker compose up -d --build
```
```
SPA: http://localhost:3000 · Gateway: http://localhost:8080 · Swagger: http://localhost:8081..8086/swagger
```
```
./scripts/smoke-test.ps1  (25-step end-to-end proof of the whole stack)
```

![Containers](docs/images/containers.png)

### Authentication and Authorization

```
JWT Bearer token is being used for authentication and authorization (issued by Identity, validated by every service and the SignalR hub)
```
```
User is filling his mandatory data and uploading images.
```
```
Resend email verification if no email is received
```
```
Verification - required step in order to use the app
```
```
Reset password if forgotten
```

### User profile

```
Fast and easy edit of your profile information
```

```
Add and remove photos
```

### Home Tab

```
All not swiped users
```

```
Easy filtration by age, gender, distance in km calculated Latitude and Longitude of both users.
```
```
Easy swipe for mobile users, and buttons for PC master race fans
```
```
Upon successful match - notification is received (pushed over the hub to both users)
```

### Matches Tab

```
All matches you have got
```

```
Easy chat with option to send images
```
```
Infinite scrolling
```
```
Search your beloved ones
```

### Dashboard Tab
```
Up-to-date statistics (aggregated by the gateway from every service)
```

```
Infinite scrolling of users, with possibility to ban them
```
```
Search for intruders of the site
```

## 📈 Database Placement <a name = "database_placement"></a>
Every service owns its schema; the shared tables you see (`outbox_messages`, `inbox_messages`) are the reliable-messaging pattern, and `messages` in the chat database is a **partitioned table** with monthly partitions.

![Database placement](docs/images/database-placement.png)

## 🧪 Tests <a name = "tests"></a>
* 41 NUnit tests across the five services (ported and adapted from the monolith suite)
* Plus a 25-step end-to-end smoke test against the live compose stack (`scripts/smoke-test.ps1`)
* ![Tests](docs/images/tests.png)

## ⛏️ Built Using <a name = "built_using"></a>
* React 19 + TypeScript (Vite)
* Tailwind CSS v4 + shadcn/ui
* TanStack Query + motion + sonner
* .NET 8 APIs (7 services)
* EF Core 8 with PostgreSQL (database-per-service)
* Apache Kafka (KRaft)
* Redis
* YARP API Gateway
* Swashbuckle Swagger
* JWT Bearer
* SignalR (+ Redis backplane)
* Serilog + OpenTelemetry
* Cloudinary
* SendGrid
* NUnit
* Moq
* Axios
* Day.js
* Docker Compose
