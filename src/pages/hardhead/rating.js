import React, {useEffect} from 'react';
// import axios from "axios";

const Rating = (propsData) => {
    // const [ratingData, setRatingData] = useState({ratings: [], isLoading: false});

    useEffect(() => {
        // const getRatingData = async  () => {
        //     try {
        //         const response = await axios.get("https://www.hress.org", {
        //             headers: {'Authorization': 'Basic' + ''}               
        //         })
        //     }
        //     catch(e) {
        //         console.error(e);
        //     }
        // }
        // getRatingData();
    }, [propsData])

    return (
        <div>
            Stjörnugjöf væntanleg á næstunni
        </div>
    )
}

export default Rating;