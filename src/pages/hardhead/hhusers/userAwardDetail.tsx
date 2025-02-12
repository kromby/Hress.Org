import { useAwardWinners } from "../../../hooks/hardhead/useAwardWinners";

const UserAwardDetail = ({
  awardID,
  userID,
  name,
}: {
  awardID: number;
  userID: number;
  name: string;
}) => {
  const { winners, error, isLoading } = useAwardWinners(awardID, userID);

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
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
                        winner.position === 1
                          ? { backgroundColor: "#FBEE99" }
                          : winner.position === 2
                          ? { backgroundColor: "#F5F5F5" }
                          : winner.position === 3
                          ? { backgroundColor: "#FEE6E1" }
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
