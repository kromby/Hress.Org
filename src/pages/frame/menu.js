import React, { useEffect, useState, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { isMobile } from 'react-device-detect';
import config from 'react-global-configuration';
import { useAuth } from '../../context/auth';
import axios from "axios";
import { useNavigate } from 'react-router-dom';

/**
 * Hook that alerts clicks outside of the passed ref
 */
function useOutsideAlerter(ref, visible, callback) {
    useEffect(() => {
        /**
         * Alert if clicked on outside of element
         */
        function handleClickOutside(event) {
            if (ref.current && !ref.current.contains(event.target)) {
                callback();
            }
        }
        // Bind the event listener
        if (visible) {
            document.addEventListener("mousedown", handleClickOutside);
            return () => {
                // Unbind the event listener on clean up
                document.removeEventListener("mousedown", handleClickOutside);
            };
        }
    }, [ref, visible, callback]);
}

const Menu = ({visible, onClick}) => {
    const { authTokens, setAuthTokens } = useAuth();
    const [data, setData] = useState({ isLoading: false, menuItems: null, userID: 0 });
    const [path, setPath] = useState();
    const [loggedIn, setLoggedIn] = useState(false);
    const [links, setLinks] = useState();
    const wrapperRef = useRef(null);
    const location = useLocation();
    const navigate = useNavigate();

    useOutsideAlerter(wrapperRef, visible, onClick);

    useEffect(() => {
        const getMenuData = async (pathname) => {

            try {
                var url = config.get('apiPath') + '/api/menus?navigateUrl=~' + pathname + '&fetchChildren=true';
                if (authTokens !== undefined) {
                    const response = await axios.get(url, {
                        headers: { 'X-Custom-Authorization': 'token ' + authTokens.token }
                    });
                    var userID = localStorage.getItem("userID");
                    setData({ menuItems: response.data, isLoading: false, visible: true, userID: userID })
                }
                else {
                    const response = await axios.get(url);
                    setData({ menuItems: response.data, isLoading: false, visible: true })
                }
            }
            catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }      

        const getLinks = async () => {
            try {
                var url = config.get("apiPath") + "/api/menus";
                var headers = authTokens ? { headers: { 'X-Custom-Authorization': 'token ' + authTokens.token } } : null;
                const response = await axios.get(url, headers);
                setLinks(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        

        if (isMobile && (!links || loggedIn != (authTokens != undefined))) {
            
            setLoggedIn(authTokens != undefined);
            getLinks();
        }

        if (!data.menuItems || path != window.location.pathname) {
            setPath(location.pathname);
            getMenuData(location.pathname);
        }
    }, [location, authTokens])

    const logout = () => {
        setAuthTokens();
        navigate("/");
    }

    return (
        <section id="menu" ref={wrapperRef}>

            <section>
                <form className="search" method="get" action="#">
                    <input type="text" name="query" placeholder="Leita" />
                </form>
            </section>

            <section>
                <ul className="links" onClick={() => onClick()}>
                    {isMobile && links ? links.map(link =>
                        <li key={link.id}>
                            {link.isLegacy ?
                                    <Link to={link.link.href + "?legacy=true"} target="_blank">
                                        <h3>{link.name}</h3>
                                        <p>{link.description}</p>
                                    </Link>
                                    :
                                    <Link to={link.link.href.replace("{userID}", data.userID)}>
                                        <h3>{link.name}</h3>
                                        <p>{link.description}</p>
                                    </Link>
                                }
                        </li>
                    ) : null}
                    {isMobile ? <hr/>: null}
                    {data.visible ?
                        data.menuItems.map(item =>
                            <li key={item.id}>
                                {item.isLegacy ?
                                    <Link to={item.link.href + "?legacy=true"} target="_blank">
                                        <h3>{item.name}</h3>
                                        <p>{item.description}</p>
                                    </Link>
                                    :
                                    <Link to={item.link.href.replace("{userID}", data.userID)}>
                                        <h3>{item.name}</h3>
                                        <p>{item.description}</p>
                                    </Link>
                                }
                            </li>
                        ) : null
                    }
                </ul>
            </section>

            <section>
                <ul className="actions stacked" onClick={() => onClick()}>
                    {authTokens === undefined ?
                        <li key="One"><Link className="button large fit" to="login" state={{ from: location.pathname } }>Innskráning</Link></li> :
                        [<li key="Two">
                            <Link to={"/Profile"}>
                                <h3>Prófíll</h3>
                            </Link>
                            {/* <Link to={"/Gang/Profile/MyProfile.aspx?legacy=true"} target="_blank">
                                <h3>Prófíll</h3>
                                <p>Breyttu þínum upplýsingum</p>
                            </Link> */}
                        </li>,
                        <li key="Three"><button className="button large fit" onClick={() => logout()}>Útskráning</button></li>]
                    }
                </ul>
            </section>

        </section>
    )
}

export default Menu;