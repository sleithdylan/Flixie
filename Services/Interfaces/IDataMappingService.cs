using Flixie.Models.Database;
using Flixie.Models.TMDB;
using System.Threading.Tasks;

namespace Flixie.Services.Interfaces
{
    public interface IDataMappingService
    {
        Task<Movie> MapMovieDetailAsync(MovieDetail movie);
        ActorDetail MapActorDetail(ActorDetail actor);
    }
}
