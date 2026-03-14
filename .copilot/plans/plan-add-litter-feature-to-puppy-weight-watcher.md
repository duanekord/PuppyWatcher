# 🎯 Add Litter Feature to Puppy Weight Watcher

## Understanding
Add the concept of a "Litter" to group puppies together. A litter has shared details (date of birth, breed) that are inherited by puppies added to it. Existing puppies can also be assigned to a litter. Users should be able to create a litter, manage its puppies, and add new puppies directly from a litter (with DOB/breed pre-filled).

## Assumptions
- A puppy can belong to zero or one litter (optional LitterId FK)
- Litter has: Name, DateOfBirth, Breed, and optional notes/description
- When creating a puppy from a litter, DOB and Breed are pre-filled from the litter
- Existing puppies (with no litter) can be added to a litter, which optionally updates their DOB/breed
- Deleting a litter does NOT delete its puppies — it just unsets their LitterId
- The litter list page becomes the new landing page, with "My Puppies" showing all puppies

## Approach
Work bottom-up: model → database → service → API → client → UI.

Create a `Litter` model in the Shared project. Add `LitterId` to `Puppy`. Update the DbContext with the new entity and relationship. Add litter CRUD methods to IPuppyService/PuppyService. Add API endpoints. Add client methods. Create Blazor pages for litter list, litter detail (showing its puppies), and litter create/edit. Update the AddEditPuppy page to accept an optional LitterId parameter to pre-fill fields. Add navigation.

## Key Files
- PuppyWeightWatcher.Shared/Models/Litter.cs - New model
- PuppyWeightWatcher.Shared/Models/Puppy.cs - Add LitterId FK
- PuppyWeightWatcher.ApiService/Data/PuppyDbContext.cs - New entity config
- PuppyWeightWatcher.ApiService/Services/PuppyService.cs - Litter CRUD methods
- PuppyWeightWatcher.ApiService/Program.cs - Litter API endpoints
- PuppyWeightWatcher.Web/PuppyApiClient.cs - Litter client methods
- PuppyWeightWatcher.Web/Components/Pages/Litters.razor - Litter list page
- PuppyWeightWatcher.Web/Components/Pages/LitterDetail.razor - Litter detail page
- PuppyWeightWatcher.Web/Components/Pages/AddEditLitter.razor - Create/edit litter page
- PuppyWeightWatcher.Web/Components/Pages/AddEditPuppy.razor - Accept LitterId param
- PuppyWeightWatcher.Web/Components/Layout/NavMenu.razor - Add litter nav link

## Risks & Open Questions
- Existing database needs schema update (new table + new column on Puppies) — EnsureCreatedAsync won't add columns to existing tables; may need to recreate DB or use migrations

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-14 05:00:45

## 📝 Plan Steps
- ✅ **Create the Litter model in the Shared project**
- ✅ **Add optional LitterId foreign key to the Puppy model**
- ✅ **Update PuppyDbContext with Litter entity configuration and Puppy-Litter relationship**
- ✅ **Add litter CRUD methods to IPuppyService interface and PuppyService implementation**
- ✅ **Add litter API endpoints in Program.cs**
- ✅ **Add litter client methods in PuppyApiClient.cs**
- ✅ **Create the Litters list page (Litters.razor)**
- ✅ **Create the AddEditLitter page (AddEditLitter.razor)**
- ✅ **Create the LitterDetail page showing litter info and its puppies with ability to add/remove puppies**
- ✅ **Update AddEditPuppy.razor to accept optional LitterId parameter and pre-fill DOB/breed from litter**
- ✅ **Add Litters navigation link to NavMenu.razor**
- ✅ **Build and verify all changes compile successfully**

