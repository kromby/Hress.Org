import { renderHook } from "@testing-library/react";
import { BEER_ALBUM_ID, useHressPhotos } from "./hress";
import { useImages } from "../../hooks/useImages";
import { useAlbums } from "../../hooks/useAlbums";
import { ImageContainer } from "../../types/image";

jest.mock("../../hooks/useImages");
jest.mock("../../hooks/useAlbums");
jest.mock("react-global-configuration", () => ({
  get: () => "http://localhost:7072",
}));

describe("useHressPhotos", () => {
  const uploadImage = jest.fn();
  const addImage = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useImages as jest.Mock).mockReturnValue({ uploadImage });
    (useAlbums as jest.Mock).mockReturnValue({ addImage });
  });

  it("uploadPhoto stores the photo in the album container and returns its ID", async () => {
    uploadImage.mockResolvedValue({ id: 279080 });
    const { result } = renderHook(() => useHressPhotos());
    const file = new File(["x"], "can.jpg", { type: "image/jpeg" });

    await expect(result.current.uploadPhoto(file, "Jóla Gull")).resolves.toBe(
      279080
    );
    expect(uploadImage).toHaveBeenCalledWith({
      file,
      name: "Jóla Gull",
      container: ImageContainer.Album,
    });
  });

  it("uploadPhoto throws when the upload returns no ID", async () => {
    uploadImage.mockResolvedValue(null);
    const { result } = renderHook(() => useHressPhotos());

    await expect(
      result.current.uploadPhoto(new File(["x"], "can.jpg"), "Jóla Gull")
    ).rejects.toThrow();
  });

  it("addToAlbum adds the image to album 46153", async () => {
    addImage.mockResolvedValue({});
    const { result } = renderHook(() => useHressPhotos());

    await result.current.addToAlbum(279080);

    expect(BEER_ALBUM_ID).toBe(46153);
    expect(addImage).toHaveBeenCalledWith(46153, 279080);
  });
});
