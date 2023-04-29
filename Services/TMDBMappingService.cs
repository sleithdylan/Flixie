using Flixie.Enums;
using Flixie.Models.Database;
using Flixie.Models.Settings;
using Flixie.Models.TMDB;
using Flixie.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Flixie.Services
{
    public class TMDBMappingService : IDataMappingService
    {
        private AppSettings _appSettings;
        private readonly IImageService _imageService;

        public TMDBMappingService(IOptions<AppSettings> appSettings, IImageService imageService)
        {
            _appSettings = appSettings.Value;
            _imageService = imageService;
        }

        public ActorDetail MapActorDetail(ActorDetail actor)
        {
            actor.biography = "Not Available";

            if (string.IsNullOrEmpty(actor.biography))
            {
                actor.biography = "Not Available";
            }

            if (string.IsNullOrEmpty(actor.place_of_birth))
            {
                actor.place_of_birth = "Not Available";
            }

            if (string.IsNullOrEmpty(actor.birthday))
            {
                actor.birthday = "Not Available";
            } 
            else
            {
                actor.birthday = DateTime.Parse(actor.birthday).ToString("MMM dd, yyyy");
            }

            return actor;
        }

        public async Task<Movie> MapMovieDetailAsync(MovieDetail movie)
        {
            Movie newMovie = null;

            try
            {
                newMovie = new Movie()
                {
                    MovieId = movie.id,
                    Title = movie.title,
                    TagLine = movie.tagline,
                    Overview = movie.overview,
                    RunTime = movie.runtime,
                    VoteAverage = movie.vote_average,
                    ReleaseDate = DateTime.Parse(movie.release_date),
                    TrailerUrl = BuildTrailerPath(movie.videos),
                    Backdrop = await EncodeBackdropImageAsync(movie.backdrop_path),
                    BackdropType = BuildImageType(movie.backdrop_path),
                    Poster = await EncoderPosterImageAsync(movie.poster_path),
                    PosterType = BuildImageType(movie.poster_path),
                    Rating = GetRating(movie.release_dates)
                };

                var castMembers = movie.credits.cast
                    .OrderByDescending(c => c.popularity)
                    .GroupBy(c => c.cast_id)
                    .Select(g => g.FirstOrDefault())
                    .Take(20)
                    .ToList();

                castMembers.ForEach(member =>
                {
                    newMovie.Cast.Add(new MovieCast()
                    {
                        CastId = member.id,
                        Department = member.known_for_department,
                        Name = member.name,
                        Character = member.character,
                        ImageUrl = BuildCastImage(member.profile_path)
                    });
                });

                var crewMembers = movie.credits.crew
                    .OrderByDescending(c => c.popularity)
                    .GroupBy(c => c.id)
                    .Select(g => g.First())
                    .Take(20)
                    .ToList();

                crewMembers.ForEach(member =>
                {
                    newMovie.Crew.Add(new MovieCrew()
                    {
                        CrewId = member.id,
                        Department = member.department,
                        Name = member.name,
                        Job = member.job,
                        ImageUrl = BuildCastImage(member.profile_path)
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return newMovie;
        }

        private string BuildCastImage(string profilePath)
        {
            if (string.IsNullOrEmpty(profilePath))
            {
                return _appSettings.FlixieSettings.DefaultCastImage;
            }

            return $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.FlixieSettings.DefaultPosterSize}/{profilePath}";
        }

        private string BuildTrailerPath(Videos videos)
        {
            var videoKey = videos.results.FirstOrDefault(r => r.type.ToLower().Trim() == "trailer" && r.key != "")?.key;

            return string.IsNullOrEmpty(videoKey) ? videoKey : $"{_appSettings.TMDBSettings.BaseYoutubePath}{videoKey}";
        }

        private async Task<byte[]> EncodeBackdropImageAsync(string path)
        {
            var backdropPath = $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.FlixieSettings.DefaultBackdropSize}/{path}";

            return await _imageService.EncodeImageURLAsync(backdropPath);
        }

        private string BuildImageType(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return $"image/{Path.GetExtension(path).TrimStart('.')}";
        }

        private async Task<byte[]> EncoderPosterImageAsync(string path)
        {
            var posterPath = $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.FlixieSettings.DefaultPosterSize}/{path}";

            return await _imageService.EncodeImageURLAsync(posterPath);
        }

        private MovieRating GetRating(Release_Dates dates)
        {
            var movieRating = MovieRating.NOT_RATED;
            var certification = dates.results.FirstOrDefault(r => r.iso_3166_1 == "IE");

            if (certification != null)
            {
                var apiRating = certification.release_dates.FirstOrDefault(c => c.certification != "")?.certification.Replace("-", "");

                if (!string.IsNullOrEmpty(apiRating))
                {
                    movieRating = (MovieRating)Enum.Parse(typeof(MovieRating), apiRating, true);
                }
            }

            return movieRating;
        }
    }
}
