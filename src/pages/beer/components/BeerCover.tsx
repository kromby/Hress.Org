import { hressImageUrl } from "../hress";
import { Beer } from "../types";

// Cans and bottles are tall, so the box is about 9:16.
const sizes = {
  small: { width: 56, height: 100 },
  large: { width: 96, height: 170 },
};

const placeholderStyle = {
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  border: "1px dashed rgba(160, 160, 160, 0.6)",
  borderRadius: "6px",
  background: "#f4f4f2",
  color: "#aaa",
  fontFamily: '"Raleway", Helvetica, sans-serif',
  fontSize: "9px",
  letterSpacing: "0.15em",
};

interface BeerCoverProps {
  beer: Beer;
  size?: keyof typeof sizes;
}

// The beer's cover photo, or a "MYND" placeholder if it has none.
const BeerCover = ({ beer, size = "small" }: BeerCoverProps) => {
  const { width, height } = sizes[size];

  if (!beer.imageID) {
    return (
      <div
        data-testid="beer-cover-placeholder"
        aria-hidden="true"
        style={{ ...placeholderStyle, width, height }}
      >
        MYND
      </div>
    );
  }

  return (
    <img
      src={hressImageUrl(beer.imageID, width * 2)}
      alt={beer.name}
      style={{
        display: "block",
        width,
        maxHeight: height,
        objectFit: "contain",
      }}
    />
  );
};

export default BeerCover;
