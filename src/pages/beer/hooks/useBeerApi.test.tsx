import { act, renderHook, waitFor } from "@testing-library/react";
import axios from "axios";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactNode } from "react";
import {
  DuplicateBeerError,
  seasonBeersKey,
  useAddBeerToSeason,
  useCatalogSearch,
  useCreateBeer,
} from "./useBeerApi";
import { useDebouncedValue } from "./useDebouncedValue";
import { AuthContext } from "../../../context/auth";

jest.mock("axios");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

describe("beer catalog hooks", () => {
  let queryClient: QueryClient;

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider
        value={{
          authTokens: { token: "fake-token" },
          setAuthTokens: jest.fn(),
        }}
      >
        {children}
      </AuthContext.Provider>
    </QueryClientProvider>
  );

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
  });

  it("useCatalogSearch encodes the query", async () => {
    (axios.get as jest.Mock).mockResolvedValue({ data: [] });
    renderHook(() => useCatalogSearch("Jóla & Co"), { wrapper });

    await waitFor(() => expect(axios.get).toHaveBeenCalled());
    expect((axios.get as jest.Mock).mock.calls[0][0]).toMatch(
      /beer\/beers\?query=J%C3%B3la%20%26%20Co$/
    );
  });

  it("useCatalogSearch does not fetch for one character", async () => {
    renderHook(() => useCatalogSearch("J"), { wrapper });

    await new Promise((resolve) => setTimeout(resolve, 20));
    expect(axios.get).not.toHaveBeenCalled();
  });

  it("useAddBeerToSeason posts the beer ID and refreshes the season list", async () => {
    (axios.post as jest.Mock).mockResolvedValue({ data: { id: "31696" } });
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useAddBeerToSeason("jolabjor-2026"), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync("31696");
    });

    expect(axios.post).toHaveBeenCalledWith(
      "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers",
      { beerID: "31696" },
      expect.anything()
    );
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: seasonBeersKey("jolabjor-2026"),
      refetchType: "all",
    });
  });

  it("useCreateBeer posts the beer and refreshes the season list", async () => {
    (axios.post as jest.Mock).mockResolvedValue({ data: { id: "abc" } });
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useCreateBeer("jolabjor-2026"), {
      wrapper,
    });
    const input = {
      name: "Jóla Gull",
      producer: "Ölgerðin",
      country: "Ísland",
      alcoholPercentage: 5.4,
    };

    await act(async () => {
      await result.current.mutateAsync(input);
    });

    expect(axios.post).toHaveBeenCalledWith(
      "http://localhost:7072/api/beer/seasons/jolabjor-2026/beers",
      { beer: input },
      expect.anything()
    );
    expect(invalidate).toHaveBeenCalledWith({
      queryKey: seasonBeersKey("jolabjor-2026"),
      refetchType: "all",
    });
  });

  it("useCreateBeer turns a 409 duplicate into DuplicateBeerError", async () => {
    (axios.post as jest.Mock).mockRejectedValue({
      response: {
        status: 409,
        data: { duplicate: { id: "1", name: "Jóla Kaldi" } },
      },
    });
    const { result } = renderHook(() => useCreateBeer("s"), { wrapper });

    let error: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync({
          name: "Jóla Kaldi",
          producer: "p",
          country: "c",
          alcoholPercentage: 5,
        });
      } catch (e) {
        error = e;
      }
    });

    expect(error).toBeInstanceOf(DuplicateBeerError);
    expect((error as DuplicateBeerError).existing.id).toBe("1");
  });

  it("useCreateBeer passes other errors through", async () => {
    (axios.post as jest.Mock).mockRejectedValue({
      response: { status: 409, data: "Season 'x' is closed." },
    });
    const { result } = renderHook(() => useCreateBeer("s"), { wrapper });

    let error: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync({
          name: "n",
          producer: "p",
          country: "c",
          alcoholPercentage: 5,
        });
      } catch (e) {
        error = e;
      }
    });

    expect(error).toBeDefined();
    expect(error).not.toBeInstanceOf(DuplicateBeerError);
  });

  it("useDebouncedValue only updates after the delay", () => {
    jest.useFakeTimers();
    try {
      const { result, rerender } = renderHook(
        ({ value }) => useDebouncedValue(value, 300),
        { initialProps: { value: "a" } }
      );
      rerender({ value: "ab" });

      act(() => {
        jest.advanceTimersByTime(299);
      });
      expect(result.current).toBe("a");

      act(() => {
        jest.advanceTimersByTime(1);
      });
      expect(result.current).toBe("ab");
    } finally {
      jest.useRealTimers();
    }
  });
});
