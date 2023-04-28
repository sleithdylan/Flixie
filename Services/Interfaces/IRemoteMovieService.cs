using Flixie.Enums;
using Flixie.Models.TMDB;
using System.Threading.Tasks;

namespace Flixie.Services.Interfaces
{
    public interface IRemoteMovieService
    {
        Task<MovieDetail> MovieDetailAsync(int id);
        Task<MovieSearch> SearchMoviesAsync(MovieCategory category, int count);
        Task<ActorDetail> ActorDetailAsync(int id);
    }
}
