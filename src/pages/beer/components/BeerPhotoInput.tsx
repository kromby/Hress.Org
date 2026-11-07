import { useEffect, useState } from "react";

const MAX_BYTES = 10 * 1024 * 1024;

// Hides the browser's file input but keeps it focusable; the label is the
// visible button.
const hiddenInput = {
  position: "absolute" as const,
  width: "1px",
  height: "1px",
  opacity: 0,
  overflow: "hidden",
};

interface BeerPhotoInputProps {
  file: File | null;
  onChange: (file: File | null) => void;
}

// Takes (or picks) a photo of the can and previews it.
const BeerPhotoInput = ({ file, onChange }: BeerPhotoInputProps) => {
  const [error, setError] = useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [focused, setFocused] = useState(false);

  useEffect(() => {
    if (!file || typeof URL.createObjectURL !== "function") {
      setPreviewUrl(null);
      return undefined;
    }
    const url = URL.createObjectURL(file);
    setPreviewUrl(url);
    return () => {
      if (typeof URL.revokeObjectURL === "function") URL.revokeObjectURL(url);
    };
  }, [file]);

  const pick = (picked: File | undefined) => {
    if (!picked) return;
    if (!picked.type.startsWith("image/")) {
      setError("Aðeins er hægt að hlaða upp myndum");
      onChange(null);
    } else if (picked.size > MAX_BYTES) {
      setError("Mynd er of stór. Hámarksstærð er 10MB");
      onChange(null);
    } else {
      setError(null);
      onChange(picked);
    }
  };

  return (
    <div>
      <input
        id="beer-photo"
        type="file"
        accept="image/*"
        capture="environment"
        // Lets the same file be picked again after removing it.
        key={file ? "has-file" : "no-file"}
        style={hiddenInput}
        onFocus={() => setFocused(true)}
        onBlur={() => setFocused(false)}
        onChange={(e) => pick(e.target.files?.[0])}
      />
      {/* The theme's button look, instead of the browser's "Browse…". */}
      <label
        htmlFor="beer-photo"
        className="button icon solid fa-camera"
        style={focused ? { outline: "2px solid currentColor" } : undefined}
      >
        Taka mynd af dósinni
      </label>
      {error && <p role="alert">{error}</p>}
      {file && previewUrl && (
        <p>
          <img
            src={previewUrl}
            alt="Mynd af dósinni"
            style={{ maxWidth: "100%", maxHeight: "16em" }}
          />
          <br />
          <button
            type="button"
            className="button small"
            onClick={() => {
              setError(null);
              onChange(null);
            }}
          >
            Fjarlægja mynd
          </button>
        </p>
      )}
    </div>
  );
};

export default BeerPhotoInput;
