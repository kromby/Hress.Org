import { ChangeEvent, useEffect, useRef, useState } from "react";
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
  onUploadingChange?: (uploading: boolean) => void;
};

const imageUrl = (id: number, size: string) =>
  `${config.get("apiPath")}/api/images/${id}/content?${size}`;

const SelectedImage = ({
  imageId,
  imageAlign,
  onImageChange,
  onAlignChange,
}: Omit<NewsImagePickerProps, "onUploadingChange"> & {
  imageId: number;
}) => (
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

type ImageSource = "upload" | "album";

const SourceSelect = ({
  value,
  hasImage,
  onChange,
}: {
  value: ImageSource;
  hasImage: boolean;
  onChange: (source: ImageSource) => void;
}) => (
  <div className="col-12">
    <label htmlFor="news-image-source">
      {hasImage ? "Skipta um mynd" : "Bæta við mynd"}
    </label>
    <select
      id="news-image-source"
      value={value}
      onChange={(e) => onChange(e.target.value as ImageSource)}
    >
      <option value="upload">Hlaða upp mynd</option>
      <option value="album">Velja úr albúmi</option>
    </select>
  </div>
);

const UploadInput = ({
  uploading,
  onChange,
}: {
  uploading: boolean;
  onChange: (event: ChangeEvent<HTMLInputElement>) => void;
}) => (
  <div className="col-12">
    <label htmlFor="news-image-file">Mynd af tölvu</label>
    <input
      id="news-image-file"
      type="file"
      accept="image/*"
      onChange={onChange}
      disabled={uploading}
    />
    {uploading && <p>Hleð upp...</p>}
  </div>
);

const NewsImagePicker = ({
  onImageChange,
  onUploadingChange,
  ...props
}: NewsImagePickerProps) => {
  const { imageId } = props;
  const [source, setSource] = useState<ImageSource>("upload");
  const [message, setMessage] = useState("");
  const { uploadImage, loading: uploading, error: uploadError } = useImages();
  // Bumped on every selection, so an upload that finishes after a newer choice is ignored.
  const selection = useRef(0);

  useEffect(() => {
    onUploadingChange?.(uploading);
  }, [uploading]);

  const selectImage = (id?: number) => {
    selection.current += 1;
    onImageChange(id);
  };

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

    selection.current += 1;
    const uploadSelection = selection.current;
    try {
      const uploaded = await uploadImage({
        file,
        name: file.name,
        container: ImageContainer.News,
      });
      if (uploaded?.id && uploadSelection === selection.current) {
        onImageChange(uploaded.id);
      }
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
      {imageId ? (
        <SelectedImage
          {...props}
          imageId={imageId}
          onImageChange={selectImage}
        />
      ) : null}

      <div className="row gtr-uniform">
        <SourceSelect
          value={source}
          hasImage={Boolean(imageId)}
          onChange={setSource}
        />
        {source === "upload" ? (
          <UploadInput uploading={uploading} onChange={handleFileChange} />
        ) : (
          <AlbumImagePicker imageId={imageId} onImageChange={selectImage} />
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
