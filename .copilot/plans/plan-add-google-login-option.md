# 🎯 Add Google Login Option

## Understanding
The user wants to add Google as an external login provider, with a branded button placed above the existing Microsoft button on both the Login and Register pages. The Google auth package and configuration code already exist in `Program.cs`. The button should follow Google's branding guidelines from the reference page.

## Assumptions
- `Microsoft.AspNetCore.Authentication.Google` package is already installed (v10.0.5)
- Google auth configuration code already exists in `Program.cs` (lines 46-53) reading from `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret`
- The user will store Google credentials in Key Vault following the same pattern as Microsoft (`Authentication--Google--ClientId`, `Authentication--Google--ClientSecret`)
- Button styling follows Google branding: white background, Google "G" logo, dark text, similar to the custom graphic approach from the reference page

## Approach
Create a Google "G" logo SVG in [wwwroot/images](PuppyWeightWatcher.Web/wwwroot/images/), add `google-login-btn` CSS to [app.css](PuppyWeightWatcher.Web/wwwroot/app.css) matching the existing `ms-login-btn` pattern, and add Google-specific `@if` blocks above the Microsoft blocks in both [Login.razor](PuppyWeightWatcher.Web/Components/Pages/Account/Login.razor) and [Register.razor](PuppyWeightWatcher.Web/Components/Pages/Account/Register.razor).

## Key Files
- PuppyWeightWatcher.Web/wwwroot/images/google-logo.svg - Google "G" logo
- PuppyWeightWatcher.Web/wwwroot/app.css - Google button CSS
- PuppyWeightWatcher.Web/Components/Pages/Account/Login.razor - add Google button
- PuppyWeightWatcher.Web/Components/Pages/Account/Register.razor - add Google button

## Risks & Open Questions
- User must register a Google OAuth client ID at Google Cloud Console before the button works

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-15 02:14:53

## 📝 Plan Steps
- ✅ **Create Google "G" logo SVG in wwwroot/images/google-logo.svg**
- ✅ **Add Google login button CSS to app.css**
- ✅ **Add Google button above Microsoft button in Login.razor**
- ✅ **Add Google button above Microsoft button in Register.razor**
- ✅ **Build and verify compilation**

