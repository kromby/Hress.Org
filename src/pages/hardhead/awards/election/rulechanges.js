import axios from "axios";
import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth";

const RuleChanges = ({id, title, href, date, dateFormatted, description, current, selectedValue, selectedRule, onSubmit}) => {
    const { authTokens } = useAuth();
    const [changes, setChanges] = useState();
    const [value, setValue] = useState(-1);
    const [fallbackRule, setFallbackRule] = useState();

    useEffect(() => {
        const getChanges = async () => {
            var url = config.get('apiPath') + href;
            try {
                const response = await axios.get(url);
                setChanges(response.data);
                setFallbackRule(response.data[0].id);
            } catch (e) {
                console.error(e);
            }
        }

        if (!changes) {
            getChanges();
        }
    }, [href])

    const handleChange = async (event) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        // setSelectedRule(event);
        var newValue = 0;
        if (event === -1)
            newValue = -1;
        else
            newValue = 1;
        setValue(newValue);

        var ruleID = event;
        if (ruleID === -1)
            ruleID = fallbackRule;

        onSubmit(id, ruleID, newValue);
    }

    return (
        <Post key={id}
            id={id}
            title={title}
            description={description}
            date={date}
            dateFormatted={dateFormatted}
            body={
                <section>
                    <div onClick={() => handleChange(-1)}>
                        <input type="radio" radioGroup={"id_" + id} checked={selectedValue === -1} onChange={() => handleChange(-1)} />
                        <label>
                            <u>Núverandi</u> {current}
                        </label>
                    </div>
                    {changes ? changes.map(change =>
                        <div key={change.id} onClick={() => handleChange(change.id)}>
                            <input type="radio" radioGroup={"id_" + id} checked={selectedRule === change.id && selectedValue === 1} onChange={() => handleChange(change.id)} />
                            <label>
                                <u>{change.typeName}{change.ruleText ? ":": ""}</u> {change.ruleText}
                                <br/>
                                <i>{change.reasoning ? <u>Rökstuðningur:</u> : null} {change.reasoning}</i>
                            </label>
                        </div>
                    ) : null}
                </section>
            }
        />
    )
}

export default RuleChanges;