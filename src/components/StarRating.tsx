import React, { useState } from "react";

interface StarRatingProps {
  rating: number;
  starRatedColor: string;
  starHoverColor: string;
  starEmptyColor: string;
  changeRating: (rating: number) => void;
  numberOfStars: number;
  starDimension: string;
  starSpacing: string;
  readonly?: boolean;
  onHoverChange?: (rating: number) => void;
}

const StarRating: React.FC<StarRatingProps> = ({
  rating,
  starRatedColor,
  starHoverColor,
  starEmptyColor,
  changeRating,
  numberOfStars,
  starDimension,
  starSpacing,
  readonly = false,
  onHoverChange,
}) => {
  const [hoverRating, setHoverRating] = useState(0);

  const getStarColor = (starPosition: number) => {
    if (hoverRating >= starPosition && !readonly) {
      return starHoverColor;
    }
    if (rating >= starPosition) {
      return starRatedColor;
    }
    return starEmptyColor;
  };

  const handleMouseEnter = (starPosition: number) => {
    if (!readonly) {
      setHoverRating(starPosition);
      onHoverChange?.(starPosition);
    }
  };

  const handleMouseLeave = () => {
    if (!readonly) {
      setHoverRating(0);
      onHoverChange?.(0);
    }
  };

  const handleClick = (starPosition: number) => {
    if (!readonly) {
      changeRating(starPosition);
    }
  };

  const starStyle = {
    width: starDimension,
    height: starDimension,
    marginRight: starSpacing,
    cursor: readonly ? "default" : "pointer",
  };

  return (
    <div style={{ display: "inline-flex", alignItems: "center" }}>
      {[...Array(numberOfStars)].map((_, index) => {
        const starPosition = index + 1;
        return (
          <svg
            key={index}
            style={starStyle}
            onClick={() => handleClick(starPosition)}
            onMouseEnter={() => handleMouseEnter(starPosition)}
            onMouseLeave={handleMouseLeave}
            viewBox="0 0 25 25"
            fill={getStarColor(starPosition)}
          >
            <path d="M12 .587l3.668 7.568 8.332 1.151-6.064 5.828 1.48 8.279-7.416-3.967-7.417 3.967 1.481-8.279-6.064-5.828 8.332-1.151z" />
          </svg>
        );
      })}
    </div>
  );
};

export default StarRating;
