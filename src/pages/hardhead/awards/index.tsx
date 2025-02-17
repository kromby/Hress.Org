import { useEffect } from "react";
import { Post } from "../../../components";
import AwardsWinners from "./awardsWinners";
import { useAwards } from "../../../hooks/hardhead/useAwards";

const Awards = () => {
  const { awards, error, isLoading } = useAwards();

  useEffect(() => {
    document.title = "Harðhausaverðlaunin | Hress.Org";
  }, []);

  return (
    <div id="main">
      {isLoading ? <div className="loading">Sæki verðlaun...</div> : null}
      {error ? (
        <div className="error">Villa kom upp við að sækja verðlaun</div>
      ) : null}
      {awards
        ? awards.map((award) => (
            <Post
              key={award.id}
              id={award.id}
              href={`/hardhead/awards/${award.id}`}
              title={award.name}
              description="Sigurvegarar frá upphafi"
              body={
                <AwardsWinners
                  href={award.winners.href}
                  position={1}
                  year={undefined}
                />
              }
            />
          ))
        : null}
    </div>
  );
};

export default Awards;
