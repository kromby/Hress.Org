import React from 'react';
import { Intro } from '../../components';
import './sidebar.css';
import NextHardhead from './components/nexthardhead';
import AwardsSide from './awards/awardsSide';
import StatisticSide from './statistics/statisticsSide';
import RulesSide from './rules/rulesSide';
import YearsSide from './components/yearsSide';

const HardheadSidebar = (propsData) => {
    return (
        <section id="sidebar">
            <Intro logo="https://hress.azurewebsites.net/App_Themes/Default/Images/Logo.png" title="Harðhaus" description="" />

			{/* <!-- Mini Posts --> */}
			<section>
				<div className="mini-posts">
					{/* <!-- Mini Post --> */}
					<NextHardhead/>					
					<AwardsSide />					
					<StatisticSide />
					<RulesSide />
				</div>
			</section>
			{/* <!-- Posts List --> */}
			<section>
				<ul className="posts">
					{/* <YearsSide/> */}
				</ul>
			</section>

			<section>
				<ul className="posts">					
					<li>
						<article>
							<header>
								<h3><a href="http://www.hress.org/Hardhead/Films.aspx" target="_parent">Myndirnar</a></h3>
							</header>
						</article>
					</li>
					<li>
						<article>
							<header>
								<h3><a href="http://www.hress.org/Hardhead/defaultold.aspx" target="_parent">Gamla harðhausasíðan</a></h3>
							</header>
						</article>
					</li>
				</ul>
			</section>

			{/* <!-- About --> */}
			<section className="blurb">
				<h2>Um Harðhaus</h2>
				<p>Harðhausakvöld eru haldin einu sinni í mánuði. Á kvöldinu er horft á harðhausamynd.</p>
				<ul className="actions">
					<li><a href="http://www.hress.org/Hardhead/Rules.aspx" className="button">Lestu reglurnar</a></li>
				</ul>
			</section>

			{/* <!-- Footer --> */}
			<section id="footer">
				<ul className="icons">
					{/* <li><a href="#" className="icon brands fa-twitter"><span className="label">Twitter</span></a></li> */}
					<li><a href="https://www.facebook.com/groups/988193164623754/" className="icon brands fa-facebook-f" target="_blank" rel="noopener noreferrer"><span className="label">Facebook</span></a></li>
					{/* <li><a href="#" className="icon brands fa-instagram"><span className="label">Instagram</span></a></li>
					<li><a href="#" className="icon solid fa-rss"><span className="label">RSS</span></a></li>
					<li><a href="#" className="icon solid fa-envelope"><span className="label">Email</span></a></li> */}
				</ul>
				<p className="copyright">&copy; Hress.Org - Guðjón Karl Arnarson.<br/>Design: <a href="http://html5up.net">HTML5 UP</a>.</p>
			</section>
        </section>    
    );
}

export default HardheadSidebar;
