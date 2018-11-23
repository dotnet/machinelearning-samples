using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace movierecommender.Models
{
    public class Profile
    {
        public int ProfileID { get; set; }
        public string ProfileImageName { get; set;}
        public string ProfileName { get; set; }
        public List<Tuple<int,int>> ProfileMovieRatings { get; set;}
     }
}
