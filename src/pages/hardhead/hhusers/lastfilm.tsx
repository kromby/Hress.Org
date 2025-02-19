import config from "react-global-configuration";
import { MiniPost } from "../../../components";
import { useHardhead } from "../../../hooks/hardhead/useHardhead";
import { HardheadNight } from "../../../types/hardheadNight";
import { useEffect, useState } from "react";

const LastFilm = ({ userId }: { userId: number }) => {
  const { fetchByUserId } = useHardhead();
  const [hardheads, setHardheads] = useState<HardheadNight[]>([]);

  useEffect(() => {
    const fetchHardheads = async () => {
      const result = await fetchByUserId(userId);
      setHardheads(result.filter((h: HardheadNight) => h.movie));
    };
    fetchHardheads();
  }, [userId]);

  if (hardheads.length === 0) {
    return null;
  }

  const hardhead = hardheads[0];

  return (
    <div>
      {hardhead?.movie ? (
        <MiniPost
          title={"Nýjasta harðhausakvöld: " + hardhead.number}
          description={
            <span>
              {hardhead.guestCount + " gestir"}
              <br />
              <br />
              {hardhead.description
                ? hardhead.description
                : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
            </span>
          }
          date={hardhead.date}
          dateString={hardhead.dateString}
          imageSource={
            hardhead.movie.posterPhoto?.href
              ? `${config.get("apiPath")}${
                  hardhead.movie.posterPhoto.href
                }?width=410`
              : undefined
          }
        />
      ) : null}
    </div>
  );
};

export default LastFilm;
