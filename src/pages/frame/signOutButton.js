import React from "react";
import { useMsal } from "@azure/msal-react";

/**
 * Renders a sign-out button
 */
export const SignOutButton = () => {
    const { instance } = useMsal();
    return (
        <button className="button large fit ml-auto" variant="secondary" onClick={() => instance.logout()}>Útskráning</button>
    )
}