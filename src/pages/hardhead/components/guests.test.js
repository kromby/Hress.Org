import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import axios from "axios";
import Guests from "./guests";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

describe("Guests", () => {
  const guests = [
    { id: 1, username: "guest1", profilePhoto: { href: "photo1.jpg" } },
    { id: 2, username: "guest2", profilePhoto: { href: "photo2.jpg" } },
  ];

  const renderGuests = (props) =>
    render(
      <MemoryRouter>
        <Guests hardheadID={123} {...props} />
      </MemoryRouter>
    );

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("fetches and renders guests when none are passed in", async () => {
    axios.get.mockResolvedValueOnce({ data: guests });

    renderGuests();

    expect(await screen.findByText("guest1")).toBeInTheDocument();
    expect(screen.getByText("guest2")).toBeInTheDocument();
    expect(axios.get).toHaveBeenCalledWith(
      "http://localhost:7072/api/hardhead/123/guests"
    );
  });

  it("renders passed-in guests without calling the API", () => {
    renderGuests({ initialGuests: guests });

    expect(screen.getByText("guest1")).toBeInTheDocument();
    expect(screen.getByText("guest2")).toBeInTheDocument();
    expect(axios.get).not.toHaveBeenCalled();
  });

  it("does not call the API when the night has no guests", () => {
    renderGuests({ initialGuests: [] });

    expect(axios.get).not.toHaveBeenCalled();
  });
});
