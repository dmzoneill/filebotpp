using System.Collections.Generic;

namespace FileBotPP.Metadata.tvdb.Interfaces
{
    public interface ISeason
    {
        int get_season_num();
        void add_episode( IEpisode episode );
        List< IEpisode > get_episodes();
    }
}