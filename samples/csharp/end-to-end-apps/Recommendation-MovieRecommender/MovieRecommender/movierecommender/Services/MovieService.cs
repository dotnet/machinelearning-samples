using Microsoft.AspNetCore.Hosting;
using movierecommender.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace movierecommender.Services
{
    public class MovieService : IMovieService
    {
        public readonly static int _moviesToRecommend = 6;
        private readonly static int _trendingMoviesCount = 20;
        public Lazy<List<Movie>> _movies = new Lazy<List<Movie>>(LoadMovieData);
        private List<Movie> _trendingMovies = LoadTrendingMovies();
        public readonly static string _modelpath = @"model.zip";
        private readonly IHostingEnvironment _hostingEnvironment;

        public List<Movie> GetTrendingMovies => LoadTrendingMovies();

        public MovieService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public static List<Movie> LoadTrendingMovies() {
            List<Movie> result = new List<Movie>();

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
            return Path.Combine(_hostingEnvironment.ContentRootPath, "Models", _modelpath);
        }

        public IEnumerable<Movie> GetSomeSuggestions()
        {
            Movie[] movies = GetRecentMovies().ToArray();

            Random rnd = new Random();
            int[] movieselector = new int[_moviesToRecommend];

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
            List<Movie> result = new List<Movie>();

            FileStream fileReader = File.OpenRead("Content/movies.csv");

            StreamReader reader = new StreamReader(fileReader);
            try
            {
                bool header = true;
                int index = 0;
                string line = "";
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        line = reader.ReadLine();
                        header = false;
                    }
                    line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    int MovieID = int.Parse(fields[0].TrimStart(new char[] { '0' }));
                    string MovieName = fields[1];
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