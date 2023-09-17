import axios from "axios";
import { useEffect } from "react";
import { useState } from "react";
import { Post } from "../../components";
import config from "react-global-configuration";
import { isMobile } from "react-device-detect";
import UserImage from "../../components/users/userimage";

const Teams = ({id}) => {
    const [teams, setTeams] = useState();
    var isFirst = true;

    useEffect(() => {
        const getTeams = async () => {
            var url = config.get("apiPath") + "/api/dinnerparties/" + id + "/teams";
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

    if (teams && teams.length > 0) {
        return (
            <Post key="teams"
                title="Rauðvínssmökkun"
                body={
                    <section>
                        {teams.map(team =>
                            <div key={team.id}>
                                {displayHr() ? <hr /> : null}
                                <h2>{"Lið #" + team.number}</h2>
                                {team.isWinner || team.wine ?
                                    <blockquote>
                                        {team.isWinner ? [<b>Sigurliðið</b>, <br key="2" />] : null}
                                        {team.wine ? <span dangerouslySetInnerHTML={{ __html: team.wine }} /> : null}
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
                            </div>
                        )}
                    </section>
                }
            />
        )
    }
}

export default Teams;