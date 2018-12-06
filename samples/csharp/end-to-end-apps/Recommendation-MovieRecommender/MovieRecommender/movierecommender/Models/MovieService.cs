using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace movierecommender.Models
{
    public partial class MovieService
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
            var result = new List<Movie>();
            
            Stream fileReader = File.OpenRead("Content/movies.csv");

            StreamReader reader = new StreamReader(fileReader);
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
                    string[] fields = line.Split(',');
                    int MovieID = Int32.Parse(fields[0].ToString().TrimStart(new char[] { '0' }));
                    string MovieName = fields[1].ToString();
                    result.Add(new Movie() { MovieID = MovieID, MovieName = MovieName });
                    index++;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose(); 
                }
            }

            return result;
        }
    }
}