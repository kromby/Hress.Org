import Author from "../../../components/author";
import { useAwardWinnersByHref } from "../../../hooks/hardhead/useAwardWinners";

const AwardsWinners = ({
  href,
  position,
  year,
}: {
  href: string;
  position?: number;
  year?: number;
}) => {
  const { winners, error, isLoading } = useAwardWinnersByHref(
    href,
    undefined,
    position,
    year
  );

  if (error) {
    return <div>Error: {error.message}</div>;
  }
  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="table-wrapper">
      <table>
        <thead>
          <tr>
            {position ? <td width="100px">Ár</td> : <td width="100px">Sæti</td>}
            <td width="200px">Harðhaus</td>
            <td width="100px">Atkvæði</td>
            <td>Útskýring</td>
          </tr>
        </thead>
        <tbody>
          {winners?.map((winner) => (
            <tr key={winner.id}>
              <td>{position ? winner.year : winner.position}</td>
              <td>
                <Author
                  id={winner.winner.id}
                  username={winner.winner.username ?? ""}
                  userPath="/hardhead/users/"
                  profilePhoto={winner.winner.profilePhoto?.href}
                />
              </td>
              <td>{winner.value}</td>
              <td>{winner.text}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AwardsWinners;
