import axios from "axios";
import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth";

const RuleChanges = (propsData) => {
    const { authTokens } = useAuth();
    const [changes, setChanges] = useState();
    const [value, setValue] = useState(-1);
    const [fallbackRule, setFallbackRule] = useState();

    useEffect(() => {
        const getChanges = async () => {
            var url = config.get('path') + propsData.href + '?code=' + config.get('code');
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
    }, [propsData])

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

        propsData.onSubmit(propsData.id, ruleID, newValue);
    }

    // const handleSubmit = async(event) => {
    //     event.preventDefault();
    //     if(authTokens === undefined) {
    //         alert("Þú þarf að skrá þig inn");
    //         return;
    //     }

    //     var ruleID = selectedRule;
    //     if(ruleID === 0)
    //         ruleID = fallbackRule;

    //     try {
    //         var url = config.get('path') + '/api/elections/' + propsData.id + '/vote?code=' + config.get('code');
    //         await axios.post(url, {                
    //             value: value,
    //             eventID: propsData.id,
    //             pollEntryId: ruleID
    //         }, {
    //             headers: {'Authorization': 'token ' + authTokens.token},
    //         });
    //     } catch(e) {
    //         console.error(e);
    //         alert(e);
    //     }
    // }

    return (
        <Post key={propsData.id}
            id={propsData.id}
            title={propsData.title}
            description={propsData.description}
            body={
                <section>
                    <div onClick={() => handleChange(-1)}>
                        <input type="radio" radioGroup={"id_" + propsData.id} checked={propsData.selectedValue === -1} onChange={() => handleChange(-1)} />
                        <label>
                            <u>Núverandi:</u> {propsData.current}
                        </label>
                    </div>
                    {changes ? changes.map(change =>
                        <div key={change.ID} onClick={() => handleChange(change.ID)}>
                            <input type="radio" radioGroup={"id_" + propsData.id} checked={propsData.selectedRule === change.ID && propsData.selectedValue === 1} onChange={() => handleChange(change.ID)} />
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