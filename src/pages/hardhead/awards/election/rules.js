import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth"
import RuleChanges from "./rulechanges";

const Rules = ({ID, Name, onSubmit}) => {
    const { authTokens } = useAuth();
    const [ruleList, setRuleList] = useState([]);
    const [selectedValues, setSelectedValues] = useState([]);
    const [savingAllowed, setSavingAllowed] = useState(false);

    useEffect(() => {
        const getRules = async () => {
            var url = config.get('apiPath') + '/api/hardhead/rules';
            try {
                const response = await axios.get(url);

                let temparray = [];
                await response.data.forEach(async parent => {
                    var childRules = await getChild(parent.subRules.href);

                    let selectedArr = selectedValues;
                    childRules.forEach(element => {
                        selectedArr.push({ EventID: element.id, PollEntryID: 0, Value: 0 });
                    });
                    setSelectedValues(selectedArr);

                    temparray = temparray.concat(childRules);
                    setRuleList(temparray);
                });

            } catch (e) {
                console.error(e);
            }
        }

        getRules();
    }, [])

    async function getChild(href) {
        var url = config.get('apiPath') + href;
        try {
            const response = await axios.get(url);
            return response.data.filter(p => p.changes);
        } catch (e) {
            console.error(e);
        }
    }    

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

    const handleChange = async (id, changeId, value) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var tempList = selectedValues.filter(v => v.EventID !== id);
        tempList.push({ EventID: id, PollEntryID: changeId, Value: value });
        setSelectedValues(tempList);

        if (tempList.filter(v => v.Value === 0).length === 0) {
            setSavingAllowed(true);
        }
    }

    return (
        <div>
            {ruleList ? ruleList.map(rule =>
                <RuleChanges
                    key={rule.id}
                    href={rule.changes ? rule.changes.href : null}
                    current={rule.name}
                    id={rule.id}
                    title={rule.parentNumber + ". kafli " + rule.number + ". grein"}
                    description="Reglubreyting"
                    selectedRule={selectedValues.filter(v => v.EventID === rule.id)[0].PollEntryID}
                    selectedValue={selectedValues.filter(v => v.EventID === rule.id)[0].Value}
                    onSubmit={handleChange} />
            ) : <Post title="Engar reglubreytingar" />}

            <ul className="actions pagination">
                <li>
                    <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Kjósa um " + Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default Rules;