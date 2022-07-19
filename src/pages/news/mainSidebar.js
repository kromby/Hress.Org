import { Intro } from '../../components';
import NextHardhead from '../hardhead/components/nexthardhead';
import './mainSidebar.css';
import OnThisDay from './onthisday';

const MainSidebar = (propsData) => {

    return (
        <section id="sidebar">
            <Intro logo="https://hress.azurewebsites.net/App_Themes/Default/Images/Logo.png" title="Hress.Org" description="þar sem hressleikinn býr" />

            {/* <!-- Mini Posts --> */}
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