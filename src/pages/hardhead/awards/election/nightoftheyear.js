import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';
import HardheadRating from '../../components/rating';
import HardheadBody from '../../components/hardheadbody';

const NightOfTheYear = (propsData) => {
    const { authTokens } = useAuth();
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

        if (!nights) {
            getHardheadUsers();
        }
    }, [propsData, url])

    const handelSubmit = async () => {

        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        try {
            var userID = localStorage.getItem("userID");
            var url = config.get('path') + "/api/elections/49/voters/" + userID + "?code=" + config.get('code');
            await axios.put(url, {
                LastStepID: propsData.ID
            }, {
                headers: { 'Authorization': 'token ' + authTokens.token }
            });
        } catch (e) {
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
                            Gefðu öllum kvöldunum sem þú mættir á einkunn, smelltu síðan á <i>Ljúka</i> neðst á síðunni til að halda áfram
                        </p>
                    </section>
                }
            />

            {nights ? nights.map(hardhead =>
                <Post
                    key={hardhead.ID}
                    id={hardhead.ID}
                    title={hardhead.Name}
                    description={hardhead.GuestCount + " gestir"}
                    date={hardhead.Date}
                    dateFormatted={hardhead.DateString}
                    author={hardhead.Host}
                    body={<HardheadBody id={hardhead.ID} name={hardhead.Name} description={hardhead.Description} viewMovie={false} />}
                    // body= {[ 
                    //     <section>
                    //         <h3>Kvöldið</h3>
                    //         <p>
                    //             {hardhead.Description ? hardhead.Description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
                    //         </p>                                
                    //     </section>,
                    //     <section>
                    //         <Guests hardheadID={hardhead.ID} />      
                    //         <p/>                      
                    //     </section>
                    // ]}
                    actions={<ul className="actions"></ul>}
                    stats={<HardheadRating id={hardhead.ID} movieRatingVisible="false" />}
                />
            )
                : null}

            <ul className="actions pagination">
                <li>
                    <a href="#" className="button large next" onClick={handelSubmit}>{"Ljúka einkunnargjöf fyrir " + propsData.Name}</a>
                    {/* <input type="submit" className="button large next" value={"Ljúka " + propsData.Name} disabled={!savingAllowed} /> */}
                </li>
            </ul>
        </div>
    )
}

export default NightOfTheYear;