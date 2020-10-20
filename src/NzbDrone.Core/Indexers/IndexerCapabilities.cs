using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers
{
    public enum TvSearchParam
    {
        Q,
        Season,
        Ep,
        ImdbId,
        TvdbId,
        RId,
    }

    public enum MovieSearchParam
    {
        Q,
        ImdbId,
        TmdbId
    }

    public enum MusicSearchParam
    {
        Q,
        Album,
        Artist,
        Label,
        Year
    }

    public class IndexerCapabilities
    {
        public int? LimitsMax { get; set; }
        public int? LimitsDefault { get; set; }

        public bool SearchAvailable { get; set; }

        public List<TvSearchParam> TvSearchParams;
        public bool TvSearchAvailable => TvSearchParams.Count > 0;
        public bool TvSearchSeasonAvailable => TvSearchParams.Contains(TvSearchParam.Season);
        public bool TvSearchEpAvailable => TvSearchParams.Contains(TvSearchParam.Ep);
        public bool TvSearchImdbAvailable => TvSearchParams.Contains(TvSearchParam.ImdbId);
        public bool TvSearchTvdbAvailable => TvSearchParams.Contains(TvSearchParam.TvdbId);
        public bool TvSearchTvRageAvailable => TvSearchParams.Contains(TvSearchParam.RId);

        public List<MovieSearchParam> MovieSearchParams;
        public bool MovieSearchAvailable => MovieSearchParams.Count > 0;
        public bool MovieSearchImdbAvailable => MovieSearchParams.Contains(MovieSearchParam.ImdbId);
        public bool MovieSearchTmdbAvailable => MovieSearchParams.Contains(MovieSearchParam.TmdbId);

        public List<MusicSearchParam> MusicSearchParams;
        public bool MusicSearchAvailable => MusicSearchParams.Count > 0;
        public bool MusicSearchAlbumAvailable => MusicSearchParams.Contains(MusicSearchParam.Album);
        public bool MusicSearchArtistAvailable => MusicSearchParams.Contains(MusicSearchParam.Artist);
        public bool MusicSearchLabelAvailable => MusicSearchParams.Contains(MusicSearchParam.Label);
        public bool MusicSearchYearAvailable => MusicSearchParams.Contains(MusicSearchParam.Year);

        public bool BookSearchAvailable { get; set; }

        public List<IndexerCategory> Categories { get; private set; }

        public IndexerCapabilities()
        {
            SearchAvailable = true;
            TvSearchParams = new List<TvSearchParam>();
            MovieSearchParams = new List<MovieSearchParam>();
            MusicSearchParams = new List<MusicSearchParam>();
            BookSearchAvailable = false;
            Categories = new List<IndexerCategory>();
        }

        public void ParseTvSearchParams(IEnumerable<string> paramsList)
        {
            if (paramsList == null)
            {
                return;
            }

            foreach (var paramStr in paramsList)
            {
                if (Enum.TryParse(paramStr, true, out TvSearchParam param))
                {
                    if (!TvSearchParams.Contains(param))
                    {
                        TvSearchParams.Add(param);
                    }
                    else
                    {
                        throw new Exception($"Duplicate tv-search param: {paramStr}");
                    }
                }
                else
                {
                    throw new Exception($"Not supported tv-search param: {paramStr}");
                }
            }
        }

        public void ParseMovieSearchParams(IEnumerable<string> paramsList)
        {
            if (paramsList == null)
            {
                return;
            }

            foreach (var paramStr in paramsList)
            {
                if (Enum.TryParse(paramStr, true, out MovieSearchParam param))
                {
                    if (!MovieSearchParams.Contains(param))
                    {
                        MovieSearchParams.Add(param);
                    }
                    else
                    {
                        throw new Exception($"Duplicate movie-search param: {paramStr}");
                    }
                }
                else
                {
                    throw new Exception($"Not supported movie-search param: {paramStr}");
                }
            }
        }

        public void ParseMusicSearchParams(IEnumerable<string> paramsList)
        {
            if (paramsList == null)
            {
                return;
            }

            foreach (var paramStr in paramsList)
            {
                if (Enum.TryParse(paramStr, true, out MusicSearchParam param))
                {
                    if (!MusicSearchParams.Contains(param))
                    {
                        MusicSearchParams.Add(param);
                    }
                    else
                    {
                        throw new Exception($"Duplicate music-search param: {paramStr}");
                    }
                }
                else
                {
                    throw new Exception($"Not supported Music-search param: {paramStr}");
                }
            }
        }

        private string SupportedTvSearchParams()
        {
            var parameters = new List<string> { "q" }; // q is always enabled
            if (TvSearchSeasonAvailable)
            {
                parameters.Add("season");
            }

            if (TvSearchEpAvailable)
            {
                parameters.Add("ep");
            }

            if (TvSearchImdbAvailable)
            {
                parameters.Add("imdbid");
            }

            if (TvSearchTvdbAvailable)
            {
                parameters.Add("tvdbid");
            }

            if (TvSearchTvRageAvailable)
            {
                parameters.Add("rid");
            }

            return string.Join(",", parameters);
        }

        private string SupportedMovieSearchParams()
        {
            var parameters = new List<string> { "q" }; // q is always enabled
            if (MovieSearchImdbAvailable)
            {
                parameters.Add("imdbid");
            }

            if (MovieSearchTmdbAvailable)
            {
                parameters.Add("tmdbid");
            }

            return string.Join(",", parameters);
        }

        private string SupportedMusicSearchParams()
        {
            var parameters = new List<string> { "q" }; // q is always enabled
            if (MusicSearchAlbumAvailable)
            {
                parameters.Add("album");
            }

            if (MusicSearchArtistAvailable)
            {
                parameters.Add("artist");
            }

            if (MusicSearchLabelAvailable)
            {
                parameters.Add("label");
            }

            if (MusicSearchYearAvailable)
            {
                parameters.Add("year");
            }

            return string.Join(",", parameters);
        }

        private string SupportedBookSearchParams
        {
            get
            {
                var parameters = new List<string>() { "q" };
                if (BookSearchAvailable)
                {
                    parameters.Add("author,title");
                }

                return string.Join(",", parameters);
            }
        }

        public bool SupportsCategories(int[] categories)
        {
            var subCategories = Categories.SelectMany(c => c.SubCategories);
            var allCategories = Categories.Concat(subCategories);
            var supportsCategory = allCategories.Any(i => categories.Any(c => c == i.ID));
            return supportsCategory;
        }

        public XDocument GetXDocument()
        {
            var xdoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("caps",
                    new XElement("server",
                        new XAttribute("title", "Prowlarr")),
                    LimitsMax != null || LimitsDefault != null ?
                        new XElement("limits",
                            LimitsMax != null ? new XAttribute("max", LimitsMax) : null,
                            LimitsDefault != null ? new XAttribute("default", LimitsDefault) : null)
                    : null,
                    new XElement("searching",
                        new XElement("search",
                            new XAttribute("available", SearchAvailable ? "yes" : "no"),
                            new XAttribute("supportedParams", "q")),
                        new XElement("tv-search",
                            new XAttribute("available", TvSearchAvailable ? "yes" : "no"),
                            new XAttribute("supportedParams", SupportedTvSearchParams())),
                        new XElement("movie-search",
                            new XAttribute("available", MovieSearchAvailable ? "yes" : "no"),
                            new XAttribute("supportedParams", SupportedMovieSearchParams())),
                        new XElement("music-search",
                            new XAttribute("available", MusicSearchAvailable ? "yes" : "no"),
                            new XAttribute("supportedParams", SupportedMusicSearchParams())),
                        new XElement("audio-search",
                            new XAttribute("available", MusicSearchAvailable ? "yes" : "no"),
                            new XAttribute("supportedParams", SupportedMusicSearchParams())),
                        new XElement("book-search",
                            new XAttribute("available", BookSearchAvailable ? "yes" : "no"),
                            new XAttribute("supportedParams", SupportedBookSearchParams))),
                    new XElement("categories",
                        from c in Categories.OrderBy(x => x.ID < 100000 ? "z" + x.ID.ToString() : x.Name)
                        select new XElement("category",
                            new XAttribute("id", c.ID),
                            new XAttribute("name", c.Name),
                            from sc in c.SubCategories
                            select new XElement("subcat",
                                new XAttribute("id", sc.ID),
                                new XAttribute("name", sc.Name))))));
            return xdoc;
        }

        public string ToXml() =>
            GetXDocument().Declaration + Environment.NewLine + GetXDocument();

        public static IndexerCapabilities Concat(IndexerCapabilities left, IndexerCapabilities right)
        {
            left.SearchAvailable = left.SearchAvailable || right.SearchAvailable;
            left.TvSearchParams = left.TvSearchParams.Union(right.TvSearchParams).ToList();
            left.MovieSearchParams = left.MovieSearchParams.Union(right.MovieSearchParams).ToList();
            left.MusicSearchParams = left.MusicSearchParams.Union(right.MusicSearchParams).ToList();
            left.BookSearchAvailable = left.BookSearchAvailable || right.BookSearchAvailable;
            left.Categories.AddRange(right.Categories.Where(x => x.ID < 100000).Except(left.Categories)); // exclude indexer specific categories (>= 100000)
            return left;
        }
    }
}