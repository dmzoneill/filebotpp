using System.Collections.Generic;
using FileBotPP.Interfaces;

namespace FileBotPP.Metadata.tvdb
{
    public class Season : ISeason
    {
        private readonly int _num;
	    private readonly List<IEpisode> _episodes;
		
	    public Season( int num )
        {
		    this._num = num;
            this._episodes = new List<IEpisode>();
        }

        public int get_season_num()
        {
            return this._num;
        }

        public void add_episode( IEpisode episode )
        {
		    this._episodes.Add( episode );
        }

        public List<IEpisode> get_episodes()
        {
            return this._episodes;
        }
    }
}
