import { fireEvent, render, screen, waitFor } from "@testing-library/react";
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

const posted: ChatMessage = {
  id: 3,
  userID: 7,
  username: "kromby",
  text: "Góður",
  created: "2026-11-06T19:00:00Z",
};

describe("ChatComposer", () => {
  let queryClient: QueryClient;
  const setAuthTokens = jest.fn();

  const renderChat = (isOpen = true) =>
    render(
      <QueryClientProvider client={queryClient}>
        <AuthContext.Provider
          value={{ authTokens: { token: "fake-token" }, setAuthTokens }}
        >
          <ChatThread seasonID="jolabjor-2026" beerID="b1" isOpen={isOpen} />
        </AuthContext.Provider>
      </QueryClientProvider>
    );

  const textbox = () => screen.getByRole("textbox", { name: /Skilaboð/ });
  const sendButton = () => screen.getByRole("button", { name: "Senda" });

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    // Empty before the post, the new message on the refetch after it.
    (axios.get as jest.Mock)
      .mockResolvedValueOnce({ data: [] })
      .mockResolvedValue({ data: [posted] });
    (axios.post as jest.Mock).mockResolvedValue({ data: posted });
  });

  it("posts the text with the Hress token, clears the box and shows the message", async () => {
    renderChat();
    await screen.findByTestId("chat-empty");

    fireEvent.change(textbox(), { target: { value: "Góður" } });
    fireEvent.click(sendButton());

    await waitFor(() => expect(textbox()).toHaveValue(""));
    expect(axios.post).toHaveBeenCalledWith(
      messagesUrl,
      { text: "Góður" },
      { headers: { "X-Custom-Authorization": "token fake-token" } }
    );
    expect(await screen.findByText("Góður")).toBeInTheDocument();
  });

  it("sends line breaks as they were typed", async () => {
    renderChat();
    await screen.findByTestId("chat-empty");

    fireEvent.change(textbox(), { target: { value: "Fyrsta\nÖnnur" } });
    fireEvent.click(sendButton());

    await waitFor(() =>
      expect(axios.post).toHaveBeenCalledWith(
        messagesUrl,
        { text: "Fyrsta\nÖnnur" },
        expect.anything()
      )
    );
  });

  it("send is disabled while the text is blank", async () => {
    renderChat();
    await screen.findByTestId("chat-empty");

    expect(sendButton()).toBeDisabled();
    fireEvent.change(textbox(), { target: { value: "   \n " } });
    expect(sendButton()).toBeDisabled();
    fireEvent.submit(screen.getByTestId("chat-composer"));

    expect(axios.post).not.toHaveBeenCalled();
  });

  it("limits the text to 2000 characters", async () => {
    renderChat();
    await screen.findByTestId("chat-empty");

    expect(textbox()).toHaveAttribute("maxLength", "2000");
  });

  it("keeps the text and shows an error when posting fails", async () => {
    (axios.post as jest.Mock).mockRejectedValue({ response: { status: 500 } });
    renderChat();
    await screen.findByTestId("chat-empty");

    fireEvent.change(textbox(), { target: { value: "Góður" } });
    fireEvent.click(sendButton());

    expect(await screen.findByTestId("chat-post-error")).toBeInTheDocument();
    expect(textbox()).toHaveValue("Góður");
    expect(setAuthTokens).not.toHaveBeenCalled();
  });

  it("clears an expired token when posting is unauthorized", async () => {
    (axios.post as jest.Mock).mockRejectedValue({ response: { status: 401 } });
    renderChat();
    await screen.findByTestId("chat-empty");

    fireEvent.change(textbox(), { target: { value: "Góður" } });
    fireEvent.click(sendButton());

    await waitFor(() => expect(setAuthTokens).toHaveBeenCalledWith());
  });

  it("has no message box when the season is closed", async () => {
    renderChat(false);
    await screen.findByTestId("chat-empty");

    expect(screen.queryByTestId("chat-composer")).toBeNull();
    expect(screen.queryByRole("textbox")).toBeNull();
    expect(screen.getByTestId("chat-closed")).toBeInTheDocument();
  });
});
