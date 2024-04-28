import axios from "axios";
import { useEffect, useState } from "react";
import { Post } from "../../components";
import config from "react-global-configuration";
import { isMobile } from "react-device-detect";
import UserImage from "../../components/users/userimage";

const Teams = ({ id }) => {
    const [teams, setTeams] = useState();
    const [showAnswers, setShowAnswers] = useState(false);
    let isFirst = true;

    useEffect(() => {
        const getTeams = async () => {
            const url = `${config.get("apiPath")}/api/dinnerparties/${id}/teams`;
            try {
                const response = await axios.get(url);
                setTeams(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!teams) {
            getTeams();
        }
    }, [id])

    function displayHr() {
        if (isFirst) {
            isFirst = false;
            return false;
        }
        return true;
    }

    const handleChange = () => {
        setShowAnswers(!showAnswers);
    }

    if (teams && teams.length > 0) {
        return (
            <Post key="teams"
                title="Rauðvínssmökkun"
                body={
                    <section>
                        {teams.map(team =>
                            <div key={team.id}>
                                {displayHr() ? <hr /> : null}
                                <h2>{"Lið #" + team.number} {team.isWinner ? "(Sigurliðið)" : null}</h2>
                                {team.isWinner || team.wine ?
                                    <blockquote>                                        
                                        {team.wine ? <span dangerouslySetInnerHTML={{ __html: team.wine }} /> : null} {/* skipcq: JS-0440 */}
                                    </blockquote> : null}
                                <div className="row gtr-uniform">
                                    {team.members.map(member =>
                                        <div className={isMobile ? "col-4 align-center" : "col-2 align-center"} key={member.id}>
                                            {member.user.profilePhoto ?
                                                <UserImage id={member.user.id} username={member.name} profilePhoto={member.user.profilePhoto.href} /> :
                                                <UserImage id={member.user.id} username={member.name} />
                                            }
                                        </div>
                                    )}
                                </div>
                                {team.quizQuestions && team.quizQuestions.length > 0 ?
                                    <div className="row gtr-uniform">
                                        <div className="col-12">
                                            <br />
                                            <h4>Spurningar</h4>
                                            <ol>
                                                {team.quizQuestions.map(question =>
                                                    <li key={question.id}>
                                                        {question.question}
                                                        {showAnswers ?
                                                            <blockquote>
                                                                {question.answer}
                                                            </blockquote> : null
                                                        }
                                                    </li>
                                                )}
                                            </ol>
                                        </div>
                                    </div>
                                    : null}
                                <p/>
                            </div>
                        )}
                        {id >= 5410 ?
                            <div className="row gtr-uniform">
                                <div className="col-12">
                                    <br />
                                    <input type="checkbox" id="cbx" checked={showAnswers} onChange={this.handleChange} />
                                    <label htmlFor="cbx">Sýna svör við öllum spurningum</label>
                                </div>
                            </div> : null
                        }
                    </section>
                }
            />
        )
    }
}

export default Teams;