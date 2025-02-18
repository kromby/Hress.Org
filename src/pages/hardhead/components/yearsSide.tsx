import { useState, useEffect } from "react";
import config from "react-global-configuration";
import SidePost from "../../../components/sidepost";
import { useHardheadYears } from "../../../hooks/hardhead/useHardheadYears";

const YearsSide = () => {
  const { years, error, isLoading } = useHardheadYears();

  if (isLoading) {
    return <div className="loading">Sæki Harðhausa árin...</div>;
  }

  if (error) {
    return <div className="error">Villa við að sækja ár: {error.message}</div>;
  }

  return (
    <div>
      {years
        ? years.map((year) => (
            <li key={year.id}>
              <SidePost
                title={`Árið ${year.name}`}
                href={`/hardhead?parentID=${year.id}`}
                dateString={
                  year.hardhead?.username
                    ? `Harðhaus ársins ${year.hardhead?.username ?? ""}`
                    : undefined
                }
                image={
                  year.photo?.href
                    ? `${config.get("apiPath")}${year.photo.href}?width=75`
                    : undefined
                }
                imageText={`Árið ${year.name}`}
              />
            </li>
          ))
        : null}
    </div>
  );
};

export default YearsSide;
