import { Console } from "console";
import { MovieInfo as MovieInfoType } from "../../../types/movieInfo";

interface MovieInfoProps {
  movieInfo?: MovieInfoType | null;
}

const getCrewRoleName = (role: string) => {
  console.info("[getCrewRoleName] role: ", role);

  if (role === "Director") {
    return "leikstjóri";
  }
  if (role === "Writer") {
    return "handritshöfundur";
  }
  if (role === "Actor") {
    return "leikari";
  }

  return "annað";
};

const MovieInfo = ({ movieInfo }: MovieInfoProps) => {
  if (!movieInfo) {
    return null;
  }

  return (
    <>
      <h4>Nánari upplýsingar</h4>
      <div className="row" style={{ marginBottom: "1em" }}>
        {movieInfo?.genre?.length && movieInfo.genre.length > 0 ? (
          <div className="col-4">
            <strong>Tegund</strong>
            <br />
            {movieInfo.genre.join(", ")}
          </div>
        ) : null}
        {movieInfo?.language?.length && movieInfo.language.length > 0 ? (
          <div className="col-4">
            <strong>Tungumál</strong>
            <br />
            {movieInfo.language.join(", ")}
          </div>
        ) : null}
        {movieInfo?.country && (
          <div className="col-4">
            <strong>Land</strong>
            <br />
            {movieInfo.country}
          </div>
        )}
        {movieInfo?.boxOffice ? (
          <div className="col-4">
            <strong>Tekjur</strong>
            <br />
            {movieInfo.boxOffice}
          </div>
        ) : null}
        {movieInfo?.rated ? (
          <div className="col-4">
            <strong>Aldursflokkun</strong>
            <br />
            {movieInfo.rated}
          </div>
        ) : null}
        {movieInfo?.awards ? (
          <div className="col-4">
            <strong>Verðlaun</strong>
            <br />
            {movieInfo.awards}
          </div>
        ) : null}
      </div>
      {movieInfo?.ratings ? <h4>Einkunnir</h4> : null}
      <div className="row" style={{ marginBottom: "1em" }}>
        {movieInfo?.ratings && Object.keys(movieInfo.ratings).length > 0
          ? Object.entries(movieInfo.ratings).map(([key, value]) => (
              <div key={key} className="col-4">
                <strong>{key}</strong>
                <br />
                {value}
                {key === "Internet Movie Database" && movieInfo.imdbVotes
                  ? ` (${movieInfo.imdbVotes} atkvæði)`
                  : null}
              </div>
            ))
          : null}
      </div>
      {movieInfo.description ? (
        <>
          <h4>Lýsing</h4>
          <p>{movieInfo.description}</p>
        </>
      ) : null}
      {movieInfo?.crew?.length && movieInfo.crew.length > 0 ? (
        <div className="row" style={{ marginBottom: "2em" }}>
          <h4 className="col-12">Kvikmyndagerðarfólk og leikarar</h4>
          {movieInfo.crew
            .sort((a, b) => a.role.localeCompare(b.role))
            .map(({ role, name }) => (
              <div key={`${name}-${role}`} className="col-4">
                {name} - {getCrewRoleName(role)}
              </div>
            ))}
        </div>
      ) : null}
    </>
  );
};

export default MovieInfo;
