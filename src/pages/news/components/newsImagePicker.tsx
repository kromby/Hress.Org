import { ChangeEvent, useState } from "react";
import config from "react-global-configuration";
import { useAlbumImages, useAlbumList } from "../../../hooks/useAlbumImages";
import { useImages } from "../../../hooks/useImages";
import { ImageContainer, ImageEntity } from "../../../types/image";
import "./newsImagePicker.css";

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

const IMAGE_ALIGN_OPTIONS = [
  { value: 1, label: "Vinstri" },
  { value: 2, label: "Hægri" },
  { value: 4, label: "Top" },
];

type NewsImagePickerProps = {
  imageId?: number;
  imageAlign: number;
  onImageChange: (imageId?: number) => void;
  onAlignChange: (imageAlign: number) => void;
};

const imageUrl = (id: number, size: string) =>
  `${config.get("apiPath")}/api/images/${id}/content?${size}`;

const SelectedImage = ({
  imageId,
  imageAlign,
  onImageChange,
  onAlignChange,
}: NewsImagePickerProps & { imageId: number }) => (
  <div className="row gtr-uniform">
    <div className="col-6 col-12-small">
      <span className="image fit">
        <img src={imageUrl(imageId, "width=400")} alt="Valin mynd" />
      </span>
    </div>
    <div className="col-6 col-12-small">
      <label htmlFor="news-image-align">Staðsetning myndar</label>
      <select
        id="news-image-align"
        value={imageAlign}
        onChange={(e) => onAlignChange(Number(e.target.value))}
      >
        {IMAGE_ALIGN_OPTIONS.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      <button
        type="button"
        className="button small news-image-remove"
        onClick={() => onImageChange()}
      >
        Fjarlægja mynd
      </button>
    </div>
  </div>
);

type AlbumImageGridProps = {
  images: ImageEntity[];
  imageId?: number;
  onImageChange: (imageId: number) => void;
};

const AlbumImageGrid = ({
  images,
  imageId,
  onImageChange,
}: AlbumImageGridProps) => (
  <ul className="news-image-grid">
    {images.map((image) => (
      <li key={image.id}>
        <button
          type="button"
          className={image.id === imageId ? "selected" : undefined}
          aria-pressed={image.id === imageId}
          onClick={() => onImageChange(image.id)}
        >
          <img
            src={imageUrl(image.id, "width=200&height=150")}
            alt={image.name || `Mynd ${image.id}`}
          />
        </button>
      </li>
    ))}
  </ul>
);

const AlbumImagePicker = ({
  imageId,
  onImageChange,
}: Pick<NewsImagePickerProps, "imageId" | "onImageChange">) => {
  const [albumId, setAlbumId] = useState<number>();
  const albums = useAlbumList();
  const albumImages = useAlbumImages(albumId);

  return (
    <div className="col-12">
      <label htmlFor="news-image-album">Albúm</label>
      <select
        id="news-image-album"
        value={albumId ?? ""}
        onChange={(e) =>
          setAlbumId(e.target.value ? Number(e.target.value) : undefined)
        }
      >
        <option value="">
          {albums.isLoading ? "Sæki albúm..." : "- Veldu albúm -"}
        </option>
        {albums.data?.map((album) => (
          <option key={album.id} value={album.id}>
            {album.name}
          </option>
        ))}
      </select>
      {albums.error && <div className="error">Ekki tókst að sækja albúm.</div>}
      {albumImages.isLoading && <p>Sæki myndir...</p>}
      {albumImages.error && (
        <div className="error">Ekki tókst að sækja myndir.</div>
      )}
      {albumImages.data && (
        <AlbumImageGrid
          images={albumImages.data}
          imageId={imageId}
          onImageChange={onImageChange}
        />
      )}
    </div>
  );
};

const NewsImagePicker = (props: NewsImagePickerProps) => {
  const { imageId, onImageChange } = props;
  const [source, setSource] = useState<"upload" | "album">("upload");
  const [message, setMessage] = useState("");
  const { uploadImage, loading: uploading, error: uploadError } = useImages();

  const handleFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const input = event.target;
    const file = input.files?.[0];
    if (!file) return;

    setMessage("");
    if (!file.type.startsWith("image/")) {
      setMessage("Aðeins er hægt að hlaða upp myndum");
      input.value = "";
      return;
    }
    if (file.size > MAX_FILE_SIZE) {
      setMessage(
        `Mynd er of stór. Hámarksstærð er ${MAX_FILE_SIZE / 1024 / 1024}MB`
      );
      input.value = "";
      return;
    }

    try {
      const uploaded = await uploadImage({
        file,
        name: file.name,
        container: ImageContainer.News,
      });
      if (uploaded?.id) onImageChange(uploaded.id);
    } catch (e) {
      // Shown through useImages' error state.
      console.error(e);
    } finally {
      input.value = "";
    }
  };

  return (
    <fieldset className="news-image-picker">
      <legend>Aðalmynd</legend>
      {imageId ? <SelectedImage {...props} imageId={imageId} /> : null}

      <div className="row gtr-uniform">
        <div className="col-12">
          <label htmlFor="news-image-source">
            {imageId ? "Skipta um mynd" : "Bæta við mynd"}
          </label>
          <select
            id="news-image-source"
            value={source}
            onChange={(e) => setSource(e.target.value as "upload" | "album")}
          >
            <option value="upload">Hlaða upp mynd</option>
            <option value="album">Velja úr albúmi</option>
          </select>
        </div>

        {source === "upload" ? (
          <div className="col-12">
            <label htmlFor="news-image-file">Mynd af tölvu</label>
            <input
              id="news-image-file"
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              disabled={uploading}
            />
            {uploading && <p>Hleð upp...</p>}
          </div>
        ) : (
          <AlbumImagePicker imageId={imageId} onImageChange={onImageChange} />
        )}

        {(message || uploadError) && (
          <div className="col-12">
            <div className="error">{message || uploadError}</div>
          </div>
        )}
      </div>
    </fieldset>
  );
};

export default NewsImagePicker;
