import axios from "axios";
import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth";

const RuleChanges = ({id, title, href, description, current, selectedValue, selectedRule, onSubmit}) => {
    const { authTokens } = useAuth();
    const [changes, setChanges] = useState();
    const [value, setValue] = useState(-1);
    const [fallbackRule, setFallbackRule] = useState();

    useEffect(() => {
        const getChanges = async () => {
            var url = config.get('path') + href + '?code=' + config.get('code');
            try {
                const response = await axios.get(url);
                setChanges(response.data);
                setFallbackRule(response.data[0].ID);
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
            body={
                <section>
                    <div onClick={() => handleChange(-1)}>
                        <input type="radio" radioGroup={"id_" + id} checked={selectedValue === -1} onChange={() => handleChange(-1)} />
                        <label>
                            <u>Núverandi:</u> {current}
                        </label>
                    </div>
                    {changes ? changes.map(change =>
                        <div key={change.ID} onClick={() => handleChange(change.ID)}>
                            <input type="radio" radioGroup={"id_" + id} checked={selectedRule === change.ID && selectedValue === 1} onChange={() => handleChange(change.ID)} />
                            <label>
                                {change.Description}
                            </label>
                        </div>
                    ) : null}
                </section>
            }
        />
    )
}

export default RuleChanges;