import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import SidePost from "../../../components/sidepost";

const AwardsSidebar = () => {
  const [years, setYears] = useState<any[]>([]);

  useEffect(() => {
    const getYears = async () => {
      try {
        const url =
          config.get("path") + "/api/hardhead/years?code=" + config.get("code");
        const response = await axios.get(url);
        setYears(response.data.filter((year: any) => year.Hardhead));
      } catch (e) {
        console.error(e);
      }
    };

    if (!years) {
      getYears();
    }
  }, []);

  return (
    <section id="sidebar">
      <section>
        <ul className="posts">
          {years
            ? years.map((year) => (
                <li key={year.ID}>
                  <SidePost
                    key={year.ID}
                    title={"Árið " + year.Name}
                    href={"/hardhead/awards/year/" + year.ID}
                    dateString={"Harðhaus ársins " + year.Hardhead.Username}
                    image={
                      year.Hardhead
                        ? config.get("apiPath") +
                          year.Hardhead.ProfilePhoto.Href
                        : undefined
                    }
                    imageText={
                      year.Hardhead
                        ? "Harðhaus ársins: " + year.Hardhead.Username
                        : undefined
                    }
                  />
                </li>
              ))
            : null}
        </ul>
      </section>
    </section>
  );
};

export default AwardsSidebar;
