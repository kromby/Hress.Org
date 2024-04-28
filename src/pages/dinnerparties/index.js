import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';
import { Post } from "../../components";

const DinnerParties = () => {
    const[dinners, setDinners] = useState();

    useEffect(() => {
        const getDinners = async () => {
            const url = config.get("apiPath") + "/api/dinnerparties?top=4&includeGuests=true";
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
    }, [])

    function getAssistants (guests) {
        if(guests.length === 0) {
            return "Að þessu sinni voru engir aðstoðarkokkar."
        }
        if(guests.length === 1) {
            return guests[0].role + " í þetta skiptið var " + guests[0].name + ".";
        } 
        if(guests.length === 2) {
            return "Aðstoðarkokkar í þetta skiptið voru " + guests[0].name + " og " + guests[1].name + ".";
        }
        else {
            return "Aðstoðarkokkar voru " + guests.map(guest => guest.name).join(", ") + ".";
        }
    }

    return (
        <div id="main">
            {dinners ? dinners.map(dinner =>
                <Post key={dinner.id}
                    id={dinner.id}
                    href={"/dinnerparties/" + dinner.id}
                    title={dinner.name}
                    description={"Kvöld númer " + dinner.number}
                    author={dinner.guests[0] ? dinner.guests[0].user : null}
                    date={dinner.date}
                    dateFormatted={dinner.dateString}
                    image={dinner.coverImage ? config.get('apiPath') + dinner.coverImage.href : null}
                    body={
                        <section>
                            <p>
                                Þetta kvöld var haldið {dinner.location[0].toLowerCase()}{dinner.location.substring(1)} og gestirnir voru {dinner.guestCount}.
                                {dinner.theme ? <br/> : null}
                                {dinner.theme ? " Þema kvöldsins var " + dinner.theme[0].toLowerCase() + dinner.theme.substring(1) + "." : null}
                                <br/>
                                {getAssistants(dinner.guests)} 
                            </p>
                        </section>
                    }
                />
            ) : null}
        </div>
    )
}

export default DinnerParties;