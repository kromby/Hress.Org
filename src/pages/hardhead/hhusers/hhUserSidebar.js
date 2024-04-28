import { useEffect, useState } from "react"
import axios from "axios";
import config from 'react-global-configuration';
import { MiniPost } from '../../../components';
import LastFilm from "./lastfilm";
import { Helmet } from "react-helmet";
import { useParams } from "react-router-dom";

const HHUserSidebar = () => {
    const [user, setUser] = useState();
    const [stats, setStats] = useState();
    const params = useParams();

    useEffect(() => {
        const getUser = async () => {
            const url = config.get('path') + '/api/users/' + params.id + '/?code=' + config.get('code');

            try {
                const response = await axios.get(url);
                setUser(response.data);
                document.title = response.data.Name + " | Hress.Org";
            } catch (e) {
                console.error(e);
            }
        }

        const getStats = async () => {
            const url = config.get('apiPath') + '/api/hardhead/statistics/users/' + params.id + '?periodType=All';

            try {
                const response = await axios.get(url);
                setStats(response.data.list[0]);
            } catch (e) {
                console.error(e);
            }
        }

        

        if (!user) {
            getUser();
        }

        if (!stats) {
            getStats();
        }
    }, [])

    return (
        <Helmet key="helmet">
            <title>{user ? user.Name : null} | Hress.Org</title>
            <meta name="description" content={user ? user.Name : null} />
            <meta property="og:title" content={user ? user.Name : null} />
            <meta property="og:image" content={user?.ProfilePhoto ? config.get("apiPath") + user.ProfilePhoto.Href : null} />
            <meta property="og:image:secure_url" content={user?.ProfilePhoto ? config.get("apiPath") + user.ProfilePhoto.Href : null} />
        </Helmet>,
        <section id="sidebar">
            <section id="intro">
                <a href="#" className="logo"><img src={user?.ProfilePhoto ? config.get("apiPath") + user.ProfilePhoto.Href + "?width=80&height=80" : null} alt="" /></a>
                <header>
                    <h2>{user ? user.Name : null}</h2>
                    <p>
                        {stats ? "Mætti fyrst " + stats.firstAttendedString : null}
                    </p>
                </header>
            </section>
            {/* <!-- Intro --> */}

            {/* <!-- Mini Posts --> */}
            <section>
                <div className="mini-posts">

                    {/* <!-- Mini Post --> */}
                    <MiniPost
                        title="Mæting"
                        // href="/hardhead/awards"
                        description={stats ? "Hefur mætt á " + stats.attendedCount + " kvöld" : null}
                        date={stats ? stats.FirstAttended : null}
                    // dateString={stats ? "Frá " + stats.FirstAttendedString : null}
                    // userHref={"http://www.hress.org/Gang/Single.aspx?Id=" + data.awards.Winner.ID}
                    // userPhoto={config.get('path') + data.awards.Winner.ProfilePhoto.Href + "?code=" + config.get('code')}
                    // userText={data.awards.Winner.Username} 
                    />

                    <LastFilm userID={params.id} />

                    {/* <!-- Mini Post --> */}
                    {/* <article class="mini-post">
                        <header>
                            <h3><a href="single.html">Rutrum neque accumsan</a></h3>
                            <time class="published" datetime="2015-10-19">October 19, 2015</time>
                            <a href="#" class="author"><img src="images/avatar.jpg" alt=""/></a>
                        </header>
                        <a href="single.html" class="image"><img src="images/pic05.jpg" alt=""/></a>
                    </article> */}

                </div>
            </section>

            {/* <!-- Posts List --> */}
            <section>
                {/* <ul class="posts">
                    <li>
                        <article>
                            <header>
                                <h3><a href="single.html">Lorem ipsum fermentum ut nisl vitae</a></h3>
                                <time class="published" datetime="2015-10-20">October 20, 2015</time>
                            </header>
                            <a href="single.html" class="image"><img src="images/pic08.jpg" alt=""/></a>
                        </article>
                    </li>
                    <li>
                        <article>
                            <header>
                                <h3><a href="single.html">Convallis maximus nisl mattis nunc id lorem</a></h3>
                                <time class="published" datetime="2015-10-15">October 15, 2015</time>
                            </header>
                            <a href="single.html" class="image"><img src="images/pic09.jpg" alt=""/></a>
                        </article>
                    </li>                    
                </ul> */}
            </section>

            {/* <!-- About --> */}
            {/* <section class="blurb">
                <h2>About</h2>
                <p>Mauris neque quam, fermentum ut nisl vitae, convallis maximus nisl. Sed mattis nunc id lorem euismod amet placerat. Vivamus porttitor magna enim, ac accumsan tortor cursus at phasellus sed ultricies.</p>
                <ul class="actions">
                    <li><a href="#" class="button">Learn More</a></li>
                </ul>
            </section> */}

            {/* <!-- Footer --> */}
            <section id="footer">
                <ul className="icons">
                    <li><a href="https://www.facebook.com/groups/988193164623754/" className="icon brands fa-facebook-f" target="_blank" rel="noopener noreferrer"><span className="label">Facebook</span></a></li>
                </ul>
                <p className="copyright">&copy; Hress.Org - Guðjón Karl Arnarson.<br />Design: <a href="http://html5up.net">HTML5 UP</a>.</p>
            </section>

        </section>
    );
}

export default HHUserSidebar;