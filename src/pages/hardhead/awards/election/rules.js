import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth"
import RuleChanges from "./rulechanges";

async function getChild(href) {
    var url = config.get('path') + href + '?code=' + config.get('code');
    try {
        const response = await axios.get(url);
        return response.data.filter(p => p.Changes);
    } catch (e) {
        console.error(e);
    }
}

const Rules = (propsData) => {
    const { authTokens } = useAuth();
    const [ruleList, setRuleList] = useState([]);
    const [selectedValues, setSelectedValues] = useState([]);
    const [savingAllowed, setSavingAllowed] = useState(false);

    useEffect(() => {
        const getRules = async () => {
            var url = config.get('path') + '/api/hardhead/rules?code=' + config.get('code');
            try {
                const response = await axios.get(url);

                let temparray = [];
                await response.data.forEach(async parent => {
                    var childRules = await getChild(parent.SubRules.Href);

                    let selectedArr = selectedValues;
                    childRules.forEach(element => {
                        selectedArr.push({ EventID: element.ID, PollEntryID: 0, Value: 0 });
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
                    key={rule.ID}
                    href={rule.Changes.Href}
                    current={rule.Name}
                    id={rule.ID}
                    title={rule.ParentNumber + ". kafli " + rule.Number + ". grein"}
                    description="Reglubreyting"
                    selectedRule={selectedValues.filter(v => v.EventID === rule.ID)[0].PollEntryID}
                    selectedValue={selectedValues.filter(v => v.EventID === rule.ID)[0].Value}
                    onSubmit={handleChange} />
            ) : <Post title="Engar reglubreytingar" />}

            <ul className="actions pagination">
                <li>
                    {/* <a href="#" className="button large next" onClick={handleSubmit}>{"Ljúka kosningu um " + propsData.Name}</a> */}
                    <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Ljúka kosningu um " + propsData.Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default Rules;