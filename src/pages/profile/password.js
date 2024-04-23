import { useState, useEffect } from "react";
import { Post } from "../../components";
import { useAuth } from "../../context/auth"
import axios from "axios";
import config from 'react-global-configuration';
import { useLocation, useNavigate } from "react-router-dom";


const Password = () => {
    const { authTokens } = useAuth();
    const [buttonEnabled, setButtonEnabled] = useState(false);
    const [message, setMessage] = useState();

    const [password, setPassword] = useState();
    const [newPassword, setNewPassword] = useState();
    const [confirmPassword, setConfirmPassword] = useState();

    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        if (authTokens === undefined) {
            navigate("/login", {state: { from: location.pathname }} );
                return;
        }
    })

    const handleSubmit = async (event) => {
        setButtonEnabled(false);
        event.preventDefault();

        try {
            var url = config.get('apiPath') + '/api/users/0/password';
            const response = await axios.put(url, {
                password: password,
                newPassword: newPassword
            }, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            })

            if (response.status !== 202) {
                setMessage("Ekki tókst að breyta lykilorði!");
                return;
            }

            setMessage("Vistað!");

        } catch (e) {
            console.error(e);
            if (e.response && e.response.status === 400) {
                setMessage("Ekki tókst að breyta lykilorði! - " + e.message);
            }
            else {
                setMessage("Ekki tókst að breyta lykilorði!");
            }
        }
    }

    const handleNewPasswordChange = (event) => {
        setNewPassword(event.target.value);
        setButtonEnabled(comparePasswords(event.target.value, confirmPassword));
    }

    const handleConfirmPasswordChange = (event) => {
        setConfirmPassword(event.target.value);
        setButtonEnabled(comparePasswords(event.target.value, newPassword));
    }

    const comparePasswords = (pass1, pass2) => {
        if (password.length < 5) {
            return false;
        }

        if (pass1.length < 8) {
            return false;
        }

        return pass1 === pass2;
    }

    return (
        <div id="main">
            <Post
                title="Breyta lykilorði"
                body={[
                    <form onSubmit={handleSubmit} key="Form1">
                        <div className="row gtr-uniform">
                            <div className="col-6">
                                <input type="password" name="password" id="password" defaultValue={password} onChange={e => { setPassword(e.target.value) }} placeholder="Lykilorð" />
                            </div>
                            <div className="col-6"></div>
                            <div className="col-6">
                                <input type="password" name="newPassword" id="newPassword" defaultValue={newPassword} onChange={(ev) => handleNewPasswordChange(ev)} placeholder="Nýtt lykilorð" />
                            </div>
                            <div className="col-6">
                                <input type="password" name="confirmPassword" id="confirmPassword" defaultValue={confirmPassword} onChange={(ev) => handleConfirmPasswordChange(ev)} placeholder="Staðfestu nýja lykilorðið" />
                            </div>
                            <div className="col-12">
                                Lykilorð þarf að vera að minnsta kosti 8 stafir.
                            </div>
                            <div className="col-12"></div>
                            <div className="col-12">
                                <button
                                    title="Vista"
                                    className="button large"
                                    disabled={!buttonEnabled}>Breyta lykilorði</button>
                            </div>
                            <div className="col-12">
                                {message ? <b>{message}<br /></b> : null}
                            </div>
                        </div>
                    </form>
                ]}
            />
        </div>
    )
}

export default Password;