using movierecommender.Models;
using System.Collections.Generic;
using System.IO;

namespace movierecommender.Services
{
    public class ProfileService : IProfileService
    {
        private List<Profile> _profile = new List<Profile>(LoadProfileData());

        public List<Profile> GetProfiles => _profile;


        public int _activeprofileid = -1;

        public List<(int movieId, int movieRating)> GetProfileWatchedMovies(int id)
        {
            foreach(Profile Profile in _profile)
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
            foreach (Profile Profile in _profile)
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
            List<Profile> result = new List<Profile>();

            FileStream fileReader = File.OpenRead("Content/Profiles.csv");
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
                    int ProfileID = int.Parse(fields[0].TrimStart(new char[] { '0' }));
                    string ProfileImageName = fields[1];
                    string ProfileName = fields[2];

                    List<(int movieId, int movieRating)> ratings = new List<(int movieId, int movieRating)>();

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
