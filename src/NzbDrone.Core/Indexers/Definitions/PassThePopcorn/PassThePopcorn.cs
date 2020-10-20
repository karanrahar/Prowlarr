using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcorn : HttpIndexerBase<PassThePopcornSettings>
    {
        public override string Name => "PassThePopcorn";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override IndexerPrivacy Privacy => IndexerPrivacy.Private;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;

        public override IndexerCapabilities Capabilities => new IndexerCapabilities
        {
            MovieSearchParams = new List<MovieSearchParam>
            {
                MovieSearchParam.Q, MovieSearchParam.ImdbId
            }
        };

        public override int PageSize => 50;

        public PassThePopcorn(IHttpClient httpClient,
            ICacheManager cacheManager,
            IIndexerStatusService indexerStatusService,
            IConfigService configService,
            Logger logger)
            : base(httpClient, indexerStatusService, configService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new PassThePopcornRequestGenerator()
            {
                Settings = Settings,
                HttpClient = _httpClient,
                Logger = _logger,
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new PassThePopcornParser(Settings, _logger);
        }
    }
}