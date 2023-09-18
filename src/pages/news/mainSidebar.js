import { Intro } from '../../components';
import NextHardhead from '../hardhead/components/nexthardhead';
import './mainSidebar.css';
import OnThisDay from './onthisday';

const MainSidebar = () => {

    return (
        <section id="sidebar">
            <Intro logo="/logo.png" title="Hress.Org" description="þar sem hressleikinn býr" />

			<section>
				<div className="mini-posts">
					{/* <!-- Mini Post --> */}
					<OnThisDay />
                    <NextHardhead />	
				</div>
			</section>
        </section>
    )
}

export default MainSidebar;