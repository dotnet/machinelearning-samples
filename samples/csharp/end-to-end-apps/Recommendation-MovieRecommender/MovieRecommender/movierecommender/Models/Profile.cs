using System.Collections.Generic;

namespace movierecommender.Models
{
    public class Profile
    {
        public int ProfileID { get; set; }
        public string ProfileImageName { get; set;}
        public string ProfileName { get; set; }
        public List<(int movieId, int movieRating)> ProfileMovieRatings { get; set;}
     }
}
