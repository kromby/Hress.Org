import React, { useRef, useState } from "react";

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
  // Exposes the stars as keyboard-operable radios. The caller must wrap them in a radiogroup.
  asRadio?: boolean;
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
  asRadio = false,
}) => {
  const [hoverRating, setHoverRating] = useState(0);
  const starRefs = useRef<(SVGSVGElement | null)[]>([]);
  const isRadio = asRadio && !readonly;

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

  // Arrow keys move focus between stars, Enter or Space picks the focused one.
  const handleKeyDown = (
    event: React.KeyboardEvent<SVGSVGElement>,
    starPosition: number
  ) => {
    const focusStar = (position: number) =>
      starRefs.current[position - 1]?.focus();

    switch (event.key) {
      case "ArrowRight":
      case "ArrowUp":
        event.preventDefault();
        focusStar(Math.min(starPosition + 1, numberOfStars));
        break;
      case "ArrowLeft":
      case "ArrowDown":
        event.preventDefault();
        focusStar(Math.max(starPosition - 1, 1));
        break;
      case "Home":
        event.preventDefault();
        focusStar(1);
        break;
      case "End":
        event.preventDefault();
        focusStar(numberOfStars);
        break;
      case "Enter":
      case " ":
        event.preventDefault();
        handleClick(starPosition);
        break;
    }
  };

  // Only one star is in the tab order: the rated one, or the first.
  const tabStop = rating >= 1 && rating <= numberOfStars ? rating : 1;

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
            {...(isRadio && {
              ref: (el: SVGSVGElement | null) => {
                starRefs.current[index] = el;
              },
              role: "radio",
              "aria-checked": rating === starPosition,
              "aria-label": `${starPosition} af ${numberOfStars}`,
              tabIndex: starPosition === tabStop ? 0 : -1,
              onKeyDown: (event: React.KeyboardEvent<SVGSVGElement>) =>
                handleKeyDown(event, starPosition),
            })}
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
