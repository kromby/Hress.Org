import { QueryClient } from "@tanstack/react-query";
import { clearNotificationCache } from "./clearNotificationCache";

it("removes the notification list and status but leaves other queries", () => {
  const client = new QueryClient();
  client.setQueryData(["notifications"], { notifications: [] });
  client.setQueryData(["notifications", "status", "jwt"], { hasUnread: true });
  client.setQueryData(["news"], [1]);
  clearNotificationCache(client);
  expect(client.getQueryData(["notifications"])).toBeUndefined();
  expect(
    client.getQueryData(["notifications", "status", "jwt"])
  ).toBeUndefined();
  expect(client.getQueryData(["news"])).toEqual([1]);
});
