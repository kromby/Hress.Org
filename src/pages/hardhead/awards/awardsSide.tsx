import { useState, useEffect } from "react";
import config from "react-global-configuration";
import { MiniPost } from "../../../components";
import axios from "axios";
import { WinnerEntity } from "../../../types/winnerEntity";

const AwardsSide = () => {
  const [data, setData] = useState({ isLoading: false, visible: false });
  const [awards, setAwards] = useState<WinnerEntity>();

  useEffect(() => {
    const getAwards = async () => {
      try {
        const url = `${config.get(
          "apiPath"
        )}/api/hardhead/awards/364/winners?position=1`;
        setData({ isLoading: true, visible: false });
        const response = await axios.get(url);
        setData({ isLoading: false, visible: true });
        setAwards(response.data[0]);
      } catch (e) {
        console.error(e);
        setData({ isLoading: false, visible: false });
      }
    };

    if (!awards) {
      getAwards();
    }
  }, []);

  return (
    <div>
      {data.visible && awards ? (
        <MiniPost
          title="Harðhausa verðlaunin"
          href="/hardhead/awards"
          description={
            <span>
              Harðhaus ársins
              <br />
              {awards.winner.username} með {awards.value} atkvæði
            </span>
          }
          date={`1.1.${awards.year}`}
          dateString={awards.year.toString()}
          userHref={`/hardhead/users/${awards.winner.id}`}
          userPhoto={`${config.get("path")}${
            awards.winner.profilePhoto?.href
          }?code=${config.get("code")}`}
          userText={awards.winner.username}
        />
      ) : null}
    </div>
  );
};

export default AwardsSide;
