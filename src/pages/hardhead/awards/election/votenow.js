import React from 'react';
import { Link } from 'react-router-dom';
import MiniPost from '../../../../components/minipost';

const VoteNow = (propsData) => {
    return (
        <div>
            <MiniPost title="Harðhausakosningin 2020"
                description={
                    <div>
                        <Link to="/hardhead/awards/election" className="button large">Kjósa núna!</Link>
                    </div>
                }
            />
        </div>
    )
}

export default VoteNow;