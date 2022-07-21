// import React from "react";
// import { useAuth } from "../../../context/auth";
// import { Post } from "../../../components";

// function Admin(props) {
//   const { authTokens, setAuthTokens } = useAuth();

//   function logOut() {
//     setAuthTokens();
//   }

//   return (
//     <div id="main">
//       <div>Admin Page</div>
//       <Post 
//         title="Admin síða"
//         description={<span>authTokens<br/>{authTokens ? JSON.stringify(authTokens) : null}</span>}
//         body={<button onClick={() => logOut()}>Útskrá</button>}
//       />      
//     </div>
//   );
// }

// export default Admin;