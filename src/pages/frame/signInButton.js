// import React from "react";
// import { useMsal } from "@azure/msal-react";
// import { loginRequest } from "../../context/authConfig";
// /**
//  * Renders a drop down button with child buttons for logging in with a popup or redirect
//  */
// export const SignInButton = () => {
//     const { instance } = useMsal();

//     const handleLogin = (loginType) => {
//         if (loginType === "popup") {
//             instance.loginPopup(loginRequest).catch(e => {
//                 console.log(e);
//             });
//         } else if (loginType === "redirect") {
//             instance.loginRedirect(loginRequest).catch(e => {
//                 console.log(e);
//             });
//         }
//     }
//     return (
//         <button className="button large fit" onClick={() => handleLogin("popup")}>Innskráning</button>
//         // <DropdownButton variant="secondary" className="ml-auto" drop="left" title="Sign In">
//         //     <Dropdown.Item as="button" onClick={() => handleLogin("popup")}>Sign in using Popup</Dropdown.Item>
//         //     <Dropdown.Item as="button" onClick={() => handleLogin("redirect")}>Sign in using Redirect</Dropdown.Item>
//         // </DropdownButton>
//     )
// }