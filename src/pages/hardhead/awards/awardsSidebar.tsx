import config from "react-global-configuration";
import SidePost from "../../../components/sidepost";
import { useHardheadYears } from "../../../hooks/hardhead/useHardheadYears";
import { YearEntity } from "../../../types/yearEntity";

const AwardsSidebar = () => {
  const { years, error, isLoading } = useHardheadYears();

  if (isLoading) {
    return <div className="loading">Sæki Harðhausa árin...</div>;
  }

  if (error) {
    return <div className="error">Villa við að sækja ár: {error.message}</div>;
  }

  return (
    <section id="sidebar">
      <section>
        <ul className="posts">
          {years
            .filter((year: YearEntity) => year.hardhead)
            .map((year) => (
              <li key={year.id}>
                <SidePost
                  title={`Árið ${year.name}`}
                  href={`/hardhead/awards/year/${year.id}`}
                  dateString={`Harðhaus ársins ${
                    year.hardhead?.username ?? ""
                  }`}
                  image={
                    year.hardhead?.profilePhoto?.href
                      ? `${config.get("apiPath")}${
                          year.hardhead.profilePhoto.href
                        }?width=75&height=75`
                      : undefined
                  }
                  imageText={`Harðhaus ársins ${year.hardhead?.username ?? ""}`}
                />
              </li>
            ))}
        </ul>
      </section>
    </section>
  );
};

export default AwardsSidebar;
