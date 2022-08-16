import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';
import { ErrorBoundary } from "react-error-boundary";
import { Post } from "../../components";
import UserImage from "../../components/users/userimage";
import DinnerMenu from "./dinnermenu";
import { isMobile } from "react-device-detect";

const DinnerParty = (propsData) => {
    const [dinner, setDinner] = useState();

    useEffect(() => {
        const getDinner = async () => {
            var url = config.get("apiPath") + "/api/dinnerparties/" + propsData.match.params.id;
            try {
                const response = await axios.get(url);
                setDinner(response.data);
            }
            catch (e) {
                console.error(e);
            }
        }

        document.title = "Matar- og Rauðvínskvöld | Hress.Org";

        if (!dinner) {
            getDinner();
        }
    }, [propsData])

    function getAssistants(guests) {
        console.log("[dinnerparties] guest.length: ", guests.length);
        if (guests.length === 0) {
            return "Að þessu sinni voru engir aðstoðarkokkar."
        }
        if (guests.length === 1) {
            return guests[0].role + " í þetta skiptið var " + guests[0].name + ".";
        }
        if (guests.length === 2) {
            return "Aðstoðarkokkar í þetta skiptið voru " + guests[0].name + " og " + guests[1].name + ".";
        }
        else {
            return "Aðstoðarkokkar voru " + guests.map(guest => guest.name).join(", ") + ".";
        }
    }

    return (
        <div id="main">
            <ErrorBoundary
                FallbackComponent={<div>Það vantar einhvern hressleika hér!</div>}
                onError={(error, errorInfo) => errorService.log({ error, errorInfo })}
                onReset={() => {
                    // reset the state of your app so the error doesn't happen again
                }}
            >
                {dinner ?
                    [
                        <Post key={dinner.id}
                            id={dinner.id}
                            href={"/dinnerparties/" + dinner.id}
                            title={dinner.name}
                            // description={"Kvöld númer " + dinner.number}
                            author={dinner.guests[0] ? dinner.guests[0].user : null}
                            date={dinner.date}
                            dateFormatted={dinner.dateString}
                            image={dinner.coverImage ? config.get('apiPath') + dinner.coverImage.href : null}
                            body={[
                                <section key="One">
                                    <p>
                                        Þetta kvöld var haldið {dinner.location[0].toLowerCase()}{dinner.location.substring(1)} og gestirnir voru {dinner.guestCount}.
                                        {dinner.theme ? <br /> : null}
                                        {dinner.theme ? " Þema kvöldsins var " + dinner.theme[0].toLowerCase() + dinner.theme.substring(1) + "." : null}
                                    </p>
                                </section>,
                                <section key="two">
                                    <h3>Nefndin</h3>
                                    <div className="row gtr-uniform">
                                        {dinner.guests.filter(guest => guest.role === "Kokkur" || guest.role === "Aðstoðarkokkur" || guest.role === "Gæðastjóri").map(guest =>
                                            <div className={isMobile ? "col-6 align-center" : "col-2 align-center"} key={guest.id}>
                                                {guest.user.profilePhoto ?
                                                    <UserImage id={guest.user.id} username={guest.name} profilePhoto={guest.user.profilePhoto.href} /> :
                                                    <UserImage id={guest.user.id} username={guest.name} />
                                                }
                                                {guest.role}
                                            </div>
                                        )}
                                    </div>
                                    <br />

                                    <h3>Gestir</h3>
                                    <div className="row gtr-uniform">
                                        {dinner.guests.filter(guest => guest.role === "Gestur").map(guest =>
                                            <div className={isMobile ? "col-4 align-center" : "col-2 align-center"} key={guest.id}>
                                                {guest.user.profilePhoto ?
                                                    <UserImage id={guest.user.id} username={guest.name} profilePhoto={guest.user.profilePhoto.href} /> :
                                                    <UserImage id={guest.user.id} username={guest.name} />
                                                }
                                            </div>
                                        )}
                                    </div>
                                </section>
                            ]}
                        />,
                        <DinnerMenu key={"Menu" + dinner.id} id={dinner.id} />
                    ]
                    : null
                }
            </ErrorBoundary>
        </div>
    )
}

export default DinnerParty;