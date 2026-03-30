# 🎯 Improve Login Performance and Add Persistent Cookie Authentication

## Understanding
The user reports that logging in to production is very slow and sometimes fails. They also want the site to remember them so they don't have to log in every time they visit. The current code has several issues: no persistent cookie configuration, external logins marked as `isPersistent: false`, no cookie expiration settings, and the `ExternalLoginCallback.razor` component performs multiple sequential database calls that could fail silently or cause delays in cold-start scenarios.

## Assumptions
- The slowness in production is likely caused by: (1) Azure SQL cold-start latency during the Identity DB calls in the callback, (2) no connection pooling/retry on the external login callback path, and (3) unnecessary round-trips in the callback
- The "have to login every time" issue is because both external and local login flows set `isPersistent: false`, and there's no `ExpireTimeSpan` configured on the cookie — so the auth cookie is session-only and disappears when the browser closes
- The `Remember Me` checkbox on the local login form already exists but is not being leveraged for the external login flow
- The Key Vault configuration loading at startup with `DefaultAzureCredential` may also add several seconds on cold start

## Approach
1. **Configure persistent cookies** with a reasonable `ExpireTimeSpan` (14 days) and `SlidingExpiration` so the cookie refreshes itself when users are active. This is the primary fix for the "have to login every time" issue.

2. **Make external logins persistent** — currently `ExternalLoginSignInAsync` and `SignInAsync` both pass `isPersistent: false`. Change to `true` so external logins also persist across browser sessions.

3. **Add connection resiliency to the login callback** — the callback does serial DB lookups (`GetExternalLoginInfoAsync` → `ExternalLoginSignInAsync` → `FindByEmailAsync` → `CreateAsync` → `AddLoginAsync` → `SignInAsync`). If Azure SQL is cold, each call blocks. Add `try/catch` with a user-friendly retry prompt instead of silent failures.

4. **Optimize the callback flow** — the existing code calls `AddLoginAsync` even when the user already exists but the login entry might already be present (returning an error). Add a guard to avoid unnecessary writes.

5. **Add Data Protection key persistence** — without this, in a container environment, data protection keys are ephemeral and all cookies become invalid on every container restart, forcing re-login.

## Key Files
- PuppyWeightWatcher.Web/Program.cs — cookie configuration, data protection
- PuppyWeightWatcher.Web/Components/Pages/Account/ExternalLoginCallback.razor — external login flow fixes
- PuppyWeightWatcher.Web/Components/Pages/Account/Login.razor — minor improvements
- PuppyWeightWatcher.Web/Components/Pages/Account/Register.razor — make post-registration login persistent

## Risks & Open Questions
- Data Protection keys stored on the filesystem will survive container restarts but not container rebuilds; ideally keys should be stored in Azure Blob or the database, but filesystem is sufficient for now
- The 14-day cookie lifetime is a reasonable default; user may want to adjust

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-30 02:26:42

## 📝 Plan Steps
- ✅ **Configure persistent cookie settings in Program.cs (ExpireTimeSpan, SlidingExpiration, cookie name)**
- ✅ **Update ExternalLoginCallback.razor to use isPersistent: true, add retry/error handling, and optimize the flow**
- ✅ **Update Register.razor to use isPersistent: true for post-registration login**
- ✅ **Add Data Protection key persistence configuration in Program.cs for container environments**
- ✅ **Build and verify no compilation errors**

