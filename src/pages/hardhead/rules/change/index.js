import { useEffect, useState } from "react";
import { useAuth } from "../../../../context/auth"
import { Redirect } from "react-router-dom/cjs/react-router-dom";
import { Post } from "../../../../components";
import config from 'react-global-configuration';
import axios from "axios";

const RuleChange = (propsData) => {
    const { authTokens } = useAuth();
    const [buttonEnabled, setButtonEnabled] = useState(false);
    const [selected, setSelected] = useState();
    const [ruleCategories, setRuleCategories] = useState();
    const [selectedCategory, setSelectedCategory] = useState();
    const [ruleText, setRuleText] = useState();
    const [isSaved, setIsSaved] = useState(false);
    const [error, setError] = useState();

    useEffect(() => {
        const getRuleCategories = async () => {
            try {
                var url = config.get('path') + '/api/hardhead/rules?code=' + config.get('code');
                const response = await axios.get(url);
                setRuleCategories(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Reglubreyting | Hress.Org";

        if (!ruleCategories) {
            getRuleCategories();
        }
    }, [propsData])

    const handleSubmit = async (event) => {
        setButtonEnabled(false);
        event.preventDefault();

        try {
            // var postUrl = config.get('apiPath') + '/api/hardhead/awards/nominations';
            // const response = await axios.post(postUrl, {
            //     typeID: propsData.Type,
            //     description: description,
            //     nomineeID: nominee
            // }, {
            //     headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            // });
            setIsSaved(true);
            setSelected();
            setSelectedCategory();
            setRuleText();                   
        } catch (e) {
            console.error(e);
            if (e.response && e.response.status === 400) {
                setError("Ekki tókst að leggja fram reglubreytingu! - " + e.message);
            }
            else {
                setError("Ekki tókst að leggja fram reglubreytingu!");
            }
        }        
    }

    const handleTypeChange = async (event) => { setSelected(event); }
    const handleCategoryChange = (event) => { setSelectedCategory(event.target.value); setButtonEnabled(allowSaving(event.target.value, ruleText)); }
    const handleTextChange = (event) => { setRuleText(event.target.value); setButtonEnabled(allowSaving(selectedCategory, event.target.value)); }

    const allowSaving = (category, text) => {
        console.log('[RuleChange] category: ' + category);
        if (category === undefined || category === "")
            return false;

        if (selected === 1 || selected === 2) {
            if (text === undefined || text.length <= 16)
                return false;
        }

        setIsSaved(false);
        setError("");
        return true;
    }


    if (authTokens === undefined) {
        return <Redirect to={{ pathname: "/login", state: { from: propsData.location.pathname } }} />
    }
    else {
        return (
            <div id="main">
                <Post
                    title="tillaga að Reglubreytingu"
                    description="Komdu með frábæra hugmynd til þess að flækja reglur Harðhausa"
                    body={[

                        <div className="row gtr-uniform" key="first">
                            <div className="col-4" onClick={() => handleTypeChange(1)}>
                                <input type="radio" checked={selected === 1} onChange={() => handleChange(1)} />
                                <label>
                                    Reglubreyting
                                </label>
                            </div>
                            <div className="col-4" onClick={() => handleTypeChange(2)}>
                                <input type="radio" checked={selected === 2} onChange={() => handleChange(2)} />
                                <label>
                                    Ný regla
                                </label>
                            </div>
                            <div className="col-4" onClick={() => handleTypeChange(3)}>
                                <input type="radio" checked={selected === 3} onChange={() => handleChange(3)} />
                                <label>
                                    Fjarlægja reglu
                                </label>
                            </div>
                        </div>,
                        <hr key="Line2" />,
                        <form onSubmit={handleSubmit} key="Form1">
                            {selected ?
                                <div className="row gtr-uniform">
                                    <div className="col-12">
                                        <select id="category" name="category" onChange={(ev) => handleCategoryChange(ev)}>
                                            <option value="">- Í hvaða flokki viltu breyta reglu? -</option>
                                            {ruleCategories.map((rule, i) =>
                                                <option key={rule.ID} value={rule.ID}>{i + 1 + '. grein - ' + rule.Name}</option>
                                            )}
                                        </select>
                                    </div>
                                    {selected === 1 || selected === 2 ?
                                        <div className="col-12">
                                            <textarea
                                                name={selected}
                                                rows="2"
                                                onChange={(ev) => handleTextChange(ev)}
                                                defaultValue={ruleText}
                                                placeholder={selected === 1 ? "Hvernig viltu hafa regluna eftir breytingu?" : "Hvernig viltu hafa nýju regluna?"}
                                            />
                                        </div>
                                        : null}
                                    <div className="col-12">
                                        {isSaved ? <b>Tilnefning skráð!<br /></b> : null}
                                        {error ? <b>{error}<br /></b> : null}
                                        <button
                                            tooltip="Leggja fram reglubreytingu"
                                            className="button large"
                                            disabled={!buttonEnabled}>Leggja til</button>
                                    </div>
                                </div>
                                : null}
                        </form>
                    ]}
                />
            </div >
        )
    }
}

export default RuleChange;