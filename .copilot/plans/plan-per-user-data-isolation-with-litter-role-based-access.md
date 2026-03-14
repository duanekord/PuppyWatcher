# 🎯 Per-User Data Isolation with Litter Role-Based Access

## Understanding
Implement per-user data isolation where litters have role-based access (Owner, CoOwner, Viewer). Owners can do everything, CoOwners can do everything except delete litters/puppies, Viewers are read-only. Puppies without a litter are visible only to their creator. A user management section on each litter allows owners and co-owners to manage members.

## Assumptions
- UserId is the user's email (consistent across local and Microsoft login)
- The Web frontend passes UserId to the API via an `X-User-Id` HTTP header
- Puppies without a litter are only visible to their OwnerId (the creator)
- Puppies in a litter inherit access from the litter's membership
- When a litter is created, the creator automatically becomes the Owner
- The "My Puppies" page shows: standalone puppies owned by the user + puppies in litters where the user is a member
- The "My Litters" page shows only litters where the user is a member
- Inviting a user is done by email address

## Approach
A new `LitterMember` junction table links litters to users with a `LitterRole` enum (Owner, CoOwner, Viewer). A new `OwnerId` column on `Puppy` tracks who created standalone puppies. All API service methods gain a `userId` parameter, and queries are filtered accordingly. The Web frontend's `PuppyApiClient` adds an `X-User-Id` header on every request using a delegating handler. API endpoints extract this header and pass it to the service layer. Permission checks happen in the service layer. Blazor pages receive the user's role for each litter and conditionally render edit/delete controls.

## Key Files
- PuppyWeightWatcher.Shared/Models/LitterMember.cs - new model with LitterId, UserId, Role
- PuppyWeightWatcher.Shared/Models/LitterRole.cs - new enum (Owner, CoOwner, Viewer)
- PuppyWeightWatcher.Shared/Models/Puppy.cs - add OwnerId property
- PuppyWeightWatcher.Shared/Models/Litter.cs - add UserRole property (not persisted, populated per-request)
- PuppyWeightWatcher.ApiService/Data/PuppyDbContext.cs - add LitterMembers DbSet and configuration
- PuppyWeightWatcher.ApiService/Services/PuppyService.cs - user-filtered queries and permission checks
- PuppyWeightWatcher.ApiService/Program.cs - extract X-User-Id header, pass to service, add member management endpoints
- PuppyWeightWatcher.Web/PuppyApiClient.cs - add X-User-Id header, add member management methods
- PuppyWeightWatcher.Web/Program.cs - register delegating handler for userId header
- PuppyWeightWatcher.Web/Components/Pages/LitterDetail.razor - add Members section, conditional controls

## Risks & Open Questions
- Existing data in the database has no OwnerId or LitterMember records — migration needs to handle this gracefully (nullable OwnerId for existing puppies)
- The API service trusts the X-User-Id header from the Web frontend (acceptable since it's backend-to-backend communication within Aspire)

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-14 17:01:55

## 📝 Plan Steps
- ✅ **Create LitterRole enum and LitterMember model in PuppyWeightWatcher.Shared/Models**
- ✅ **Add OwnerId to Puppy model and UserRole to Litter model**
- ✅ **Update PuppyDbContext with LitterMembers DbSet and entity configuration**
- ✅ **Update IPuppyService interface to add userId parameter to all methods and add member management methods**
- ✅ **Update PuppyService implementation with user-filtered queries, permission checks, and member management**
- ✅ **Update API endpoints in Program.cs to extract X-User-Id header, pass userId to service, and add member management endpoints**
- ✅ **Update PuppyApiClient to send X-User-Id header via delegating handler and add member management methods**
- ✅ **Update Web Program.cs to register the delegating handler that injects the current user's email**
- ✅ **Update Blazor pages (Puppies, Litters, LitterDetail, PuppyDetail, AddEditLitter, AddEditPuppy) to pass userId and conditionally render controls based on role**
- ✅ **Add litter member management UI section to LitterDetail page**
- ✅ **Generate EF Core migration for the new LitterMember table and Puppy.OwnerId column**
- ✅ **Build the solution and verify compilation**

