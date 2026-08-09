# Hardhead Night Guest Removal & Dropdown Filtering Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow users to remove a guest from a Hardhead night and ensure the dropdown filters out the host and existing guests.

**Architecture:** Extend backend data access interface, interactor, and Azure Function HTTP trigger for `DELETE /api/hardhead/{id}/guests/{guestId}`. In the React frontend, pass host ID to `GuestsEdit`, filter available users in the dropdown, and add an 'X' button to guest cards to trigger deletion.

**Tech Stack:** C# .NET 9 (Azure Functions isolated worker, xUnit, Moq), React 19, Axios.

## Global Constraints
- React formatting: Prettier (2-space indent, double quotes, semicolons, ES5 trailing commas).
- API logging: Structured logging with `_log.LogInformation("[{Class}.{Method}] ...", nameof(Type), nameof(Method), ...)`.
- Dependencies: Use existing imports and patterns.

---

### Task 1: Backend API & Interactor Updates

**Files:**
- Modify: `api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs`
- Modify: `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs`
- Modify: `api/Ez.Hress.FunctionsApi/Hardhead/HardheadGuestFunctions.cs`
- Test: `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs`

**Interfaces:**
- Consumes: `HardheadSqlAccess.RemoveGuest(int hardheadID, int guestID)`
- Produces: `HardheadInteractor.RemoveGuestAsync(int hardheadID, int guestID)`, `DELETE /api/hardhead/{id:int}/guests/{guestId:int}`

- [ ] **Step 1: Write failing unit tests for RemoveGuestAsync and AddGuestAsync host validation**

Add tests to `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs`:

```csharp
[Fact]
public async Task RemoveGuestAsync_ValidInput_CallsDataAccess()
{
    // ARRANGE
    const int hardheadId = 10;
    const int guestId = 5;

    _hardheadDataAccess
        .Setup(d => d.RemoveGuest(hardheadId, guestId))
        .ReturnsAsync(1);

    // ACT
    var result = await _interactor.RemoveGuestAsync(hardheadId, guestId);

    // ASSERT
    Assert.Equal(1, result);
    _hardheadDataAccess.Verify(d => d.RemoveGuest(hardheadId, guestId), Times.Once);
}

[Fact]
public async Task AddGuestAsync_GuestIsHost_ThrowsArgumentException()
{
    // ARRANGE
    const int hardheadId = 10;
    const int hostId = 5;
    const int userId = 1;

    _hardheadDataAccess
        .Setup(d => d.GetHardhead(hardheadId))
        .ReturnsAsync(new HardheadNight(hardheadId, 1, new UserBasicEntity { ID = hostId }));

    // ACT & ASSERT
    await Assert.ThrowsAsync<ArgumentException>(() => _interactor.AddGuestAsync(hardheadId, hostId, userId));
    _hardheadDataAccess.Verify(d => d.AddGuest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test api/Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~HardheadInteractorTests"`
Expected: FAIL due to missing `RemoveGuestAsync` method on `HardheadInteractor` and missing host check.

- [ ] **Step 3: Implement IHardheadDataAccess, HardheadInteractor, and HardheadGuestFunctions**

1. Update `api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs`:
Add method signature:
```csharp
Task<int> RemoveGuest(int hardheadID, int guestID);
```

2. Update `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs`:
Add `RemoveGuestAsync` and update `AddGuestAsync`:
```csharp
public async Task<int> RemoveGuestAsync(int hardheadID, int guestID)
{
    _log.LogInformation("[{Class}.{Method}] Removing guest '{GuestID}' from Hardhead '{HardheadID}'", _class, nameof(RemoveGuestAsync), guestID, hardheadID);
    return await _hardheadDataAccess.RemoveGuest(hardheadID, guestID);
}

public async Task<int> AddGuestAsync(int id, int guestId, int userId)
{
    var night = await _hardheadDataAccess.GetHardhead(id);
    if (night.Host != null && night.Host.ID == guestId)
    {
        throw new ArgumentException("Geta ekki skráð gestgjafa sem gest.", nameof(guestId));
    }

    var list = await _hardheadDataAccess.GetGuests(id);

    if (list.Any(g => g.ID == guestId))
    {
        throw new SystemException("Guest already registered.");
    }

    return await _hardheadDataAccess.AddGuest(id, guestId, userId, DateTime.UtcNow);
}
```
*(Note: fix the existing line `GetGuests(guestId)` bug in `AddGuestAsync` to `GetGuests(id)` as well).*

3. Update `api/Ez.Hress.FunctionsApi/Hardhead/HardheadGuestFunctions.cs`:
Fill in `if(HttpMethods.IsDelete(req.Method))` inside `RunPostDel`:
```csharp
if(HttpMethods.IsDelete(req.Method))
{
    var result = await _hardheadInteractor.RemoveGuestAsync(id, guestId);
    if(result == 0)
        return new NotFoundResult();
    return new OkResult();
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test api/Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~HardheadInteractorTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs api/Ez.Hress.FunctionsApi/Hardhead/HardheadGuestFunctions.cs api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs
git commit -m "feat(api): implement guest deletion and host-as-guest validation"
```

---

### Task 2: Frontend Guest Dropdown Filtering & Removal UI

**Files:**
- Modify: `src/pages/hardhead/hardheadEdit.js`
- Modify: `src/pages/hardhead/components/guestsEdit.js`
- Create: `src/pages/hardhead/components/guestsEdit.test.js`

**Interfaces:**
- Consumes: `DELETE /api/hardhead/{id}/guests/{guestId}`
- Produces: Updated `<GuestsEdit />` component filtering host & existing guests and supporting guest deletion.

- [ ] **Step 1: Write failing component test for GuestsEdit**

Create `src/pages/hardhead/components/guestsEdit.test.js`:
```jsx
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import axios from "axios";
import GuestsEdit from "./guestsEdit";
import { AuthContext } from "../../../context/auth";
import { MemoryRouter } from "react-router-dom";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: jest.fn().mockReturnValue("http://localhost:7072"),
}));

describe("GuestsEdit", () => {
  const mockUsers = [
    { id: 1, name: "Host User" },
    { id: 2, name: "Existing Guest" },
    { id: 3, name: "Available Guest" },
  ];

  const mockGuests = [
    { id: 2, username: "Existing Guest", profilePhoto: null },
  ];

  const renderComponent = (props = {}) => {
    return render(
      <MemoryRouter>
        <AuthContext.Provider value={{ authTokens: { token: "fake-token" } }}>
          <GuestsEdit
            hardheadID={10}
            users={mockUsers}
            hostId={1}
            {...props}
          />
        </AuthContext.Provider>
      </MemoryRouter>
    );
  };

  beforeEach(() => {
    jest.clearAllMocks();
    axios.get.mockResolvedValue({ data: mockGuests });
  });

  it("filters out host and existing guests from dropdown options", async () => {
    renderComponent();

    await waitFor(() => {
      expect(axios.get).toHaveBeenCalledWith(
        "http://localhost:7072/api/hardhead/10/guests"
      );
    });

    const select = screen.getByRole("combobox");
    const options = Array.from(select.querySelectorAll("option")).map(
      (opt) => opt.textContent
    );

    expect(options).toContain("- Veldu gest? -");
    expect(options).toContain("Available Guest");
    expect(options).not.toContain("Host User");
    expect(options).not.toContain("Existing Guest");
  });

  it("calls delete endpoint when guest remove button is clicked", async () => {
    axios.delete.mockResolvedValueOnce({ status: 200 });

    renderComponent();

    await waitFor(() => {
      expect(screen.getByTitle("Fjarlægja gest")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByTitle("Fjarlægja gest"));

    await waitFor(() => {
      expect(axios.delete).toHaveBeenCalledWith(
        "http://localhost:7072/api/hardhead/10/guests/2",
        {
          headers: { "X-Custom-Authorization": "token fake-token" },
        }
      );
    });
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test src/pages/hardhead/components/guestsEdit.test.js -- --watchAll=false`
Expected: FAIL due to missing `hostId` prop filtering and missing remove button.

- [ ] **Step 3: Update HardheadEdit and GuestsEdit components**

1. Modify `src/pages/hardhead/hardheadEdit.js`:
Pass `hostId={hardhead.host?.id}` to `GuestsEdit`:
```jsx
<GuestsEdit key="edit3" hardheadID={hardhead.id} users={users} hostId={hardhead.host?.id} />
```

2. Modify `src/pages/hardhead/components/guestsEdit.js`:
Update `GuestsEdit` signature, filtering, and remove handler:
```jsx
import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { useAuth } from "../../../context/auth";
import UserImage from "../../../components/users/userimage";
import { useLocation, useNavigate } from "react-router-dom";

const GuestsEdit = ({ hardheadID, users, hostId }) => {
  const { authTokens } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [guests, setGuests] = useState();

  const getGuests = () => {
    const url = `${config.get("apiPath")}/api/hardhead/${hardheadID}/guests`;
    axios
      .get(url)
      .then((response) => {
        setGuests(response.data);
      })
      .catch((error) => {
        if (error.response?.status === 404) {
          console.log(
            "[GuestsEdit] Guests not found for Hardhead: ",
            hardheadID
          );
        } else {
          console.error(
            "[GuestsEdit] Error retrieving guests for Hardhead: ",
            hardheadID
          );
          console.error(error);
        }
      });
  };

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    if (!guests) {
      getGuests();
    }
  }, [hardheadID, authTokens]);

  const handleGuestChange = async (event) => {
    if (authTokens !== undefined && event.target.value) {
      event.preventDefault();
      try {
        const guestID = event.target.value;
        const url = `${config.get(
          "apiPath"
        )}/api/hardhead/${hardheadID}/guests/${guestID}`;
        await axios.post(
          url,
          {},
          {
            headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
          }
        );
        getGuests();
      } catch (e) {
        console.error("[GuestsEdit] Ekki tókst að bæta gest við.");
        console.error(e);
      }
    }
  };

  const handleRemoveGuest = async (guestID) => {
    if (authTokens !== undefined) {
      try {
        const url = `${config.get(
          "apiPath"
        )}/api/hardhead/${hardheadID}/guests/${guestID}`;
        await axios.delete(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        getGuests();
      } catch (e) {
        console.error("[GuestsEdit] Ekki tókst að fjarlægja gest.");
        console.error(e);
      }
    }
  };

  const availableUsers = users
    ? users.filter(
        (user) =>
          user.id !== hostId &&
          !guests?.some((guest) => guest.id === user.id)
      )
    : [];

  return (
    <section>
      <h3>Gestir</h3>
      <div className="row gtr-uniform">
        <div className="col-12">
          {users ? (
            <select
              id="demo-category"
              name="demo-category"
              onChange={handleGuestChange}
              value=""
            >
              <option value="">- Veldu gest? -</option>
              {availableUsers.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name}
                </option>
              ))}
            </select>
          ) : null}
        </div>
        {guests && guests.length > 0
          ? guests.map((guest) => (
              <div
                className="col-2 col-12-xsmall align-center"
                key={guest.id}
                style={{ position: "relative" }}
              >
                <button
                  type="button"
                  onClick={() => handleRemoveGuest(guest.id)}
                  title="Fjarlægja gest"
                  style={{
                    position: "absolute",
                    top: "-8px",
                    right: "4px",
                    background: "#e74c3c",
                    color: "#fff",
                    border: "none",
                    borderRadius: "50%",
                    width: "22px",
                    height: "22px",
                    lineHeight: "22px",
                    padding: 0,
                    cursor: "pointer",
                    fontSize: "12px",
                    fontWeight: "bold",
                    zIndex: 2,
                  }}
                >
                  &times;
                </button>
                <UserImage
                  id={guest.id}
                  username={guest.username}
                  profilePhoto={guest.profilePhoto?.href}
                />
              </div>
            ))
          : "Enginn skráður gestur"}
      </div>
    </section>
  );
};

export default GuestsEdit;
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test src/pages/hardhead/components/guestsEdit.test.js -- --watchAll=false`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/pages/hardhead/hardheadEdit.js src/pages/hardhead/components/guestsEdit.js src/pages/hardhead/components/guestsEdit.test.js
git commit -m "feat(hardhead): add guest removal UI and filter host and existing guests in dropdown"
```
