import { useEffect } from "react";
import { Post } from "../../../components";
import AwardsWinners from "./awardsWinners";
import { useParams } from "react-router-dom";
import { useAwards } from "../../../hooks/hardhead/useAwards";

const AwardsByYear = () => {
  useEffect(() => {
    document.title = "Harðhausaverðlaunin eftir ári | Hress.Org";
  }, []);

  const params = useParams();
  const year = Number(params.id);
  const { awards, error, isLoading } = useAwards(
    !isNaN(year) && year > 0 ? year : undefined
  );

  return (
    <div id="main">
      {isLoading ? (
        <div className="loading">Sæki verðlaun...</div>
      ) : error ? (
        <div className="error">Villa kom upp við að sækja verðlaun</div>
      ) : awards ? (
        awards.map((award) => (
          <Post
            key={award.id}
            title={award.name}
            description="Efstu sætin þetta ár"
            body={
              <AwardsWinners
                href={award.winners.href}
                year={year}
                position={undefined}
              />
            }
          />
        ))
      ) : null}
    </div>
  );
};

export default AwardsByYear;
