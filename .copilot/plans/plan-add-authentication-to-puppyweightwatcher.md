# 🎯 Add Authentication to PuppyWeightWatcher

## Understanding
The user wants to add authentication to their Blazor Server + API Aspire app. Users should be able to register/login with local credentials (username/password) and also sign in via third-party OAuth providers (Microsoft, GitHub, Google). The Blazor Server web frontend will handle all authentication directly using ASP.NET Core Identity with cookie-based auth.

## Assumptions
- Identity will be hosted in the Web (Blazor Server) project since it handles cookies and OAuth redirects natively
- The Web project will get its own Identity database on the same SQL Server instance via Aspire
- External provider client IDs/secrets will be stored in user secrets (configured via AppHost)
- All existing pages will require authentication via `[Authorize]`
- The API service remains internal (backend-to-backend) and does not need its own auth layer at this time

## Approach
ASP.NET Core Identity with EF Core will be added to the **Web project**. A new `ApplicationUser` (extending `IdentityUser`) and `ApplicationDbContext` (extending `IdentityDbContext`) will be created. The AppHost will be updated to wire a new `identitydb` database to the web frontend via Aspire's SQL Server integration. 

For the Blazor UI, custom Identity pages (Login, Register, Logout, ExternalLogin callback) will be added as Razor components. The `Routes.razor` will be updated to use `AuthorizeRouteView` to require auth by default, with `RedirectToLogin` for unauthenticated users. The `MainLayout.razor` will show the logged-in user's name and a logout button. External OAuth providers (Microsoft, GitHub, Google) will be wired up in `Program.cs` using their respective `AddMicrosoftAccount`, `AddGitHub`, `AddGoogle` extension methods, with secrets stored in AppHost user secrets.

## Key Files
- PuppyWeightWatcher.Web/PuppyWeightWatcher.Web.csproj - add Identity + OAuth NuGet packages
- PuppyWeightWatcher.Web/Program.cs - register Identity services, auth middleware, external providers
- PuppyWeightWatcher.Web/Data/ApplicationDbContext.cs - new Identity EF Core context
- PuppyWeightWatcher.Web/Data/ApplicationUser.cs - custom Identity user class
- PuppyWeightWatcher.Web/Components/Routes.razor - add AuthorizeRouteView
- PuppyWeightWatcher.Web/Components/Layout/MainLayout.razor - show auth state
- PuppyWeightWatcher.Web/Components/Pages/Account/ - Login, Register, Logout, ExternalLogin pages
- PuppyWeightWatcher.Web/Components/_Imports.razor - add auth using directives
- PuppyWeightWatcher.AppHost/AppHost.cs - wire identitydb to webfrontend
- PuppyWeightWatcher.AppHost/PuppyWeightWatcher.AppHost.csproj - no change expected

## Risks & Open Questions
- External provider client IDs/secrets must be registered by the user with each provider (Microsoft Entra, GitHub, Google) and stored in user secrets — placeholder configuration will be provided
- A new EF Core migration must be generated after all code changes for the Identity schema
- Existing pages that were open to everyone will now require login

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-14 14:22:03

## 📝 Plan Steps
- ✅ **Add NuGet packages to the Web project for Identity, EF Core SQL Server, Aspire SQL Server integration, and external OAuth providers**
- ✅ **Create ApplicationUser model in PuppyWeightWatcher.Web/Data/ApplicationUser.cs**
- ✅ **Create ApplicationDbContext in PuppyWeightWatcher.Web/Data/ApplicationDbContext.cs**
- ✅ **Update AppHost.cs to add an identitydb database and wire it to the webfrontend project**
- ✅ **Update Web Program.cs to register Identity services, authentication middleware, EF Core context, and external providers**
- ✅ **Create Account Blazor pages: Login.razor, Register.razor, Logout.razor, and ExternalLogin.razor in Components/Pages/Account/**
- ✅ **Update _Imports.razor to add authorization using directives**
- ✅ **Update Routes.razor to use AuthorizeRouteView and redirect unauthenticated users**
- ✅ **Update MainLayout.razor to display logged-in username and logout link in the top bar**
- ✅ **Generate the EF Core Identity migration for the new ApplicationDbContext**
- ✅ **Build the solution and verify compilation succeeds**

