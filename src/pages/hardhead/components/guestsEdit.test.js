import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import axios from "axios";
import GuestsEdit from "./guestsEdit";
import { AuthContext } from "../../../context/auth";
import { MemoryRouter } from "react-router-dom";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: (key) => "http://localhost:7072",
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
      const select = screen.getByRole("combobox");
      const options = Array.from(select.querySelectorAll("option")).map(
        (opt) => opt.textContent
      );
      expect(options).not.toContain("Existing Guest");
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
