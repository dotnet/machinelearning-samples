using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace movierecommender.Models
{
    public class MovieService
    {
        public readonly static int _moviesToRecommend = 6;
        public readonly static int _trendingmovies = 20;
        public Lazy<List<Movie>> _movies = new Lazy<List<Movie>>(() => LoadMovieData());
        public List<Movie> _trendingMovies = LoadTrendingMovies();
        public readonly static string _modelpath = @".\Models\model.zip";

        public static List<Movie> LoadTrendingMovies() {
            var result = new List<Movie>();

            result.Add(new Movie { MovieID = 1573, MovieName = "Face/Off (1997)" });
            result.Add(new Movie { MovieID = 1721,  MovieName = "Titanic (1997)" });
            result.Add(new Movie { MovieID = 1703, MovieName = "Home Alone 3 (1997)" });
            result.Add(new Movie { MovieID = 49272, MovieName = "Casino Royale (2006)" });
            result.Add(new Movie { MovieID = 5816, MovieName = "Harry Potter and the Chamber of Secrets (2002)" });
            result.Add(new Movie { MovieID = 3578, MovieName = "Gladiator (2000)" });
            return result;
        }

        public string GetModelPath()
        {
            return _modelpath;
        }

        public IEnumerable<Movie> GetSomeSuggestions()
        {
            var movies = GetRecentMovies().ToArray();

            var rnd = new Random();
            var movieselector = new int[_moviesToRecommend];

            for (int i = 0; i < _moviesToRecommend; i++)
            {
                movieselector[i] = rnd.Next(movies.Length);
            }

            return movieselector.Select(s => movies[s]);
        }

        public IEnumerable<Movie> GetRecentMovies()
        {
            return GetAllMovies()
                .Where(m => m.MovieName.Contains("20")
                            || m.MovieName.Contains("198")
                            || m.MovieName.Contains("199"));
        }

        public Movie Get(int id)
        {
            return _movies.Value.Single(m => m.MovieID == id);
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return _movies.Value;
        }

        private static List<Movie> LoadMovieData()
        {
            var result = new List<Movie>();

            var fileReader = File.OpenRead("Content/movies.csv");

            var reader = new StreamReader(fileReader);
            try
            {
                bool header = true;
                int index = 0;
                var line = "";
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        line = reader.ReadLine();
                        header = false;
                    }
                    line = reader.ReadLine();
                    var fields = line.Split(',');
                    var MovieID = int.Parse(fields[0].TrimStart(new char[] { '0' }));
                    var MovieName = fields[1];
                    result.Add(new Movie() { MovieID = MovieID, MovieName = MovieName });
                    index++;
                }
            }
            finally
            {
                reader?.Dispose();
            }

            return result;
        }
    }
}