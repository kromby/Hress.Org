import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from "../../../../context/auth"
import { Post } from "../../../../components";

const RulesNewOld = (propsData) => {
    const { authTokens } = useAuth();
    const [rules, setRules] = useState();
    const [selectedValues, setSelectedValues] = useState([]);
    const [savingAllowed, setSavingAllowed] = useState(false);

    useEffect(() => {
        const getRules = async () => {
            var url = config.get('path') + '/api/hardhead/rules/changes?code=' + config.get('code');
            try {
                const response = await axios.get(url);
                setRules(response.data);

                var arr = [];
                response.data.forEach(element => {
                    arr.push({ PollEntryID: element.ID, Value: 0 });
                });
                setSelectedValues(arr);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        if (!rules) {
            getRules();
        }
    }, [propsData])

    const handleSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        try {
            var url = config.get('apiPath') + '/api/elections/' + propsData.ID + '/vote';
            await axios.post(url, selectedValues, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        propsData.onSubmit();
    }

    const handleChange = async (id, value) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var tempList = selectedValues.filter(v => v.PollEntryID !== id);
        tempList.push({ PollEntryID: id, Value: value });
        setSelectedValues(tempList);


        console.log("Length");
        console.log(tempList.filter(v => v.Value === 0).length);
        if (tempList.filter(v => v.Value === 0).length === 0) {
            setSavingAllowed(true);
        }
    }

    return (
        <div>
            {rules ? rules.map(rule =>
                <Post
                    key={rule.ID}
                    id={rule.ID}
                    title={rule.ElectionType.Name + " " + rule.Name}
                    description={rule.Description}
                    body={
                        <section>
                            <div onClick={() => handleChange(rule.ID, 1)}>
                                <input
                                    type="radio"
                                    radioGroup={"id_" + rule.ID}
                                    checked={selectedValues.filter(v => v.PollEntryID === rule.ID)[0] ? selectedValues.filter(v => v.PollEntryID === rule.ID)[0].Value === 1 : false}
                                    onChange={() => handleChange(rule.ID, 1)} />
                                <label>Samþykkja</label>
                            </div>
                            <div onClick={() => handleChange(rule.ID, -1)}>
                                <input
                                    type="radio"
                                    radioGroup={"id_" + rule.ID}
                                    checked={selectedValues.filter(v => v.PollEntryID === rule.ID)[0] ? selectedValues.filter(v => v.PollEntryID === rule.ID)[0].Value === -1 : false}
                                    onChange={() => handleChange(rule.ID, -1)} />
                                <label>Hafna</label>
                            </div>
                        </section>
                    }
                />
            ) : <Post title="Engar reglubreytingar" />}

            <ul className="actions pagination">
                <li>
                    {/* <a href="#" className="button large next" onClick={handleSubmit} disabled={!savingAllowed}>{"Ljúka kosningu um " + propsData.Name}</a>
                    <input type="submit" value={"Kjósa " + propsData.Name} disabled={!savingAllowed} /> */}
                    <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Ljúka kosningu um " + propsData.Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default RulesNewOld;