import React from 'react';
// import UsersStatistics from '../userstats';
import { Intro } from '../../../components';
import NextHardhead from './nexthardhead';
// import './App.css';

function HardheadSidebar() {
    return (
        <section id="sidebar">
            {/* Intro */}
            {/* <section id="intro">
                <a href="#" className="logo"><img src="https://hress.azurewebsites.net/App_Themes/Default/Images/Logo.png" alt="" /></a>
                <header>
                    <h2>Harðhaus</h2>
                    <p>Þar sem hressleikinn býr</p>
                </header>
            </section> */}
            <Intro logo="https://hress.azurewebsites.net/App_Themes/Default/Images/Logo.png" title="Harðhaus" description="" />

            {/* <!-- Mini Posts --> */}
							<section>
								<div className="mini-posts">

									{/* <!-- Mini Post --> */}
									<NextHardhead/>
                                    {/* <UsersStatistics/> */}

										{/* <article className="mini-post">
											<header>
												<h3><a href="single.html">Vitae sed condimentum</a></h3>
												<time className="published" datetime="2015-10-20">October 20, 2015</time>
												<a href="#" className="author"><img src="images/avatar.jpg" alt="" /></a>
											</header>
											<a href="single.html" className="image"><img src="images/pic04.jpg" alt="" /></a>
										</article> */}

									{/* <!-- Mini Post --> */}
										{/* <article className="mini-post">
											<header>
												<h3><a href="single.html">Rutrum neque accumsan</a></h3>
												<time className="published" dateTime="2015-10-19">October 19, 2015</time>
												<a href="#" className="author"><img src="images/avatar.jpg" alt="" /></a>
											</header>
											<a href="single.html" className="image"><img src="images/pic05.jpg" alt="" /></a>
										</article> */}

									{/* <!-- Mini Post --> */}
										{/* <article className="mini-post">
											<header>
												<h3><a href="single.html">Odio congue mattis</a></h3>
												<time className="published" dateTime="2015-10-18">October 18, 2015</time>
												<a href="#" className="author"><img src="images/avatar.jpg" alt="" /></a>
											</header>
											<a href="single.html" className="image"><img src="images/pic06.jpg" alt="" /></a>
										</article> */}

									{/* <!-- Mini Post --> */}
										{/* <article className="mini-post">
											<header>
												<h3><a href="single.html">Enim nisl veroeros</a></h3>
												<time className="published" dateTime="2015-10-17">October 17, 2015</time>
												<a href="#" className="author"><img src="images/avatar.jpg" alt="" /></a>
											</header>
											<a href="single.html" className="image"><img src="images/pic07.jpg" alt="" /></a>
										</article> */}

								</div>
							</section>
                            {/* <!-- Posts List --> */}
							<section>
								<ul className="posts">
									<li>
										<article>
											<header>
												<h3><a href="http://www.hress.org/Hardhead/Awards.aspx">Verðlaun</a></h3>
											</header>
										</article>
									</li>
									<li>
										<article>
											<header>
												<h3><a href="http://www.hress.org/Hardhead/Rules.aspx">Lög og reglur</a></h3>
											</header>
										</article>
									</li>
									<li>
										<article>
											<header>
												<h3><a href="http://www.hress.org/Hardhead/Films.aspx">Myndirnar</a></h3>
											</header>
										</article>
									</li>
									{/* <li>
										<article>
											<header>
												<h3><a href="single.html">Convallis maximus nisl mattis nunc id lorem</a></h3>
												<time class="published" datetime="2015-10-15">October 15, 2015</time>
											</header>
											<a href="single.html" class="image"><img src="images/pic09.jpg" alt="" /></a>
										</article>
									</li>
									<li>
										<article>
											<header>
												<h3><a href="single.html">Euismod amet placerat vivamus porttitor</a></h3>
												<time class="published" datetime="2015-10-10">October 10, 2015</time>
											</header>
											<a href="single.html" class="image"><img src="images/pic10.jpg" alt="" /></a>
										</article>
									</li>
									<li>
										<article>
											<header>
												<h3><a href="single.html">Magna enim accumsan tortor cursus ultricies</a></h3>
												<time class="published" datetime="2015-10-08">October 8, 2015</time>
											</header>
											<a href="single.html" class="image"><img src="images/pic11.jpg" alt="" /></a>
										</article>
									</li>
									<li>
										<article>
											<header>
												<h3><a href="single.html">Congue ullam corper lorem ipsum dolor</a></h3>
												<time class="published" datetime="2015-10-06">October 7, 2015</time>
											</header>
											<a href="single.html" class="image"><img src="images/pic12.jpg" alt="" /></a>
										</article>
									</li> */}
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
									<li><a href="https://www.facebook.com/groups/988193164623754/" className="icon brands fa-facebook-f"><span className="label">Facebook</span></a></li>
									{/* <li><a href="#" className="icon brands fa-instagram"><span className="label">Instagram</span></a></li>
									<li><a href="#" className="icon solid fa-rss"><span className="label">RSS</span></a></li>
									<li><a href="#" className="icon solid fa-envelope"><span className="label">Email</span></a></li> */}
								</ul>
								<p className="copyright">&copy; Hress.Org - Guðjón Karl Arnarson.<br/>Design: <a href="http://html5up.net">HTML5 UP</a>. Images: <a href="http://unsplash.com">Unsplash</a>.</p>
							</section>
        </section>    
    );
}

export default HardheadSidebar;
