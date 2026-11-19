import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import NextHardhead from "./nexthardhead";

jest.mock("react-global-configuration", () => ({
  get: () => "http://api",
}));

const mockFetchNextHardhead = jest.fn();
jest.mock("../../../hooks/hardhead/useHardhead", () => ({
  useHardhead: () => ({ fetchNextHardhead: mockFetchNextHardhead }),
}));

const night = {
  id: 123,
  name: "Harðhaus #250",
  date: "2026-11-20T20:00:00",
  dateString: "20. nóvember 2026",
  number: 250,
  guestCount: 0,
  host: { id: 3, username: "Jónsi" },
  year: { id: 1, href: "/api/hardhead/1" },
};

const renderBox = () =>
  render(
    <MemoryRouter>
      <NextHardhead allowEdit={false} />
    </MemoryRouter>
  );

describe("NextHardhead", () => {
  it("shows the start time and location", async () => {
    mockFetchNextHardhead.mockResolvedValue([
      { ...night, startTimeString: "20:00", location: "Bláskógar 3" },
    ]);
    renderBox();
    expect(await screen.findByText(/kl\. 20:00/)).toBeInTheDocument();
    expect(screen.getByText(/Bláskógar 3/)).toBeInTheDocument();
  });

  it("leaves them out when not set", async () => {
    mockFetchNextHardhead.mockResolvedValue([
      { ...night, startTimeString: null, location: null },
    ]);
    renderBox();
    expect(await screen.findByText("Næsta harðhausakvöld")).toBeInTheDocument();
    expect(screen.queryByText(/kl\./)).not.toBeInTheDocument();
  });
});
