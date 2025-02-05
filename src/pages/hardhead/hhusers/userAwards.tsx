import { Post } from "../../../components";
import UserAwardDetail from "./userAwardDetail";
import { useAwards } from "../../../hooks/hardhead/useAwards";

interface UserAwardsProps {
    id: number;
}

const UserAwards = ({ id }: UserAwardsProps) => {
    const { awards, error, isLoading } = useAwards();

    return (
        <Post 
            title="Verðlaunin"
            description="Efstu þrjú sætin"
            body={
                <section>
                    {isLoading ? (
                        <div className="loading">Sæki verðlaun...</div>
                    ) : error ? (
                        <div className="error">Villa kom upp við að sækja verðlaun</div>
                    ) : awards?.length ? (
                        awards.map(award => 
                            <UserAwardDetail 
                                key={award.id} 
                                awardID={award.id} 
                                name={award.name} 
                                userID={id} 
                            />
                        )
                    ) : (
                        <div>Engin verðlaun fundust</div>
                    )}
                </section>
            }
        />
    );
}

export default UserAwards;