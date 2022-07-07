import React, { useEffect, useState, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { isMobile } from 'react-device-detect';
import config from 'react-global-configuration';
import { useAuth } from '../../context/auth';
import axios from "axios";

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

const Menu = (propsData) => {
    const { authTokens } = useAuth();
    const [data, setData] = useState({ isLoading: false, menuItems: null, userID: 0 });
    const [path, setPath] = useState();
    const [loggedIn, setLoggedIn] = useState(false);
    const [links, setLinks] = useState();
    const wrapperRef = useRef(null);
    const { pathname } = useLocation();

    useOutsideAlerter(wrapperRef, propsData.visible, propsData.onClick);

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

        console.log("[menu.js] isMobile: '" + isMobile + "'");

        if (isMobile && (!links || loggedIn != (authTokens != undefined))) {
            console.log("[menu.js] getLinks");
            setLoggedIn(authTokens != undefined);
            getLinks();
        }

        if (!data.menuItems || path != window.location.pathname) {
            setPath(window.location.pathname);
            getMenuData(window.location.pathname);
        }
    }, [propsData, authTokens])

    return (
        <section id="menu" ref={wrapperRef}>

            <section>
                <form className="search" method="get" action="#">
                    <input type="text" name="query" placeholder="Leita" />
                </form>
            </section>

            <section>
                <ul className="links" onClick={() => propsData.onClick()}>
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
                <ul className="actions stacked" onClick={() => propsData.onClick()}>
                    {/* 
                        {authTokens !== undefined ?
                        <li onClick={() => propsData.onClick()}><a href="#" className="button large fit">Útskráning</a></li> :
                        <li><a href="#" className="button large fit">Innskráning</a></li>
                    */}
                    {authTokens === undefined ?
                        <li key="One"><Link className="button large fit" to={{ pathname: "/login", state: { from: pathname } }}>Innskráning</Link></li> :
                        [<li key="Two">
                            <Link to={"/Gang/Profile/MyProfile.aspx?legacy=true"} target="_blank">
                                <h3>Prófíll</h3>
                                <p>Breyttu þínum upplýsingum</p>
                            </Link>
                        </li>,
                        <li key="Three">Útskráning (one day)</li>]
                    }
                </ul>
            </section>

        </section>
    )
}

export default Menu;