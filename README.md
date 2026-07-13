<h1 align="center">LOVE.NET</h1>
<p align="center">A dating app — swipe, match, chat in real time — built as event-driven microservices on .NET 8, Kafka, Redis, and PostgreSQL.
    <br>
</p>

## About

LOVE.NET lets you match with people around the world and chat in real time.
Demo accounts, countries, cities, and genders are seeded on first start —
through the same Kafka event pipeline production traffic uses.

This repository was refactored from a layered ASP.NET Core monolith into
seven services designed to scale the realtime chat workload horizontally.
The original monolith is preserved on the
[`monolith-snapshot`](../../tree/monolith-snapshot) branch, and the full
design — service boundaries, Kafka topics, delivery semantics, resiliency
patterns, and the scaling story — is documented in
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

```
React SPA ──► Gateway (YARP) ──► Identity │ Profile │ Matching │ Chat │ Realtime (SignalR) │ Notifications
                                     │         │         │        │        │                    │
                                  Postgres  Postgres  Postgres Postgres  Redis               Postgres
                                     └─────────┴────── Kafka (outbox → topics → inbox) ────────┘
```

Highlights:

- **Realtime chat** over an authorized SignalR hub with a Redis backplane and
  Redis presence — horizontally scalable, no in-process state.
- **Durable-before-display** messaging: the hub gets a Kafka ack (acks=all,
  keyed by room for ordering) before any client renders the message; the Chat
  service persists it idempotently into a **monthly-partitioned** Postgres
  table behind a storage seam ready for Cassandra/ScyllaDB.
- **Transactional outbox + inbox dedupe** on every service boundary:
  at-least-once delivery, effectively-once processing, dead-letter queue for
  poison messages.
- **Database-per-service** with event-carried read models — no cross-service
  joins, no shared tables.
- **Gateway** with rate limiting (per-IP, tighter on credentials), a single
  CORS policy, websocket proxying, and a resilient fan-out aggregation for the
  admin dashboard.
- **Observability**: structured logs, OpenTelemetry traces/metrics with an
  optional Jaeger container, correlation ids across HTTP and Kafka hops.

## Getting started

Prerequisites: Docker Desktop. Optionally Cloudinary/SendGrid keys for image
uploads and real emails (everything else works without them).

```bash
cp .env.example .env          # fill in secrets if you have them
docker compose up -d --build  # ~15 containers: services, dbs, kafka, redis, SPA
```

- SPA: http://localhost:3000
- Gateway (API + websocket): http://localhost:8080
- Swagger per service: http://localhost:8081..8086/swagger
- Tracing UI (optional): `docker compose --profile observability up -d jaeger` → http://localhost:16686

Verify the whole stack end to end:

```powershell
./scripts/smoke-test.ps1
```

The smoke test registers users through the gateway, drives the
identity → profile → matching event pipeline, creates a match, joins the
SignalR hub over a websocket, sends a message, proves it was persisted via
Kafka, and checks the admin aggregation — 20 assertions.

## Features

- Register with photos, e-mail verification, password reset (links arrive via
  the Notifications service)
- Swipe deck with age/distance/gender preferences, mutual-like matching with
  instant `MatchReceived` push
- 1:1 match chats and public themed chatrooms, with image messages and live
  presence
- Profile editing with photo management
- Admin dashboard: statistics, user search, ban/unban (lockout)

## Solution layout

```
src/
  BuildingBlocks/   Contracts (events), EventBus (Kafka + outbox/inbox),
                    Web (JWT, logging, telemetry, health), Media (Cloudinary)
  Gateway/          LoveNet.Gateway (YARP)
  Services/         Identity | Profile | Matching | Chat | Realtime | Notifications
                    (each: LoveNet.<X>.Api + LoveNet.<X>.Tests)
Web-FrontEnd/       React 18 SPA
scripts/            smoke-test.ps1 + hub-smoke.js
docs/               ARCHITECTURE.md
```

## Tests

```bash
dotnet test LoveNet.sln
```

37 NUnit tests across five services (ported and adapted from the monolith's
suite), plus the end-to-end smoke script above.

## Built using

.NET 8 · ASP.NET Core · SignalR (+ Redis backplane) · EF Core 8 / Npgsql ·
Apache Kafka (KRaft) · Redis · PostgreSQL 16 · YARP · Serilog ·
OpenTelemetry · React 18 · Docker Compose · Cloudinary · SendGrid
