import config from "react-global-configuration";
import { MiniPost } from "../../../components";
import { useAwardWinnersById } from "../../../hooks/hardhead/useAwardWinners";

const AwardsSide = () => {
  const { winners, error, isLoading } = useAwardWinnersById(364, undefined, 1);

  if (error) {
    return <div>Error: {error.message}</div>;
  }
  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      {winners && (
        <MiniPost
          title="Harðhausa verðlaunin"
          href="/hardhead/awards"
          description={
            <span>
              Harðhaus ársins
              <br />
              {winners[0]?.winner?.username} með {winners[0]?.value} atkvæði
            </span>
          }
          date={`1.1.${winners[0]?.year}`}
          dateString={winners[0]?.year?.toString()}
          userHref={`/hardhead/users/${winners[0]?.winner?.id}`}
          userPhoto={`${config.get("apiPath")}${
            winners[0]?.winner?.profilePhoto?.href
          }`}
          userText={winners[0]?.winner?.username}
        />
      )}
    </div>
  );
};

export default AwardsSide;
