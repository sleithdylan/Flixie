using Flixie.Models.Database;
using Flixie.Models.TMDB;
using System.Collections.Generic;

namespace Flixie.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Collection> CustomCollection { get; set; }
        public MovieSearch NowPlaying { get; set; }
        public MovieSearch Popular { get; set; }
        public MovieSearch TopRated { get; set; }
        public MovieSearch Upcoming { get; set; }
    }
}
