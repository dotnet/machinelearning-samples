using movierecommender.Models;
using System.Collections.Generic;

namespace movierecommender.Services
{
    public interface IMovieService
    {
        Movie Get(int id);
        IEnumerable<Movie> GetAllMovies();
        string GetModelPath();
        IEnumerable<Movie> GetRecentMovies();
        IEnumerable<Movie> GetSomeSuggestions();

        List<Movie> GetTrendingMovies { get; }
    }
}