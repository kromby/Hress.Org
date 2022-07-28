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
                        <section>
                            <p>
                                Þetta kvöld var haldið {dinner.location[0].toLowerCase()}{dinner.location.substring(1)} og gestirnir voru {dinner.guestCount}.
                                <br/>
                                {dinner.theme ? " Þema kvöldsins var " + dinner.theme[0].toLowerCase() + dinner.theme.substring(1) + "." : null}
                            </p>
                        </section>
                    }
                />
            ) : null}
        </div>
    )
}

export default DinnerParties;