using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace movierecommender.Models
{
    public class ProfileService
    {
        public List<Profile> _profile = new List<Profile>(LoadProfileData());

        public int _activeprofileid = -1;       

        public List<Tuple<int, int>> GetProfileWatchedMovies(int id)
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

            Stream fileReader = File.OpenRead("Content/Profiles.csv");
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
                    int ProfileID = Int32.Parse(fields[0].ToString().TrimStart(new char[] { '0' }));
                    String ProfileImageName = fields[1].ToString();
                    string ProfileName = fields[2].ToString();
                    List<Tuple<int, int>> ratings = new List<Tuple<int, int>>();
                    for (int i = 3; i < fields.Length; i+=2) {
                    ratings.Add(Tuple.Create(Int32.Parse(fields[i]), Int32.Parse(fields[i+1])));
                    }
                    result.Add(new Profile() { ProfileID = ProfileID, ProfileImageName= ProfileImageName, ProfileName = ProfileName, ProfileMovieRatings = ratings });
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
