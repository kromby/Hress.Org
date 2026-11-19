import { toWallClockString } from "./wallClock";

describe("toWallClockString", () => {
  it("keeps the picked date and time, with no time zone", () => {
    expect(toWallClockString(new Date(2026, 10, 20, 20, 0, 0))).toBe(
      "2026-11-20T20:00:00"
    );
  });

  it("pads single digits and drops seconds", () => {
    expect(toWallClockString(new Date(2026, 0, 5, 7, 5, 59))).toBe(
      "2026-01-05T07:05:00"
    );
  });

  it("keeps midnight as no start time", () => {
    expect(toWallClockString(new Date(2026, 10, 30))).toBe(
      "2026-11-30T00:00:00"
    );
  });
});
