import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from "../../../../context/auth"
import { Post } from "../../../../components";

const RulesNewOld = ({ ID, Name, onSubmit }) => {
    const { authTokens } = useAuth();
    const [rules, setRules] = useState();
    const [selectedValues, setSelectedValues] = useState([]);
    const [savingAllowed, setSavingAllowed] = useState(false);

    useEffect(() => {
        const getRules = async () => {
            var url = `${config.get('apiPath')}/api/hardhead/rules/changes`;
            try {
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
                });
                setRules(response.data);

                var arr = [];
                response.data.forEach(element => {
                    arr.push({ ID: element.id, Value: 0 });
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
    }, [ID])

    const handleSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        try {
            var url = config.get('apiPath') + '/api/elections/' + ID + '/vote';
            await axios.post(url, selectedValues, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        onSubmit();
    }

    const handleChange = async (id, value) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var tempList = selectedValues.filter(v => v.ID !== id);
        tempList.push({ ID: id, Value: value });
        setSelectedValues(tempList);

        if (tempList.filter(v => v.Value === 0).length === 0) {
            setSavingAllowed(true);
        }
    }

    return (
        <div>
            {rules ? rules.map(rule =>
                <Post
                    key={rule.id}
                    id={rule.id}
                    title={`${rule.typeName} ${rule.name}`}
                    description={rule.ruleText}
                    body={
                        <section>
                            <div onClick={() => handleChange(rule.id, 1)}>
                                <input
                                    type="radio"
                                    radioGroup={"id_" + rule.id}
                                    checked={selectedValues.filter(v => v.ID === rule.id)[0] ? selectedValues.filter(v => v.ID === rule.id)[0].Value === 1 : false}
                                    onChange={() => handleChange(rule.id, 1)} />
                                <label>Samþykkja</label>
                            </div>
                            <div onClick={() => handleChange(rule.ID, -1)}>
                                <input
                                    type="radio"
                                    radioGroup={"id_" + rule.id}
                                    checked={selectedValues.filter(v => v.ID === rule.id)[0] ? selectedValues.filter(v => v.ID === rule.id)[0].Value === -1 : false}
                                    onChange={() => handleChange(rule.id, -1)} />
                                <label>Hafna</label>
                            </div>
                        </section>
                    }
                />
            ) : <Post title="Engar reglubreytingar" />}

            <ul className="actions pagination">
                <li>
                    <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Kjósa um " + Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default RulesNewOld;