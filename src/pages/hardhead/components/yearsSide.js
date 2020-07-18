import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import SidePost from '../../../components/sidepost';

const YearsSide = (propsData) => {
    const[data, setData] = useState({years: null, isLoading: false, visible: false});

    var url = config.get('path') + '/api/hardhead/years?code=' + config.get('code');		

    useEffect(() => {
        const getYears = async () => {
            try {
                setData({isLoading: true});
                const response = await axios.get(url);
                setData({years: response.data, isLoading: false, visible: true});
            }
            catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }
        }

        getYears();

    }, [propsData, url])

    return (
        <div>
            {data.visible ? data.years.map((year) => 
                <li key={year.ID}>
                    <SidePost 
                        title={"Árið " + year.Name} 
                        href={"/hardhead?parentID=" + year.ID} 
                        // dateString={year.Name} date="" 
                        image={year.Photo ? config.get("path") + year.Photo.Href + "?code=" + config.get("code") : null} imageText={year.Photo ? year.Description : null} 
                    />
                </li>
            ) : null}
        </div>
    )
}

export default YearsSide;