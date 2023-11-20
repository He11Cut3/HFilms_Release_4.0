namespace HellsFilms.Models.ViewModels
{
    public class SearchAndSimilarViewModel
    {
        public List<BasicFilm> SearchResults { get; set; }
        public List<BasicFilm> SimilarMovies { get; set; }
    }
}