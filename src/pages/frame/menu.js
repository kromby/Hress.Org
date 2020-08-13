import React, {useEffect, useState, useRef} from 'react';
// import config from 'react-global-configuration';
import { useAuth } from '../../context/auth';
// import axios from "axios";

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
    const [data, setData] = useState({isLoading: false});
    const wrapperRef = useRef(null);
    useOutsideAlerter(wrapperRef, propsData.visible, propsData.onClick);

    useEffect(() => {
        const getMenuData = async  () => {
            // if(authTokens !== undefined){ 
            //     try {
            //         var url = config.get('path') + '/api/hardhead/' + propsData.id + '/ratings?code=' + config.get('code');                    
            //         const response = await axios.get(url, {
            //             headers: {'Authorization': 'token ' + authTokens.token}               
            //         })
            //         setData({ratings: response.data, isLoading: false, visible: true})
            //     }
            //     catch(e) {
            //         console.error(e);
            //         setData({isLoading: false, visible: false});
            //     }
            // }
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
                <ul className="links">
                    <li>
                    <a href="#">
                        <h3>Lorem ipsum</h3>
                        <p>Feugiat tempus veroeros dolor</p>
                    </a>
                    </li> 
                </ul>
                </section>

                <section>
                <ul className="actions stacked">
                    <li><a href="#" className="button large fit">Innskráning</a></li>
                </ul>
                </section>
            
        </section>   
    )
}

export default Menu;