import axios from "axios";
import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth"
import { isMobile } from 'react-device-detect';
import { Nomination } from "../../../../types/nomination";

interface DisappointmentProps {
    ID: number;
    Name: string;
    Description?: string;
    Date?: string;
    Year?: string;
    onSubmit: () => void;
}

const Disappointment = ({ ID, Name, Description, Date, Year, onSubmit }: DisappointmentProps) => {
    const { authTokens } = useAuth();
    const [disappointments, setDisappointments] = useState<Nomination[]>([]);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [savingAllowed, setSavingAllowed] = useState(false);

    useEffect(() => {
        const getNominations = async () => {
            const url = config.get('apiPath') + '/api/hardhead/awards/nominations?type=' + ID;
            try {
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
                });
                setDisappointments(response.data);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        if (!disappointments) {
            getNominations();
        }
    }, [ID])

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        setSavingAllowed(false);
        event.preventDefault();
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        const voteData = [{ id: selectedValue, Value: disappointments.filter(n => n.id === selectedValue)[0].nominee.id }];

        try {
            const url = config.get('apiPath') + '/api/elections/' + ID + '/vote';
            await axios.post(url, voteData, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        onSubmit();
    }

    const handleChange = (event: string) => {
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
                id={ID}
                title={Name}
                description={Description}
                date={Date}
                dateFormatted={Year}
                body={
                    <section>
                        <div className="row gtr-uniform">
                            {disappointments ? disappointments.map(nomination =>
                                <div className={isMobile ? "col-12" : "col-6"} key={nomination.id} onClick={() => handleChange(nomination.id)} >
                                    <input type="radio" checked={selectedValue === nomination.id} onChange={() => handleChange(nomination.id)} />
                                    <label>
                                        <h3 className="author" style={{ width: '50%', paddingLeft: '125px' }}>
                                            {nomination.nominee.profilePhoto ?
                                                <img src={`${config.get("apiPath")}${nomination.nominee.profilePhoto.href}?height=40&width=40`} alt={nomination.nominee.name} />
                                                : null}
                                            &nbsp;&nbsp;&nbsp;
                                            <b>{nomination.nominee.name}</b>
                                        </h3>
                                    </label>
                                    <br />
                                    {nomination.description}
                                    <br />
                                    <br />
                                    <u>Brot á eftirfarandi reglu:</u>
                                    <br />
                                    {nomination.affectedRule}
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
                    <button onClick={(e) => handleSubmit(e as any)} disabled={!savingAllowed} className="button large next">{"Kjósa " + Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default Disappointment;