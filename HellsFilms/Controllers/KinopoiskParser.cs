using HtmlAgilityPack;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HellsFilms.Controllers
{
    public class KinopoiskParser
    {
        private readonly HttpClient _httpClient;

        public KinopoiskParser(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<Film>> Parse(string query)
        {
            // Получить ответ от API Кинопоиска
            var client = new HttpClient();
            var response = await client.GetAsync("https://www.kinopoisk.ru/index.php?kp_query=" + Uri.EscapeDataString(query));

            // Отладочный вывод
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Kinopoisk response content: {responseBody}");

            // Обработать ответ
            var document = new HtmlDocument();
            document.LoadHtml(responseBody);

            // Найти все элементы с классом "element"
            var items = document.DocumentNode.SelectNodes("//div[contains(@class, 'element') or contains(@class, 'search_results')]");

            // Проверка на null
            if (items != null)
            {
                // Создать список фильмов
                var films = new List<Film>();

                // Для каждого элемента
                foreach (var item in items)
                {
                    // Проверить, является ли элемент фильмом (например, по наличию класса)
                    if (item.SelectSingleNode(".//p[@class='name']/a") != null)
                    {
                        // Создать фильм
                        var film = new Film();

                        // Найти заголовок фильма
                        var titleNode = item.SelectSingleNode(".//p[@class='name']/a");
                        film.Title = titleNode?.InnerText.Trim();

                        // Найти год выпуска фильма
                        var year = item.SelectSingleNode(".//span[@class='year']");
                        film.Year = year?.InnerText.Trim();

                        // Найти оценку фильма
                        var rating = item.SelectSingleNode(".//div[contains(@class, 'rating')]");
                        film.Rating = rating?.InnerText.Trim();
                        // Найти ссылку на фильм, содержащую ID
                        var idLink = titleNode?.Attributes["data-id"]?.Value;
                        film.Id = idLink;

                        // Найти ссылку на изображение
                        var image = item.SelectSingleNode(".//img[contains(@class, 'flap_img')]");
                        var imageUrl = image?.GetAttributeValue("src", "").Trim();

                        // Проверить идентификатор и сформировать URL изображения
                        if (!string.IsNullOrEmpty(film.Id) && !string.IsNullOrEmpty(imageUrl) && !imageUrl.Contains("spacer.jpg"))
                        {
                            film.ImageUrl = $"https://st.kp.yandex.net/images/film_iphone/iphone360_{film.Id}.jpg";
                        }

                        // Вывести ID отдельно
                        Console.WriteLine($"Film ID: {film.Id}");
                        Console.WriteLine($"Film Image URL: {film.ImageUrl}");
                        if (rating != null && !string.IsNullOrEmpty(rating.InnerText))
                        {
                            films.Add(film);
                        }

                    }
                }

                // Вернуть список фильмов
                return films
                    .GroupBy(f => new { f.Title, f.Year, f.Rating }) // Группировка по заголовку, году, рейтингу и фотографии
                    .Select(group => group.First()) // Выбор первого фильма из каждой группы (удаление дубликатов)
                    .ToList();
            }
            else
            {
                // Если items равен null, вернуть пустой список фильмов
                return new List<Film>();
            }
        }

        // Добавьте свойство для хранения ссылки на изображение в класс Film
        public class Film
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Year { get; set; }
            public string Rating { get; set; }
            public string ImageUrl { get; set; } // Новое свойство для хранения ссылки на изображение
        }
    }
}
