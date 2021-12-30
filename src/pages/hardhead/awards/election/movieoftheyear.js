import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';
import HardheadRating from '../../components/rating';
import HardheadBody from '../../components/hardheadbody';

const MovieOfTheYear = (propsData) => {
    const {authTokens} = useAuth();
    const [nights, setNights] = useState();

    var url = config.get('path') + '/api/hardhead?parentID=5370&attended=8&code=' + config.get('code');

    useEffect(() => {
        const getHardheadUsers = async () => {
            try {
                const response = await axios.get(url);
                setNights(response.data);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        getHardheadUsers();
    }, [propsData, url])

    const handleSubmit = async (event) => {

        if(authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        try {
            var userID = localStorage.getItem("userID");
            var url = config.get('path') + "/api/elections/49/voters/" + userID + "?code=" +  config.get('code');
            await axios.put(url, {
              LastStepID: propsData.ID
            }, {
                headers: {'Authorization': 'token ' + authTokens.token}
            });
        } catch(e) {
            console.error(e);
            alert(e);
        }

        propsData.onSubmit();
    }

    return (  
        <div>
            <Post
                id={propsData.ID}
                title={propsData.Name}
                description={propsData.Description}
                date={propsData.Date}
                dateFormatted={propsData.Year}
                body={
                    <section>
                        <p>
                            Gefðu öllum myndunum sem þú sást einkunn, smelltu síðan á <b>Ljúka</b> neðst á síðunni til að halda áfram
                        </p>
                    </section>
                }
            />

            {nights ? nights.map(hardhead => 
                    <Post
                        key={hardhead.ID} 
                        id={hardhead.ID} 
                        title={hardhead.Name}
                        date={hardhead.Date}
                        dateFormatted={hardhead.DateString}
                        // body= { <Movie id={hardhead.ID}/> }
                        body = {<HardheadBody id={hardhead.ID} name={hardhead.Name} description={hardhead.Description} viewNight={false} viewGuests={false} /> }
                        actions={ <ul className="actions"></ul> }
                        stats={ <HardheadRating id={hardhead.ID} nightRatingVisible="false" /> }	
                    />
                )
            : null}

            <ul className="actions pagination">
                <li>
                    <a href="#" className="button large next" onClick={handleSubmit}>{"Ljúka einkunnargjöf fyrir " + propsData.Name}</a>
                    {/* <input type="submit" className="button large next" value={"Ljúka " + propsData.Name} disabled={!savingAllowed} /> */}
                </li>
            </ul>
        </div>
    )
}

export default MovieOfTheYear;