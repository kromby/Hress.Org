// authProvider.js
import { MsalAuthProvider, LoginType } from 'react-aad-msal';
 
// Msal Configurations
const config = {
  auth: {
    authority: 'https://login.microsoftonline.com/common',
    clientId: 'bc9e650a-ceb7-4636-815c-db54a05bcc8e',
    redirectUri: window.location.origin
    //redirectUri: '<OPTIONAL REDIRECT URI>'
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: true
  }
};
 
// Authentication Parameters
const authenticationParameters = {
  scopes: [
    '<property (i.e. user.read)>',
    'https://krombyhress.onmicrosoft.com/hress.org/demo.read'
  ]
}
 
// Options
const options = {
  loginType: LoginType.Popup,
  tokenRefreshUri: window.location.origin + '/auth.html'
}
 
export const authProvider = new MsalAuthProvider(config, authenticationParameters, options)