import React, {useEffect, useState, useRef} from 'react';
import { Link } from 'react-router-dom';
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
      if(visible)
      {
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            // Unbind the event listener on clean up
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }
    }, [ref, visible]);
  }

const Menu = (propsData) => {
    const {authTokens} = useAuth();
    const [data, setData] = useState({isLoading: false, menuItems: null, userID: 0});
    const wrapperRef = useRef(null);
    useOutsideAlerter(wrapperRef, propsData.visible, propsData.onClick);

    useEffect(() => {
        const getMenuData = async  () => {
            
            console.log("getMenuData");
            console.log(window.location.pathname);

                try {
                    var url = config.get('path') + '/api/menus?navigateUrl=~' + window.location.pathname + '&fetchChildren=true&code=' + config.get('code');                    
                    if(authTokens !== undefined){ 
                        const response = await axios.get(url, {
                            headers: {'Authorization': 'token ' + authTokens.token}               
                        });
                        var userID = localStorage.getItem("userID");
                        setData({menuItems: response.data, isLoading: false, visible: true, userID: userID})
                        console.log("getMenuData - data retrieved");
                        console.log(response.data);
                        console.log(data.visible);
                    }
                    else {
                        const response = await axios.get(url);
                        setData({menuItems: response.data, isLoading: false, visible: true})
                        console.log("getMenuData - data retrieved");
                        console.log(response.data);
                        console.log(data.visible);
                    }                    
                }
                catch(e) {
                    console.error(e);
                    setData({isLoading: false, visible: false});
                }                    
            console.log(data.visible);
        }
        getMenuData();
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
                    {data.visible ? 
                        data.menuItems.map(item => 
                        item.Public ?
                        <li>
                            <Link to={"/" + item.Link.Href}>
                                <h3>{item.Name}</h3>
                                <p>{item.Description}</p>
                            </Link>
                        </li>: null
                    ) : null                    
                    }

                    {authTokens !== undefined ?
                    <li>
                        <Link to={"/hardhead?userID=" + data.userID}>
                            <h3>Mín kvöld</h3>
                            <p>Upplýsingar um öll kvöld sem þú hefur haldið</p>
                        </Link>
                    </li> : null}
                </ul>
                </section>

                <section>
                <ul className="actions stacked">
                    {authTokens !== undefined ?
                        <li onClick={() => propsData.onClick()}><a href="#" className="button large fit">Útskráning</a></li> :
                        <li><a href="#" className="button large fit">Innskráning</a></li>
                    }
                </ul>
                </section>
            
        </section>   
    )
}

export default Menu;