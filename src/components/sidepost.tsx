import { Link } from "react-router-dom";

const SidePost = ({
  title,
  href,
  date,
  dateString,
  image,
  imageText,
}: {
  title: string;
  href: string;
  date?: string;
  dateString?: string;
  image?: string;
  imageText?: string;
}) => {
  return (
    <article>
      <header>
        <h3>
          <Link to={href}>{title}</Link>
        </h3>
        {dateString ? (
          <time className="published" dateTime={date}>
            {dateString}
          </time>
        ) : null}
      </header>
      {image ? (
        <Link to={href} className="image">
          <img src={image} alt={imageText} />
        </Link>
      ) : null}
    </article>
  );
};

export default SidePost;
