import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';
import { ErrorBoundary } from "react-error-boundary";
import { Post } from "../../components";
import UserImage from "../../components/users/userimage";
import DinnerMenu from "./dinnermenu";
import { isMobile } from "react-device-detect";
import Teams from "./teams";
import { Link, useParams } from "react-router-dom";
import Preview from "../albums/preview";

const DinnerParty = () => {
    const params = useParams();
    const [dinner, setDinner] = useState();

    useEffect(() => {
        const getDinner = async () => {
            var url = config.get("apiPath") + "/api/dinnerparties/" + params.id;
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
    }, [])

    return (
        <div id="main">
            <ErrorBoundary
                FallbackComponent={<div>Það vantar einhvern hressleika hér!</div>}
            >
                {dinner ?
                    [
                        <Post key={dinner.id}
                            id={dinner.id}
                            href={"/dinnerparties/" + dinner.id}
                            title={dinner.name}
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
                                    <br />

                                    {dinner.albums && dinner.albums.length > 0 ?
                                    <h3>Myndir</h3>
                                    : null }
                                    {dinner.albums.map(album =>
                                        <div className="row gtr-uniform" key={album.id}>
                                            <div className={isMobile ? "col-12" : "col-4"} key={album.id}>
                                                <h4>
                                                    <Link to={"/album/" + album.id}>{album.name}</Link>
                                                </h4>
                                                {album.description}
                                            </div>
                                            <div className={isMobile ? "col-12" : "col-8"} key={album.id}>
                                                <Preview url={album.href} />
                                            </div>
                                            <p/>
                                        </div>
                                    )}
                                </section>
                            ]}
                        />,
                        <DinnerMenu key={"Menu" + dinner.id} id={dinner.id} />,
                        <Teams key={"Team" + dinner.id} id={dinner.id} />
                    ]
                    : null
                }
            </ErrorBoundary>
        </div>
    )
}

export default DinnerParty;