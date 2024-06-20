import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Post from "../../../../components/post";
import { useAuth } from "../../../../context/auth";
import HardheadRating from "../../components/rating";
import HardheadBody from "../../components/hardheadbody";

const NightOfTheYear = ({
  ID,
  Name,
  Href,
  Description,
  Date,
  Year,
  onSubmit,
}) => {
  const { authTokens } = useAuth();
  const [nights, setNights] = useState();

  useEffect(() => {
    const getHardheadUsers = async () => {
      try {
        const url = config.get("path") + Href + "&code=" + config.get("code");
        const response = await axios.get(url);
        setNights(response.data);
      } catch (e) {
        console.error(e);
        alert(e);
      }
    };

    if (!nights) {
      getHardheadUsers();
    }
  }, []);

  const handelSubmit = async () => {
    if (authTokens === undefined) {
      alert("Þú þarf að skrá þig inn");
      return;
    }

    try {
      const url = config.get("apiPath") + "/api/elections/" + ID + "/vote";
      await axios.post(url, [], {
        headers: { "X-Custom-Authorization": "token " + authTokens.token },
      });
    } catch (e) {
      console.error(e);
      alert(e);
    }

    onSubmit();
  };

  return (
    <div>
      <Post
        id={ID}
        title={Name}
        description={Description}
        date={Date}
        dateFormatted={Year}
        body={
          <section>
            <p>
              Gefðu öllum kvöldunum sem þú mættir á einkunn, smelltu síðan á{" "}
              <i>Ljúka</i> neðst á síðunni til að halda áfram
            </p>
          </section>
        }
      />

      {nights
        ? nights.map((hardhead) => (
            <Post
              key={hardhead.ID}
              id={hardhead.ID}
              title={hardhead.Name}
              description={hardhead.GuestCount + " gestir"}
              date={hardhead.Date}
              dateFormatted={hardhead.DateString}
              author={hardhead.Host}
              body={
                <HardheadBody
                  id={hardhead.ID}
                  name={hardhead.Name}
                  description={hardhead.Description}
                  viewMovie={false}
                  imageHeight={"270px"}
                />
              }
              actions={<ul className="actions" />}
              stats={
                <HardheadRating id={hardhead.ID} movieRatingVisible="false" />
              }
            />
          ))
        : null}

      <ul className="actions pagination">
        <li>
          <button onClick={handleSubmit} className="button large next">
            {"Ljúka (" + Name + ")"}
          </button>
        </li>
      </ul>
    </div>
  );
};

export default NightOfTheYear;
