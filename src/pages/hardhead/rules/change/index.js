import { useEffect, useState } from "react";
import { useAuth } from "../../../../context/auth"
import { Post } from "../../../../components";
import config from 'react-global-configuration';
import axios from "axios";
import { useLocation, useNavigate } from "react-router-dom";

const RuleChange = () => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [buttonEnabled, setButtonEnabled] = useState(false);
    const [selected, setSelected] = useState();
    const [ruleCategories, setRuleCategories] = useState();
    const [selectedCategory, setSelectedCategory] = useState();
    const [rules, setRules] = useState();
    const [selectedRule, setSelectedRule] = useState();
    const [ruleText, setRuleText] = useState();
    const [reasoning, setReasoning] = useState();
    const [isSaved, setIsSaved] = useState(false);
    const [error, setError] = useState();
    const [ruleChanges, setRuleChanges] = useState();

    const ChangeType = Object.freeze({
        CREATE: 209,
        UPDATE: 210,
        DELETE: 211,
    })

    const getRuleChanges = async () => {
        try {
            const url = config.get('apiPath') + '/api/hardhead/rules/changes';
            const response = await axios.get(url, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
            setRuleChanges(response.data);
        } catch (e) {
            console.error(e);
        }
    }

    useEffect(() => {
        if (authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        const getRuleCategories = async () => {
            try {
                const url = config.get('apiPath') + '/api/hardhead/rules';
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

        if (!ruleChanges) {
            getRuleChanges();
        }
    }, [])

    const getRules = async (id) => {
        try {
            const url = config.get('apiPath') + '/api/hardhead/rules/' + id;
            const response = await axios.get(url);
            setRules(response.data);
        } catch (e) {
            console.error(e);
        }
    }

    const handleSubmit = async (event) => {
        setButtonEnabled(false);
        event.preventDefault();

        try {
            const postUrl = config.get('apiPath') + '/api/hardhead/rules/changes';
            const response = await axios.post(postUrl, {
                typeID: selected,
                ruleText: ruleText,
                ruleID: selectedRule,
                ruleCategoryID: selectedCategory,
                reasoning: reasoning,
            }, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });

            if (response.status !== 204) {
                setError("Ekki tókst að leggja fram reglubreytingu!");
                return;
            }

            setIsSaved(true);
            setSelected();
            setSelectedCategory();
            setRuleText();
            setReasoning();
            getRuleChanges();
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

    const allowSaving = (category, text, newReasoning, rule) => {        
        if (category === undefined || category === "")
            return false;

        if (newReasoning === undefined || newReasoning.length <= 10)
            return false;

        if (selected === RuleChange.CREATE || selected === RuleChange.UPDATE) {
            if (text === undefined || text.length <= 16)
                return false;
        }

        if (selected === RuleChange.UPDATE || selected === RuleChange.DELETE) {
            if (rule === undefined || rule === "")
                return false;
        }

        setIsSaved(false);
        setError("");
        return true;
    }    

    const handleTypeChange = (event) => { setSelected(event); }
    const handleTextChange = (event) => {
        setRuleText(event.target.value);
        setButtonEnabled(allowSaving(selectedCategory, event.target.value, reasoning, selectedRule));
    }
    const handleReasoningChange = (event) => {
        setReasoning(event.target.value);
        setButtonEnabled(allowSaving(selectedCategory, ruleText, event.target.value, selectedRule));
    }
    const handleRuleChange = (event) => {
        setSelectedRule(event.target.value);
        setButtonEnabled(allowSaving(selectedCategory, ruleText, reasoning, event.target.value));
    }

    const handleCategoryChange = (event) => {
        getRules(event.target.value);
        setIsSaved(false);
        setSelectedCategory(event.target.value);
        setButtonEnabled(allowSaving(event.target.value, ruleText, reasoning, selectedRule));
    }

    return (
        <div id="main">
            <Post
                title="tillaga að Reglubreytingu"
                description="Komdu með frábæra hugmynd til þess að flækja reglur Harðhausa"
                body={[

                    <div className="row gtr-uniform" key="first">
                        <div className="col-4" onClick={() => handleTypeChange(ChangeType.UPDATE)}>
                            <input type="radio" checked={selected === ChangeType.UPDATE} onChange={() => handleTypeChange(ChangeType.UPDATE)} />
                            <label>
                                Reglubreyting
                            </label>
                        </div>
                        <div className="col-4" onClick={() => handleTypeChange(ChangeType.CREATE)}>
                            <input type="radio" checked={selected === ChangeType.CREATE} onChange={() => handleTypeChange(ChangeType.CREATE)} />
                            <label>
                                Ný regla
                            </label>
                        </div>
                        <div className="col-4" onClick={() => handleTypeChange(ChangeType.DELETE)}>
                            <input type="radio" checked={selected === ChangeType.DELETE} onChange={() => handleTypeChange(ChangeType.DELETE)} />
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
                                        {ruleCategories.map((rule) =>
                                            <option key={rule.id} value={rule.id}>{rule.number + '. grein - ' + rule.name}</option>
                                        )}
                                    </select>
                                </div>
                                {rules && (selected === ChangeType.UPDATE || selected === ChangeType.DELETE) ?
                                    <div className="col-12">
                                        <select id="rule" name="rule" onChange={(ev) => handleRuleChange(ev)}>
                                            <option value="">{selected === ChangeType.UPDATE ? "- Hvaða reglu viltu breyta? -" : "Hvaða reglu viltu fjarlægja?"}</option>
                                            {rules.map((rule) =>
                                                <option key={rule.id} value={rule.id}>{rule.parentNumber + "." + rule.number + ". " + rule.name}</option>
                                            )}
                                        </select>
                                    </div>
                                    : null}

                                {selected === ChangeType.CREATE || selected === ChangeType.UPDATE ?
                                    <div className="col-12">
                                        <textarea
                                            name="ruleText"
                                            rows="2"
                                            onChange={(ev) => handleTextChange(ev)}
                                            defaultValue={ruleText}
                                            placeholder={selected === ChangeType.UPDATE ? "Hvernig viltu hafa regluna eftir breytingu?" : "Hvernig viltu hafa nýju regluna?"}
                                        />
                                    </div>
                                    : null}
                                <div className="col-12">
                                    <textarea
                                        name="reasoning"
                                        rows="2"
                                        onChange={(ev) => handleReasoningChange(ev)}
                                        defaultValue={reasoning}
                                        placeholder="Rökstuddu breytinguna!"
                                    />
                                </div>
                                <div className="col-12">
                                    {error ? <b>{error}<br /></b> : null}
                                    <button
                                        title="Leggja fram reglubreytingu"
                                        className="button large"
                                        disabled={!buttonEnabled}>Leggja til</button>
                                </div>
                            </div>
                            : null}
                    </form>,
                    <div className="col-12" key="message">
                        {isSaved ? <b>Tilnefning skráð!<br /></b> : null}
                    </div>,
                    <hr key="Line3" />,
                    <div className="table-wrapper" key="Table4">
                        <table>
                            <thead>
                                <tr>
                                    <th width="200px">Tegund</th>
                                    <th>Regla</th>
                                    <th>Rökstuðningur</th>
                                </tr>
                            </thead>
                            {ruleChanges ?
                                <tbody>
                                    {ruleChanges.map((ruleChange) =>
                                        <tr key={ruleChange.id}>
                                            <td>{ruleChange.typeName}</td>
                                            <td>{ruleChange.ruleText}</td>
                                            <td>{ruleChange.reasoning}</td>
                                        </tr>
                                    )}
                                </tbody>
                                : null}
                        </table>
                    </div>,
                ]}
            />
        </div >
    )

}

export default RuleChange;