import { useEffect } from "react";
import { useParams } from "react-router-dom";

import { useUserById } from "../../../hooks/useSingleUser";
import UserAwards from "./userAwards";
import UserStatistics from "./userStatistics";
import Challenge from "./challenge";
import Streak from "./streak";

import "../../../components/utils/loading.css";

const HHUsers = () => {
  const params = useParams();
  const { user, loading, error } = useUserById(params.id);

  useEffect(() => {
    if (user?.name) {
      document.title = `${user.name} | Hress.Org`;
    }
  }, [user]);

  if (error) {
    return (
      <div id="main" className="error-container">
        <h2>Error loading user</h2>
        <p>{error.message || 'An unexpected error occurred'}</p>
      </div>
    );
  }

  if (loading) {
    return (
      <div id="main" className="loading-container">
        <div className="loading-spinner" />
        <p>Loading user profile...</p>
      </div>
    );
  }

  return (
    <div id="main">
      <UserAwards key="one" id={params.id} />
      <UserStatistics key="two" id={params.id} />
      {user ? (
        <Challenge
          id={params.id}
          username={user.name}
          profilePhoto={user.profilePhoto?.href}
        />
      ) : null}
      <Streak id={params.id} />
    </div>
  );
};

export default HHUsers;
