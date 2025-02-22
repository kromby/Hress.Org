import { useState, useEffect } from "react";
import config from "react-global-configuration";
import MiniPost from "../../../components/minipost";
import HardheadActions from "./actions";
import { useHardhead } from "../../../hooks/hardhead/useHardhead";

const NextHardhead = ({ allowEdit }) => {
  const { fetchNextHardhead } = useHardhead();
  const [editEnabled, setEditEnabled] = useState(false);
  const [hardhead, setHardhead] = useState(null);

  useEffect(() => {
    const loadNextHardhead = async () => {
      try {
        const data = await fetchNextHardhead();
        setHardhead(data[0]);
      } catch (error) {
        console.error("Failed to fetch next hardhead:", error);
      }
    };

    loadNextHardhead();
    setEditEnabled(allowEdit);
  }, [allowEdit]);

  return (
    <div>
      {hardhead ? (
        <MiniPost
          title="Næsta harðhausakvöld"
          description={
            <span>
              {hardhead.host.username}
              <br />
              <br />
              {editEnabled ? <HardheadActions id={hardhead.id} /> : null}
            </span>
          }
          dateString={hardhead.dateString}
          date={hardhead.date}
          userHref={"/hardhead/users/" + hardhead.host.id}
          userPhoto={config.get("apiPath") + hardhead.host.profilePhoto.href}
          userText={hardhead.host.username}
        />
      ) : null}
    </div>
  );
};

export default NextHardhead;
