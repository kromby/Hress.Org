import axios from "axios";
import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth"

const Disappointment = (propsData) => {
    const { authTokens } = useAuth();
    const [disappointments, setDisappointments] = useState();
    const [selectedValue, setSelectedValue] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);

    useEffect(() => {
        const getNominations = async () => {
            var url = config.get('path') + '/api/hardhead/awards/' + propsData.ID + '/nominations?code=' + config.get('code');
            try {
                const response = await axios.get(url);
                setDisappointments(response.data);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        if (!disappointments) {
            getNominations();
        }
    }, [propsData])

    const handleSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var voteData = [{ PollEntryID: selectedValue, Value: disappointments.filter(n => n.ID === selectedValue)[0].Nominee.ID }];

        try {
            var url = config.get('path') + '/api/elections/' + propsData.ID + '/vote?code=' + config.get('code');
            await axios.post(url, voteData, {
                headers: { 'Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        propsData.onSubmit();
    }

    const handleChange = async (event) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        setSelectedValue(event);
        setSavingAllowed(true);
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
                        <div className="row gtr-uniform">
                            {disappointments ? disappointments.map(nomination =>
                                <div className="col-6" key={nomination.ID} onClick={() => handleChange(nomination.ID)} >
                                    <input type="radio" checked={selectedValue === nomination.ID} onChange={() => handleChange(nomination.ID)} />
                                    <label>
                                        <h3 className="author" width="50%">
                                            {nomination.Nominee.ProfilePhoto ?
                                                <img src={config.get("apiPath") + nomination.Nominee.ProfilePhoto.Href} alt={nomination.Nominee.Username} />
                                                : null}
                                            &nbsp;&nbsp;&nbsp;
                                            <b>{nomination.Nominee.Username}</b>
                                        </h3>
                                    </label>
                                    <br />
                                    {nomination.Name}
                                    <br />
                                    <br />
                                    <u>Brot á eftirfarandi reglu:</u>
                                    <br />
                                    {nomination.AffectedRule}
                                    <br />
                                    <br />
                                </div>
                            ) : null}
                        </div>
                    </section>
                }
            />
            <ul className="actions pagination">
                <li>
                    <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Kjósa " + propsData.Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default Disappointment;