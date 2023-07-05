import { useEffect } from "react";
import { useAuth } from "../../../../context/auth"
import { Redirect } from "react-router-dom/cjs/react-router-dom";
import { Post } from "../../../../components";

const RuleChange = (propsData) => {
    const { authTokens } = useAuth();

    useEffect(() => {

        document.title = "Reglubreyting | Hress.Org";
    }, [propsData])

    const handleSubmit = async (event) => {

    }

    if (authTokens === undefined) {
        return <Redirect to='/hardhead' />
    }
    else {
        return (
            <div id="main">
                <Post
                    title="tillaga að Reglubreytingu"
                    description="Komdu með frábæra hugmynd til þess að flækja reglur Harðhausa"
                    body={
                        <form onSubmit={handleSubmit} key="Form1">
                            <div className="row gtr-uniform">
                                <div className="col-6 col-12-xsmall">
                                </div>
                            </div>
                        </form>
                    }
                />
            </div>
        )
    }
}

export default RuleChange;