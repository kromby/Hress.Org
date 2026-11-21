const pad = (n: number) => String(n).padStart(2, "0");

/**
 * A Hardhead night's date and start time as wall-clock time ("2026-11-20T20:00:00"), with no time zone.
 * The night is in Iceland, so the time picked is the time it starts, whatever the browser's time zone is.
 * Sending a Date as JSON would convert it to UTC and shift it for anyone editing from abroad.
 */
export const toWallClockString = (date: Date): string =>
  `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
  `T${pad(date.getHours())}:${pad(date.getMinutes())}:00`;
