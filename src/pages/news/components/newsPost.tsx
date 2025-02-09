import Post from '../../../components/post';
import config from "react-global-configuration";
import { NewsEntity } from '../../../types/newsEntity';
import { isMobile } from 'react-device-detect';

const MAX_IMAGE_HEIGHT = 500;
const getImageHeight = (image: any) => {
    if (image) {
        if (image.height > MAX_IMAGE_HEIGHT) {
            return MAX_IMAGE_HEIGHT;
        } else {
            return image.height;
        }
    } else {
        return 0;
    }
}

const NewsPost = ({ singleNews }: { singleNews: NewsEntity }) => {    
    const imageHeight = getImageHeight(singleNews.image);

    return (
        <Post
            key={singleNews.id}
            id={singleNews.id}
            href={`/news/${singleNews.id}`}
            title={singleNews.name}
            date={singleNews.inserted}
            dateFormatted={singleNews.insertedString}
            author={singleNews.author}
            body={
                <p style={singleNews.image && singleNews.imageAlign !== 4 ? { "minHeight": imageHeight - 50 } : undefined}>
                    {!isMobile && singleNews.image && singleNews.imageAlign !== 4 ?
                        <span className={singleNews.imageAlign === 1 ? "image left" : singleNews.imageAlign === 2 ? "image right" : undefined}>
                            <img 
                                style={{ "maxHeight": `${imageHeight}px` }} 
                                src={`${config.get("apiPath") + singleNews.image.href}?width=${MAX_IMAGE_HEIGHT}`} alt={singleNews.name}
                            />
                        </span> : null}
                    <span dangerouslySetInnerHTML={{ __html: singleNews.content }} />
                </p>}
            image={isMobile || singleNews.imageAlign === 4 ? config.get('apiPath') + singleNews.image.href + "?width=1400" : null}
            actions={<p />}
        />
    );
};

export default NewsPost;
