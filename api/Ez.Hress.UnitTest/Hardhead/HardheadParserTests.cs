using Ez.Hress.Hardhead.Entities.InputModels;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ez.Hress.UnitTest.Hardhead
{   
    public class HardheadParserTests
    {
        private readonly Mock<ILogger<HardheadParser>> _log;

        public HardheadParserTests()
        {
            _log = new Mock<ILogger<HardheadParser>>();
        }

        [Fact]
        public void ParseMovieInfoDieHardTest()
        {
            // ARRANGE
            var json = "{\t\"Title\": \"Die Hard\",\t\"Year\": \"1988\",\t\"Rated\": \"R\",\t\"Released\": \"20 Jul 1988\",\t\"Runtime\": \"132 min\",\t\"Genre\": \"Action, Thriller\",\t\"Director\": \"John McTiernan\",\t\"Writer\": \"Roderick Thorp, Jeb Stuart, Steven E. de Souza\",\t\"Actors\": \"Bruce Willis, Alan Rickman, Bonnie Bedelia\",\t\"Plot\": \"An NYPD officer tries to save his wife and several others taken hostage by German terrorists during a Christmas party at the Nakatomi Plaza in Los Angeles.\",\t\"Language\": \"English, German, Italian, Japanese\",\t\"Country\": \"United States\",\t\"Awards\": \"Nominated for 4 Oscars. 8 wins & 6 nominations total\",\t\"Poster\": \"https://m.media-amazon.com/images/M/MV5BZjRlNDUxZjAtOGQ4OC00OTNlLTgxNmQtYTBmMDgwZmNmNjkxXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_SX300.jpg\",\t\"Ratings\": [\t\t{\t\t\t\"Source\": \"Internet Movie Database\",\t\t\t\"Value\": \"8.2/10\"\t\t},\t\t{\t\t\t\"Source\": \"Rotten Tomatoes\",\t\t\t\"Value\": \"94%\"\t\t},\t\t{\t\t\t\"Source\": \"Metacritic\",\t\t\t\"Value\": \"72/100\"\t\t}\t],\t\"Metascore\": \"72\",\t\"imdbRating\": \"8.2\",\t\"imdbVotes\": \"834,704\",\t\"imdbID\": \"tt0095016\",\t\"Type\": \"movie\",\t\"DVD\": \"28 Dec 1999\",\t\"BoxOffice\": \"$83,844,093\",\t\"Production\": \"N/A\",\t\"Website\": \"N/A\",\t\"Response\": \"True\"}";
            var movieInfoInput = JsonConvert.DeserializeObject<MovieInfoModel>(json);
            var parser = new HardheadParser(_log.Object);

            // ACT
            Assert.NotNull(movieInfoInput);
            var movieInfo = parser.ParseMovieInfo(movieInfoInput); 

            // ASSERT
            Assert.NotNull(movieInfo);
            Assert.Equal(movieInfoInput.Title, movieInfo.Name);
            Assert.Equal(movieInfoInput.Rated, movieInfo.Rated);            
            Assert.Equal(movieInfoInput.Plot, movieInfo.Description);
        }

        [Fact]
        public void ParseMovieInfoShogunAssassinTest()
        {
            // ARRANGE
            var json = "{\r\n  \"Title\": \"Shogun Assassin\",\r\n  \"Year\": \"1980\",\r\n  \"Rated\": \"R\",\r\n  \"Released\": \"07 Nov 1980\",\r\n  \"Runtime\": \"85 min\",\r\n  \"Genre\": \"Action, Adventure\",\r\n  \"Director\": \"Robert Houston, Kenji Misumi\",\r\n  \"Writer\": \"Robert Houston, Kazuo Koike, Goseki Kojima\",\r\n  \"Actors\": \"Tomisaburô Wakayama, Kayo Matsuo, Minoru Ôki\",\r\n  \"Plot\": \"When the wife of the Shogun's Decapitator is murdered and he is ordered to commit suicide by the paranoid Shogun, he and his four-year-old son escape and become assassins for hire, embarking on a journey of blood and violent death.\",\r\n  \"Language\": \"Japanese\",\r\n  \"Country\": \"Japan\",\r\n  \"Awards\": \"N/A\",\r\n  \"Poster\": \"https://m.media-amazon.com/images/M/MV5BZWE5NTZjMWQtNWVmZC00ZmIzLWExNzEtMWMxMjMxNmU2OTEyXkEyXkFqcGdeQXVyMTQxNzMzNDI@._V1_SX300.jpg\",\r\n  \"Ratings\": [\r\n    { \"Source\": \"Internet Movie Database\", \"Value\": \"7.3/10\" },\r\n    { \"Source\": \"Rotten Tomatoes\", \"Value\": \"82%\" }\r\n  ],\r\n  \"Metascore\": \"N/A\",\r\n  \"imdbRating\": \"7.3\",\r\n  \"imdbVotes\": \"11,433\",\r\n  \"imdbID\": \"tt0081506\",\r\n  \"Type\": \"movie\",\r\n  \"DVD\": \"N/A\",\r\n  \"BoxOffice\": \"N/A\",\r\n  \"Production\": \"N/A\",\r\n  \"Website\": \"N/A\",\r\n  \"Response\": \"True\"\r\n}";
            var movieInfoInput = JsonConvert.DeserializeObject<MovieInfoModel>(json);
            var parser = new HardheadParser(_log.Object);

            // ACT
            Assert.NotNull(movieInfoInput);
            var movieInfo = parser.ParseMovieInfo(movieInfoInput);

            // ASSERT
            Assert.NotNull(movieInfo);
            Assert.Equal(movieInfoInput.Title, movieInfo.Name);
            Assert.Equal(movieInfoInput.Rated, movieInfo.Rated);
            Assert.Equal(movieInfoInput.Plot, movieInfo.Description);
        }
    }
}
