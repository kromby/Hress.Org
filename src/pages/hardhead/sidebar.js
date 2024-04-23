import React from 'react';
import { Intro } from '../../components';
import './sidebar.css';
import NextHardhead from './components/nexthardhead';
import AwardsSide from './awards/awardsSide';
import StatisticSide from './statistics/statisticsSide';
import RulesSide from './rules/rulesSide';
import YearsSide from './components/yearsSide';
import { Link } from 'react-router-dom';

const HardheadSidebar = () => {
	return (
		<section id="sidebar">
			<Intro logo="/logo.png" title="Harðhaus" description="" />

			{/* <!-- Mini Posts --> */}
			<section>
				<div className="mini-posts">
					{/* <!-- Mini Post --> */}
					<NextHardhead allowEdit="true" />
					{/* <MiniPost 
						title="Harðhausabústaður"
						dateString="19. janúar"						
					/> */}
					<AwardsSide />
					<StatisticSide />
					<RulesSide />
				</div>
			</section>
			{/* <!-- Posts List --> */}
			<section>
				<ul className="posts">
					<YearsSide />
				</ul>
			</section>

			{/* <!-- About --> */}
			<section className="blurb">
				<h2>Um Harðhaus</h2>
				<p>Harðhausakvöld eru haldin einu sinni í mánuði. Á kvöldinu er horft á harðhausamynd.</p>
			</section>

			<section>
				<ul className="posts">
					<li>
						<article>
							<header>
								<Link to={"/Hardhead/films.aspx"} target="_parent">
									<h3>Myndirnar</h3>
								</Link>
							</header>
						</article>
					</li>
					<li>
						<article>
							<header>
								<Link to={"/Hardhead/defaultold.aspx"} target="_parent">
									<h3>Gamla harðhausasíðan</h3>
								</Link>
							</header>
						</article>
					</li>
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
				<p className="copyright">&copy; Hress.Org - Guðjón Karl Arnarson.<br />Design: <a href="http://html5up.net">HTML5 UP</a>.</p>
			</section>
		</section>
	);
}

export default HardheadSidebar;
