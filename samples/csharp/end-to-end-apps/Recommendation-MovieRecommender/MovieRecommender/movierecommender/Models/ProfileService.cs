using System.Collections.Generic;
using System.IO;

namespace movierecommender.Models
{
    public class ProfileService
    {
        public List<Profile> _profile = new List<Profile>(LoadProfileData());

        public int _activeprofileid = -1;

        public List<(int movieId, int movieRating)> GetProfileWatchedMovies(int id)
        {
            foreach(var Profile in _profile)
            {
                if (id == Profile.ProfileID)
                {
                    return Profile.ProfileMovieRatings;
                }
            }

            return null;
        }

        public Profile GetProfileByID(int id)
        {
            foreach (var Profile in _profile)
            {
                if (id == Profile.ProfileID)
                {
                    return Profile;
                }
            }

            return null;
        }

        private static List<Profile> LoadProfileData()
        {
            var result = new List<Profile>();

            var fileReader = File.OpenRead("Content/Profiles.csv");
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
                    var ProfileID = int.Parse(fields[0].TrimStart(new char[] { '0' }));
                    var ProfileImageName = fields[1];
                    var ProfileName = fields[2];

                    var ratings = new List<(int movieId, int movieRating)>();

                    for (int i = 3; i < fields.Length; i+=2)
                    {
                        ratings.Add((int.Parse(fields[i]), int.Parse(fields[i+1])));
                    }
                    result.Add(new Profile()
                    {
                        ProfileID = ProfileID,
                        ProfileImageName = ProfileImageName,
                        ProfileName = ProfileName,
                        ProfileMovieRatings = ratings
                    });
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
