using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    internal class TvdbSeason : ITvdbSeason
    {
        private readonly List< ITvdbEpisode > _episodes;
        private readonly int _num;

        public TvdbSeason( int num )
        {
            this._num = num;
            this._episodes = new List< ITvdbEpisode >();
        }

        public int get_season_num()
        {
            return this._num;
        }

        public void add_episode( ITvdbEpisode tvdbEpisode )
        {
            this._episodes.Add( tvdbEpisode );
        }

        public List< ITvdbEpisode > get_episodes()
        {
            return this._episodes;
        }
    }
}