import { useEffect } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { Post } from "../../components";
import { useAuth } from "../../context/auth";
import { useMyNews } from "../../hooks/useMyNews";

const MyNews = () => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const { data: news, isLoading, error } = useMyNews();

  useEffect(() => {
    document.title = "Mínar fréttir | Hress.Org";
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
    }
  }, [authTokens]);

  const renderBody = () => {
    if (isLoading) {
      return <p>Sæki fréttir...</p>;
    }
    if (error) {
      return <p>Ekki tókst að sækja fréttir.</p>;
    }
    if (!news || news.length === 0) {
      return <p>Þú hefur ekki skrifað neinar fréttir.</p>;
    }
    return (
      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Nafn</th>
              <th>Dagsetning</th>
              <th>Lestrar</th>
              <th>Opinber</th>
            </tr>
          </thead>
          <tbody>
            {news.map((item) => (
              <tr key={item.id}>
                <td>
                  <Link to={`/news/${item.id}`}>{item.name}</Link>
                </td>
                <td>{item.insertedString}</td>
                <td>{item.hits ?? 0}</td>
                <td>{item.public ? "✓" : ""}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  };

  return (
    <div id="main">
      <Post title="Mínar fréttir" body={renderBody()} />
    </div>
  );
};

export default MyNews;
