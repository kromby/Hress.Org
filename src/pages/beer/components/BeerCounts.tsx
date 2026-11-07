import "./BeerCounts.css";

// "skilaboð" is the same in the singular and the plural.
const describeMessages = (count: number) => `${count} skilaboð`;
// Icelandic takes the singular for numbers ending in 1, except 11.
const describeRatings = (count: number) =>
  `${count} ${count % 10 === 1 && count % 100 !== 11 ? "einkunn" : "einkunnir"}`;

interface CountProps {
  icon: string;
  count: number;
  label: string;
  testID: string;
}

// An icon and the number, with the full label for screen readers and on hover.
const Count = ({ icon, count, label, testID }: CountProps) => (
  <li data-testid={testID} title={label}>
    <span className={`icon solid ${icon}`}>
      <span aria-hidden="true">{count}</span>
      <span className="beer-counts-label">{label}</span>
    </span>
  </li>
);

interface BeerCountsProps {
  messageCount: number;
  ratingCount: number;
}

// How many chat messages and ratings a beer has, styled like the theme's
// footer stats, for the meta column of the beer's post.
const BeerCounts = ({ messageCount, ratingCount }: BeerCountsProps) => (
  <ul className="beer-counts">
    <Count
      icon="fa-comment"
      count={messageCount}
      label={describeMessages(messageCount)}
      testID="message-count"
    />
    <Count
      icon="fa-star"
      count={ratingCount}
      label={describeRatings(ratingCount)}
      testID="rating-count"
    />
  </ul>
);

export default BeerCounts;
