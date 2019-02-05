using movierecommender.Models;
using System.Collections.Generic;

namespace movierecommender.Services
{
    public interface IProfileService
    {
        Profile GetProfileByID(int id);

        List<(int movieId, int movieRating)> GetProfileWatchedMovies(int id);

        List<Profile> GetProfiles { get; }
    }
}