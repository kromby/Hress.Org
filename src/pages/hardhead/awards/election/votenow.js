import React from 'react';
import { Link } from 'react-router-dom';
import { Post } from '../../../../components';
import { useAuth } from '../../../../context/auth';

const VoteNow = (propsData) => {
    const { authTokens } = useAuth();

    return (
        <div>
            {authTokens ?
                <Post
                    id="0"
                    title="Harðhausakosningin"
                    dateFormatted="2020"
                    body={
                        <section>
                            <p>
                                <Link to="/hardhead/awards/election" className="button large">Kjósa núna!</Link>
                            </p>
                        </section>
                    }
                />
                : null}
        </div>
    )
}

export default VoteNow;