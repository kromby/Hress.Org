import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../components";

const DinnerMenu = ({id}) => {
    const [menu, setMenu] = useState();
    let isFirst = true;

    useEffect(() => {
        const getMenu = async () => {
            const url = `${config.get("apiPath")}/api/dinnerParties/${id}/courses`;
            try {
                const response = await axios.get(url);
                setMenu(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!menu) {
            getMenu();
        }
    }, [id]);

    function displayHr() {
        if (isFirst) {
            isFirst = false;
            return false;
        }
        return true;
    }

    return (
        <Post
            title="Matseðill"
            body={
                <section>
                    {menu ? menu.map(course =>
                        <div key={course.id}>
                            {displayHr() ? <hr /> : null}
                            <h2>{course.name}</h2>
                            {course.dishes.map(dish =>
                                <blockquote key={dish.id}>                                    
                                    <span dangerouslySetInnerHTML={{ __html: dish.name }} /> {/* skipcq: JS-0440 */}
                                </blockquote>
                            )}
                        </div>
                    ) : null}
                </section>
            }
            actions={null}
            stats={null}
        />
    )
}

export default DinnerMenu;