import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { AuthContext } from "../../../context/auth";
import PushBanner from "./PushBanner";
import { disablePush, enablePush, getPushState } from "../push";

jest.mock("../push");

const mockGetState = getPushState as jest.Mock;
const mockEnable = enablePush as jest.Mock;
const mockDisable = disablePush as jest.Mock;

const renderBanner = () =>
  render(
    <AuthContext.Provider
      value={{ authTokens: { token: "tok" }, setAuthTokens: jest.fn() } as any}
    >
      <PushBanner />
    </AuthContext.Provider>
  );

describe("PushBanner", () => {
  beforeEach(() => jest.resetAllMocks());

  it("renders nothing when unsupported", async () => {
    mockGetState.mockResolvedValue("unsupported");
    const { container } = renderBanner();
    await waitFor(() => expect(mockGetState).toHaveBeenCalled());
    expect(container).toBeEmptyDOMElement();
  });

  it("explains home screen install on iOS", async () => {
    mockGetState.mockResolvedValue("ios-install");
    renderBanner();
    expect(await screen.findByText(/Add to Home Screen/)).toBeInTheDocument();
    expect(screen.queryByRole("button")).toBeNull();
  });

  it("explains blocked notifications", async () => {
    mockGetState.mockResolvedValue("blocked");
    renderBanner();
    expect(await screen.findByText(/læstar í stillingum/)).toBeInTheDocument();
  });

  it("off shows Kveikja; clicking enables and shows the on line", async () => {
    mockGetState.mockResolvedValueOnce("off").mockResolvedValueOnce("on");
    mockEnable.mockResolvedValue(undefined);
    renderBanner();
    fireEvent.click(await screen.findByRole("button", { name: "Kveikja" }));
    expect(mockEnable).toHaveBeenCalledWith("tok");
    expect(
      await screen.findByText("Kveikt á tilkynningum í þessu tæki")
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Slökkva" })).toBeInTheDocument();
  });

  it("shows an error and the Kveikja button when enabling fails", async () => {
    mockGetState.mockResolvedValue("off");
    mockEnable.mockRejectedValue(new Error("nope"));
    renderBanner();
    fireEvent.click(await screen.findByRole("button", { name: "Kveikja" }));
    expect(await screen.findByRole("alert")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Kveikja" })).toBeEnabled();
  });

  it("on: clicking Slökkva calls disablePush", async () => {
    mockGetState.mockResolvedValueOnce("on").mockResolvedValueOnce("off");
    mockDisable.mockResolvedValue(undefined);
    renderBanner();
    fireEvent.click(await screen.findByRole("button", { name: "Slökkva" }));
    expect(mockDisable).toHaveBeenCalledWith("tok");
    expect(
      await screen.findByRole("button", { name: "Kveikja" })
    ).toBeInTheDocument();
  });
});
