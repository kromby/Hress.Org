import React from 'react';
import ReactDOM from 'react-dom';
import { AzureAD } from 'react-aad-msal';

import { authProvider } from '../../context/authProvider';

const login = (propsData) => {

    return (
        <AzureAD provider={authProvider} forceLogin={true}>
            <p>
                Test 123
      </p>
        </AzureAD>
    );
}

export default login;