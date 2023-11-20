using HellsFilms.Infrastructure;
using HellsFilms.Models;
using HellsFilms.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace HellsFilms.Controllers
{
    public class MovieListController : Controller
    {
        private readonly KinoboxApiClient _kinoboxApiClient;
        private readonly KinopoiskParser _kinopoiskParser; // Добавлен парсер

        public MovieListController(KinoboxApiClient kinoboxApiClient, KinopoiskParser kinopoiskParser)
        {
            _kinoboxApiClient = kinoboxApiClient;
            _kinopoiskParser = kinopoiskParser;
        }

        public async Task<IActionResult> Index()
        {
            var popularMovies = await _kinoboxApiClient.GetPopularFilmsAsync();
            return View(popularMovies);
        }


        public async Task<IActionResult> Search(string query)
        {
            // Парсим результаты поиска с помощью KinopoiskParser
            var searchResults = await _kinopoiskParser.Parse(query);

            // Передаем результаты и запрос в представление
            ViewBag.Query = query;
            return View("SearchResults", searchResults);
        }
        public async Task<IActionResult> Details(string kinopoiskId)
        {
            var players = await _kinoboxApiClient.GetMainPlayersAsync(kinopoiskId);
            var player = players?.FirstOrDefault();

            if (player != null)
            {
                player.KinopoiskId = kinopoiskId; // Установите значение KinopoiskId
                return View("Details", player);
            }
            else
            {
                return View();
            }
        }



    }
}
