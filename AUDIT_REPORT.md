# SubsTracker — Security, Performance & Code Quality Audit

**Date:** 2026-09-07
**Scope:** `SubsTracker/` (.NET 9 solution — API, BLL, DAL, Domain, Hangfire, Messaging, Mediator)
**Stack:** EF Core / PostgreSQL, Redis (cache + distributed locks), MassTransit/RabbitMQ, Hangfire (Mongo storage), Auth0/JWT, FluentValidation, Mapster

---

## 🚨 Do these first

1. **Rotate exposed secrets.** `.env` / `nuget.config` contain a live-looking GitHub PAT, MongoDB Atlas password, and Auth0 client secret in plaintext. They were exposed during this audit — treat as compromised and rotate regardless of anything else below.
2. **Lock down the Hangfire dashboard.** It currently has zero authentication and is reachable on host port 5158.
3. **Fix the authorization model.** Most endpoints check *authentication* only, not *ownership/role*. This is the single biggest risk in the codebase — see Security §Critical.

---

## 1. Security

### Critical

| # | Issue | Location |
|---|---|---|
| 1 | **IDOR** — any authenticated user can read/rename/delete any group, and inject/remove any subscription from it, no membership check | `GroupsController.cs:26-31,67-81,114-129` → `GroupService.cs` |
| 2 | **Privilege escalation** — `POST /api/groups/join` takes `UserId`/`Role` straight from the client body; anyone can self-promote to Admin of any group | `CreateMemberDto.cs:5-9`, `GroupsController.cs:86-90`, `MemberPolicyChecker.cs:13-28` |
| 3 | **Privilege escalation** — `ChangeRole` has no check that the caller is an admin/moderator of that group | `GroupsController.cs:104-109`, `MemberService.cs:72-98` |
| 4 | **IDOR** — `DELETE /api/groups/leave` lets anyone remove any other user from any group | `GroupsController.cs:95-99`, `MemberService.cs:100-114` |
| 5 | **IDOR** — `GET /api/subscriptions/{id}` returns any user's subscription; no ownership check (sibling endpoints correctly check via `SubscriptionPolicyChecker`) | `SubscriptionsController.cs:26-31`, `SubscriptionService.cs:28-40` |
| 6 | **IDOR** — `PATCH /{id}/renew` mutates any subscription's billing state; the method signature doesn't even take a `userId` | `SubscriptionsController.cs:87-92`, `SubscriptionService.cs:109-141` |
| 7 | **Mass data exposure** — `GET /api/users`, `/api/subscriptions`, `/api/groups`, `/api/groups/members` return **every** record system-wide when called with no filter (includes PII); no role gate exists anywhere in the API | `UsersController.cs:48-53`, `SubscriptionsController.cs:36-41`, `GroupsController.cs:36-51` |
| 8 | **Hangfire dashboard unauthenticated** and exposed on host port 5158 — anyone reaching it can view job payloads, trigger jobs, delete/requeue jobs | `HangfireAuthFilter.cs:5-11` (`Authorize()` hardcoded `return true`), `Hangfire/Program.cs:10-15`, `docker-compose.yaml:68-80` |

Root cause (see Medium #12): there is no role-based authorization mechanism anywhere in the API — `[Authorize]` only proves the caller is logged in, not that they own or administer the resource. Fixing #1–8 requires checking group membership/role or resource ownership server-side using `User.GetInternalId()`, never a client-supplied ID.

### High

- **Live secrets on disk.** `.env` (repo root) contains, in cleartext: a GitHub PAT (`GITHUB_TOKEN=ghp_...`), a live MongoDB Atlas connection string with credentials, and an Auth0 `ClientSecret`. The same GitHub PAT is duplicated in `nuget.config` as a `ClearTextPassword`. Neither file is tracked by git, but both sit in plaintext on disk with working credentials reused across two files. **Rotate the GitHub PAT, MongoDB password, and Auth0 client secret.**
- **`nuget.config` isn't excluded from the Docker build context.** `.dockerignore` excludes `.env`/`docker-compose*`/`Dockerfile*` but not `nuget.config`; `Dockerfile:9-12` does `COPY . .`, baking the GitHub PAT into an intermediate build-stage image layer (recoverable via `docker history`/cached layers even though the final stage doesn't copy it).
- **No request-level rate limiting on the public API.** The only rate limiter configured (`SlidingWindowRateLimiter` in `ResilienceDependencies.cs:33-39`) is a Polly pipeline stage for internal Auth0 calls via `UserOrchestrator`, not `app.UseRateLimiter()` on the HTTP pipeline. Login and write endpoints have no brute-force/DoS protection.

### Medium

- **No role-based authorization mechanism exists at all** — a repo-wide search found zero `[Authorize(Roles = ...)]` or policy-based checks; `MemberRole` (Participant/Moderator/Admin) is never used to gate API access. Root cause of the Critical IDORs above.
- **RabbitMQ uses default `guest`/`guest` credentials**, including in the "real" docker-compose deployment (`SubsTracker.Hangfire/appsettings.json:17-22`, `.env:11-14`), with the management UI port (15672) published to the host.

### Low

- `Cookie__Domain=null` (`.env:59`) binds as the literal string `"null"` because `CookieOptions.Domain` (`Options/CookieOptions.cs:22`) isn't nullable — should be `string?`, only assigned when non-null.

### Verified clean (checked, no issue found)

- No raw/interpolated SQL anywhere in `SubsTracker.DAL` — all repository/filter code uses parameterized LINQ expression trees; no injection risk.
- CORS policy is an explicit origin allow-list from config, not `AllowAnyOrigin` (correct even with `AllowCredentials()`).
- HTTPS redirection and HSTS (non-Development) are enabled.
- Global exception middleware returns a generic message and doesn't leak stack traces; logs server-side only.
- No sensitive data (passwords, tokens, full PII) found in any logging call.
- No hardcoded credentials in `.cs` source files — secrets are read from `IConfiguration`/`IOptions`.

---

## 2. Performance

### High

- **H1 — Hot-path auth query: uncached, unindexed, over-fetching.** `IClaimsTransformation` runs on every JWT-authenticated request. For real Auth0 tokens the Guid-parse short-circuit never fires, so `UserService.GetByIdentityId` hits the DB on *every* request, bypassing the cache entirely, with `.Include(Subscriptions).Include(Groups)` (3 round trips via `AsSplitQuery`) just to resolve one ID — and `Users.IdentityId` has **no index** at all.
  *(`Auth/IdentityManager.cs:29`, `Auth/Session/ClaimsTransformer.cs:13-16`, `BLL/Services/UserService.cs:50-68`, `DAL/Repository/UserRepository.cs:11-21`)*
- **H2 — Dead `Include` chains.** `GroupRepository.GetFullInfoById` and `SubscriptionRepository.GetUserInfoById` eagerly join `Members`/`User`, which the target DTOs (`GroupDto`, `SusbcriptionDto`) don't even have properties for — fetched from Postgres, then silently discarded by the mapper.
  *(`DAL/Repository/GroupRepository.cs:11-18`, `DAL/Repository/SubscriptionRepository.cs:11-17`)*
- **H3 — No designed indexes anywhere**, only auto-generated FK indexes (`SubsDbContext.cs` has no `OnModelCreating` override). `Subscriptions.Active`/`DueDate` are unindexed despite being filtered **daily** by the Hangfire expiration job (full sequential scan on a growing table); `Users.Email`/`Users.IdentityId` are unindexed too.
- **H4 — `CancelRange` runs its predicate twice.** Materializes matching rows into memory, then re-filters with `.Contains(subscription)` (producing a large parameterized `IN` clause) instead of one direct `ExecuteUpdateAsync`.
  *(`DAL/Repository/SubscriptionRepository.cs:30-47`)*
- **H5 — Sequential per-row publish/insert in the daily expiration job.** One DB insert + one RabbitMQ publish per expired subscription inside a loop, no batching.
  *(`BLL/Services/SubscriptionService.cs:148-162`)*
- **H6 — Pagination is optional and silently defaults to "return everything."** Four list endpoints declare `PaginationParameters?` as nullable; when the query params are omitted, `pageSize = totalCount` returns the entire table in one response.
  *(`DAL/Extensions/QueryableExtensions.cs:8-28`, `UsersController.cs:49`, `GroupsController.cs:37`, `SubscriptionsController.cs:37,47`)*

### Medium

- Cache-stampede fallback waits once (5s) then gives up, returning a false "not found" for an entity that actually exists, under load (`BLL/Services/Cache/CacheService.cs:41-98`).
- Read-only repository calls don't use no-tracking even though the `isTracking` flag/pattern already exists — it's just never set to `false` from read-only call sites.
- Leading-wildcard `LIKE '%value%'` filters on unindexed columns force full scans regardless of indexing; needs a `pg_trgm` GIN index if free-text search over names is a real requirement.
- Dead cache-invalidation code for a `"user_groups_list"` key that is never written anywhere.
- Two JSON serializers in the hot path — Newtonsoft.Json for Redis cache (de)serialization, System.Text.Json for HTTP responses — avoidable overhead on every cache hit.
- Independent existence checks (user + group) are awaited sequentially instead of via `Task.WhenAll`.

### Low

- `RedisOptions` class is dead/unused — the Redis connection string is read as a raw config string elsewhere, bypassing it entirely.
- A single global 3-minute cache TTL is used for all entity types regardless of data volatility.

### Verified clean

- No blocking `.Result` / `.Wait()` / `Task.Run`-wrapped sync code anywhere in API/BLL/DAL/Hangfire.
- DI lifetimes (Scoped for DbContext/repos/services, Singleton for Redis multiplexer/lock factory) are correctly configured.
- Base `Repository<T>.GetAll` correctly uses `.AsNoTracking()` and orders before paging — no client-side evaluation found.
- DTOs/ViewModels are projected, not raw entities, for all standard CRUD paths (aside from the dead-`Include` issue in H2).

---

## 3. Code Quality

### High

- **H1 — Real bug, will crash in production.** `FilterHelper.AddFilterCondition` (string overload) has swapped ternary branches: when a filter value is blank, it returns the bare `condition` — which still references the (now-null) filter value via the `!` operator — instead of the accumulated `predicate`. **`GET /api/users` without an `email` query parameter throws `NullReferenceException` → 500.** Also silently drops previously-added filter conditions for the `Subscription`/`SubscriptionHistory` filters that share this helper.
  *(`BLL/Helpers/Filters/FilterHelper.cs:19-27`, exercised via `UserFilterHelper.cs:20-36`)* — masked because unit tests match predicate arguments with `Arg.Any<Expression<...>>()` and never actually execute/compile them.
- **H2 — The group-sharing feature is broken end-to-end, and the integration test hides it.** `GroupDto` has no `SharedSubscriptions`/`Members` properties, so `GroupViewModel.SharedSubscriptions` is **always null** in every API response — even immediately after a successful `POST /api/groups/share`. The integration test asserts `result.SharedSubscriptions?.ShouldContain(...)`; the `?.` makes this a silent no-op regardless of whether the feature works.
  *(`BLL/DTOs/User/GroupDto.cs`, `DAL/Repository/GroupRepository.cs:11-18`, `IntegrationTests/Group/GroupsControllerTests.cs:164`)*
- **H3 — Delete operations publish domain events *before* the actual delete.** In `SubscriptionService.Delete`, `GroupService.Delete`, `MemberService.LeaveGroup`, and `UserService.Delete`, the mediator publish happens before the repository call — a failed delete (concurrency conflict, FK violation) still fires cache invalidation, history writes, and RabbitMQ notifications for a row that was never removed. Every other mutation in these classes correctly publishes after success.
- **H4 — No transactional outbox**, despite messages being published inline on the request path (`Messaging/Services/MessageService.cs`, `SubscriptionMessageHandler`). A RabbitMQ blip after a DB commit surfaces a 500 to the client even though the write already succeeded; a process crash between commit and publish silently loses the notification. MassTransit ships a built-in EF Core outbox that isn't used here.
- **H5 — `MemberCacheHandler` never invalidates the `MemberEntity` cache itself** on role change or leave, only `GroupEntity`/`UserEntity` — stale member/role data can be served for up to 3 minutes after `ChangeRole`/`LeaveGroup`.
- **H6 — `!` on nullable service results instead of proper 404 handling.** `GET` by id for subscriptions/groups returns HTTP 200 with an empty body for a nonexistent id, instead of throwing `UnknownIdentifierException` → 404, inconsistent with every other `GetById` in the codebase.
- **H7 — Wrong exception type.** `GroupService.UnshareSubscription` throws a bare `ArgumentException`, which `ExceptionHandlingMiddleware`'s switch doesn't map — falls through to a 500 instead of 400/404.
- **H8 — Resilience config ships as all zeros in `appsettings.json`** (retry/circuit-breaker/rate-limiter sections). If this were ever the effective config, timeouts would fire immediately and the circuit breaker would likely throw at pipeline-build time; there's no range validation to catch it.
- **H9 — Cross-system retry with no compensation.** `UserOrchestrator.FullUserUpdate`/`FullUserDelete` wraps an Auth0 call plus a local DB call inside one retry block. If Auth0 succeeds but the DB step fails, retry re-invokes the (non-idempotent) Auth0 call again; on delete, this can permanently orphan the local DB row after the Auth0 identity is already gone.

### Medium

- `CancelRange`'s count-mismatch check discards the **entire batch** on a race condition, silently skipping cancellation notifications for subscriptions that were, in fact, updated.
- Inconsistent configuration hygiene — Postgres/Redis/Mongo connection strings bypass the `RegisterOptions<T>().ValidateOnStart()` pattern used for every other option class; failures surface as confusing runtime nulls instead of fail-fast startup errors.
- `Auth0Service` fetches a brand-new client-credentials token on every call, with no caching, despite such tokens typically being valid for hours.
- Cache-key strings (`"upcoming_bills"`, `"user_groups_list"`) are hardcoded independently in producer and invalidator code — a typo in either would silently break invalidation permanently.
- The "cache → fetch → 404" pattern is copy-pasted near-identically across four services (`SubscriptionService`, `GroupService`, `MemberService`, `UserService`) — a good candidate for one generic helper.

### Low

- `MemberMessageHandler.cs` actually defines a class named `MemberNotificationHandler` — naming drift versus its sibling `SubscriptionMessageHandler`.
- The circuit-breaker predicate references `Microsoft.Data.SqlClient.SqlException` in a PostgreSQL-only project — a dead branch pulling in an unnecessary dependency.
- A dead defensive null-check on `GetUpcomingBills`, whose return type can never actually be null.

### Verified clean

- Layering direction is correct throughout (API → BLL → DAL → Domain), confirmed via project references; no `DAL` usages leak into `API`.
- No empty/swallowed catch blocks and no stray `catch (Exception)` in production code — the only catch block in the non-test codebase is the intentional, logging global exception handler.
- No service-locator anti-pattern — every `IServiceProvider.GetRequiredService` call sits inside legitimate DI-registration/factory code, never inside business logic.
- Unit tests for the five BLL services are substantial (hundreds of lines each) with real assertions and call verification — the gap is coverage breadth (filter helpers, policy checkers, cache handlers, message handlers are untested), not the quality of what exists.

---

## Suggested priority order

1. Rotate the exposed secrets; add `nuget.config` to `.dockerignore`.
2. Add ownership/role checks to close the IDOR/privilege-escalation findings (Security §Critical #1–8) — the highest-impact fix in this audit.
3. Secure the Hangfire dashboard (real auth filter + restrict network exposure).
4. Fix `FilterHelper` (guaranteed production crash) and the `GroupDto` mapping gap (broken feature, masked by a bad test assertion).
5. Add the missing indexes (`Users.IdentityId`, `Subscriptions(Active, DueDate)`) and fix the pagination nullable-fallback — cheap, high-impact.
6. Move delete-event publishing to after the actual delete succeeds; consider adopting MassTransit's transactional outbox for the RabbitMQ dual-write problem.
