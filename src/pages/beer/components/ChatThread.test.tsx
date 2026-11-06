import { render, screen, within } from "@testing-library/react";
import axios from "axios";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import ChatThread from "./ChatThread";
import { AuthContext } from "../../../context/auth";
import { ChatMessage } from "../types";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

const messagesUrl =
  "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers/b1/messages";

// The API returns messages oldest first. The backend omits null values,
// so a member without a profile photo has no authorImageID.
const messages: ChatMessage[] = [
  {
    id: 1,
    userID: 7,
    username: "kromby",
    authorImageID: 55,
    text: "Fyrsta smökkun.\nSætur en góður.",
    created: "2026-11-04T20:15:00Z",
  },
  {
    id: 2,
    userID: 8,
    username: "gummi",
    text: "<img src=x onerror=alert(1)>",
    created: "2026-11-05T18:30:00Z",
  },
];

describe("ChatThread", () => {
  let queryClient: QueryClient;

  const renderComponent = () =>
    render(
      <QueryClientProvider client={queryClient}>
        <AuthContext.Provider value={{ authTokens: { token: "fake-token" } }}>
          <ChatThread seasonID="jolabjor-2026" beerID="b1" />
        </AuthContext.Provider>
      </QueryClientProvider>
    );

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    (axios.get as jest.Mock).mockResolvedValue({ data: messages });
  });

  it("fetches the beer's messages with the Hress token", async () => {
    renderComponent();
    await screen.findAllByTestId("chat-message");

    expect(axios.get).toHaveBeenCalledWith(messagesUrl, {
      headers: { "X-Custom-Authorization": "token fake-token" },
    });
  });

  it("renders messages oldest first with author and time", async () => {
    renderComponent();

    const rows = await screen.findAllByTestId("chat-message");
    expect(rows).toHaveLength(2);
    expect(within(rows[0]).getByText("kromby")).toBeInTheDocument();
    expect(within(rows[1]).getByText("gummi")).toBeInTheDocument();
    expect(rows[0].querySelector("time")).toHaveAttribute(
      "dateTime",
      "2026-11-04T20:15:00Z"
    );
    expect(rows[0].querySelector("time")).toHaveTextContent("2026");
  });

  it("keeps line breaks in the text", async () => {
    renderComponent();

    const rows = await screen.findAllByTestId("chat-message");
    const text = within(rows[0]).getByText(/Fyrsta smökkun/);
    expect(text.textContent).toBe("Fyrsta smökkun.\nSætur en góður.");
    expect(text).toHaveStyle({ whiteSpace: "pre-wrap" });
  });

  it("renders HTML in a message as literal text", async () => {
    const { container } = renderComponent();

    expect(
      await screen.findByText("<img src=x onerror=alert(1)>")
    ).toBeInTheDocument();
    expect(container.querySelector("img")).toBeNull();
  });

  it("says so when nobody has posted yet", async () => {
    (axios.get as jest.Mock).mockResolvedValue({ data: [] });
    renderComponent();

    expect(await screen.findByTestId("chat-empty")).toHaveTextContent(
      "Engin skilaboð enn."
    );
  });

  it("shows an error when the chat can't be fetched", async () => {
    (axios.get as jest.Mock).mockRejectedValue(new Error("Network Error"));
    renderComponent();

    expect(await screen.findByTestId("chat-error")).toBeInTheDocument();
  });
});
