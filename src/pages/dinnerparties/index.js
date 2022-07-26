import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';
import { Post } from "../../components";

const DinnerParties = (propsData) => {
    const[dinners, setDinners] = useState();

    useEffect(() => {
        const getDinners = async () => {
            var url = config.get("apiPath") + "/api/dinnerparties";
            try {
                const response = await axios.get(url);
                setDinners(response.data);
            }
            catch(e) {
                console.error(e);
            }
        }

        document.title = "Matar- og Rauðvínskvöld | Hress.Org";

        if(!dinners) {
            getDinners();
        }    
    }, [propsData])

    return (
        <div id="main">
            {dinners ? dinners.map(dinner =>
                <Post key={dinner.id}
                    id={dinner.id}
                    href={"/dinnerparties/" + dinner.id}
                    title={dinner.name}
                    description={"Kvöld númer " + dinner.number}
                    date={dinner.date}
                    dateFormatted={dinner.dateString}
                    image={dinner.coverImage ? config.get('apiPath') + dinner.coverImage.href : null}
                    body={
                        <p>
                            Prump
                        </p>
                    }
                />
            ) : null}
        </div>
    )
}

export default DinnerParties;