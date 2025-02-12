import config from "react-global-configuration";
import { MiniPost } from "../../../components";
import { useAwardWinners } from "../../../hooks/hardhead/useAwardWinners";

const AwardsSide = () => {
  const { winners, error, isLoading } = useAwardWinners();

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
              {winners.winner.username} með {winners.value} atkvæði
            </span>
          }
          date={`1.1.${winners.year}`}
          dateString={winners.year.toString()}
          userHref={`/hardhead/users/${winners.winner.id}`}
          userPhoto={`${config.get("path")}${
            winners.winner.profilePhoto?.href
          }?code=${config.get("code")}`}
          userText={winners.winner.username}
        />
      )}
    </div>
  );
};

export default AwardsSide;
