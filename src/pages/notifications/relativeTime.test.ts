import { relativeTime } from "./relativeTime";

const now = new Date("2026-03-10T12:00:00Z");
const ago = (ms: number) => new Date(now.getTime() - ms).toISOString();
const MIN = 60000;
const HOUR = 60 * MIN;
const DAY = 24 * HOUR;

describe("relativeTime", () => {
  it("under a minute", () => {
    expect(relativeTime(ago(59000), now)).toBe("rétt í þessu");
  });
  it("minutes", () => {
    expect(relativeTime(ago(MIN), now)).toBe("fyrir 1 mín.");
    expect(relativeTime(ago(59 * MIN), now)).toBe("fyrir 59 mín.");
  });
  it("hours", () => {
    expect(relativeTime(ago(HOUR), now)).toBe("fyrir 1 klst.");
    expect(relativeTime(ago(23 * HOUR + 59 * MIN), now)).toBe("fyrir 23 klst.");
  });
  it("yesterday", () => {
    expect(relativeTime(ago(DAY), now)).toBe("í gær");
    expect(relativeTime(ago(2 * DAY - MIN), now)).toBe("í gær");
  });
  it("days under a week", () => {
    expect(relativeTime(ago(2 * DAY), now)).toBe("fyrir 2 dögum");
    expect(relativeTime(ago(7 * DAY - MIN), now)).toBe("fyrir 6 dögum");
  });
  it("older shows the date", () => {
    const d = new Date(now.getTime() - 7 * DAY);
    expect(relativeTime(d.toISOString(), now)).toMatch(/^\d+\. \S+$/);
    expect(relativeTime("2026-03-03T12:00:00Z", now)).toBe(
      `${new Date("2026-03-03T12:00:00Z").getDate()}. mar.`
    );
  });
});
