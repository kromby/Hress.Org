# Design Spec: Hardhead Night Guest Removal & Guest Dropdown Filtering

## Overview
Currently, on a Hardhead night edit page, users can add guests to a night, but there is no mechanism to remove an added guest. Additionally, the guest selection dropdown includes all users, allowing the host to be added as a guest or allowing duplicate guest additions.

This spec details:
1. Adding support for removing a guest from a Hardhead night (backend & frontend).
2. Filtering out the host and existing guests from the dropdown list of candidate guests.
3. Adding backend validation to prevent adding the host as a guest.

---

## Requirements

### Frontend (`src/pages/hardhead/`)
1. **Dropdown Filtering**:
   - Exclude the host of the Hardhead night (`hardhead.host.id`) from the available users dropdown.
   - Exclude users who are already added as guests for the night from the dropdown.
2. **Guest Removal UI**:
   - On each guest avatar card in `GuestsEdit` (`src/pages/hardhead/components/guestsEdit.js`), display a top-right 'X' remove button.
   - Clicking 'X' calls `DELETE /api/hardhead/{hardheadID}/guests/{guestID}` with the user's auth token.
   - Upon successful removal, automatically refresh the guest list.

### Backend (`api/Ez.Hress.FunctionsApi/` & `api/Ez.Hress.Hardhead/`)
1. **`IHardheadDataAccess` & `HardheadSqlAccess`**:
   - Declare `Task<int> RemoveGuest(int hardheadID, int guestID)` in `IHardheadDataAccess`.
   - Wire `RemoveGuest` method in `HardheadSqlAccess` (which executes `DELETE FROM [dbo].[rep_User] WHERE TypeId = 52 AND eventID = @hardheadID AND userID = @guestID`).
2. **`HardheadInteractor`**:
   - Implement `RemoveGuestAsync(int hardheadID, int guestID)` forwarding to `_hardheadDataAccess.RemoveGuest`.
   - Update `AddGuestAsync(int hardheadID, int guestId, int userId)`:
     - Fetch the hardhead night via `GetHardheadAsync(hardheadID)` and verify `guestId != night.Host.ID`.
     - Throw `ArgumentException` if attempting to add the host as a guest.
3. **`HardheadGuestFunctions`**:
   - Implement `HttpMethods.IsDelete(req.Method)` in `RunPostDel`:
     - Validate JWT token.
     - Call `_hardheadInteractor.RemoveGuestAsync(id, guestId)`.
     - Return `200 OK` on success, or `404 Not Found` if 0 rows were affected.

---

## GitHub Issue Template / Description

```markdown
### Title
feat(hardhead): support removing guests and filter host/existing guests from dropdown

### Summary
Add guest removal capability to Hardhead night edit page and ensure the host and already-added guests cannot be selected as guests.

### Key Changes Needed
- **Backend**:
  - Implement `DELETE /api/hardhead/{id}/guests/{guestId}` HTTP trigger in `HardheadGuestFunctions.cs`.
  - Expose `RemoveGuestAsync` in `HardheadInteractor` and `IHardheadDataAccess`.
  - Prevent adding host as guest in `HardheadInteractor.AddGuestAsync`.
  - Add unit tests in `Ez.Hress.UnitTest`.
- **Frontend**:
  - Pass `hostId` prop to `<GuestsEdit />` from `HardheadEdit`.
  - Filter out host and existing guests from `<select>` dropdown in `GuestsEdit`.
  - Add top-right 'X' remove button on guest cards with `axios.delete(...)`.

### Acceptance Criteria
- [ ] Users can remove an existing guest by clicking the 'X' button on their card.
- [ ] Dropdown only displays users who are neither the host nor already added as guests.
- [ ] Backend returns HTTP 400 Bad Request if trying to add host as guest.
- [ ] All C# unit tests pass.
```

---

## Testing Plan

1. **Backend Unit Tests**:
   - Test `RemoveGuestAsync` calls data access layer.
   - Test `AddGuestAsync` throws `ArgumentException` when `guestId == host.ID`.
2. **Frontend Component & Integration**:
   - Verify guest list dropdown filtering.
   - Verify `DELETE` HTTP call when remove button is clicked.
