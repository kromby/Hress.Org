import { useAwardWinnersById } from "../../../hooks/hardhead/useAwardWinners";

const UserAwardDetail = ({
  awardID,
  userID,
  name,
}: {
  awardID: number;
  userID: number;
  name: string;
}) => {
  const { winners, error, isLoading } = useAwardWinnersById(awardID, userID, 3);

  const POSITION_COLORS = {
    1: "#FBEE99", // Gold
    2: "#F5F5F5", // Silver
    3: "#FEE6E1", // Bronze
  };

  if (isLoading) {
    return <div>Sæki gögn...</div>;
  }

  if (error && error.response?.status !== 404) {
    return <div>Villa við að sækja verðlaun: {error.message}</div>;
  }

  return (
    <div>
      {winners ? (
        <div>
          <h2>{name}</h2>
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <td width="100px">Ár</td>
                  <td width="100px">Sæti</td>
                  <td width="100px">Atkvæði</td>
                  <td>Útskýring</td>
                </tr>
              </thead>
              <tbody>
                {winners
                  .filter((a) => a.position <= 3)
                  .map((winner) => (
                    <tr
                      key={winner.id}
                      style={
                        winner.position <= 3
                          ? {
                              backgroundColor:
                                POSITION_COLORS[winner.position as 1 | 2 | 3],
                            }
                          : undefined
                      }
                    >
                      <td>{winner.year}</td>
                      <td>{winner.position}</td>
                      <td>{winner.value}</td>
                      <td>{winner.text}</td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}
    </div>
  );
};

export default UserAwardDetail;
