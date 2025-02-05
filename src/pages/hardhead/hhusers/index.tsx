import { useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";

import { useUserById } from "../../../hooks/useSingleUser";
import UserAwards from "./userAwards";
import UserStatistics from "./userStatistics";
import Challenge from "./challenge";
import Streak from "./streak";

import "../../../components/utils/loading.css";

const HHUsers = () => {
  const params = useParams();
  const navigate = useNavigate();
  
  useEffect(() => {
    if (!params.id) {
      navigate('/hardhead');
      return;
    }
  }, [params.id, navigate]);

  const userId = Number(params.id);
  const { user, loading, error } = useUserById(userId);

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
      <UserAwards key="one" id={userId} />
      <UserStatistics key="two" id={userId} />
      {user ? (
        <Challenge
          id={userId}
          username={user.name}
          profilePhoto={user.profilePhoto?.href}
        />
      ) : null}
      <Streak id={userId} />
    </div>
  );
};

export default HHUsers;
