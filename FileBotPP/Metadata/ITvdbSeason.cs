using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    public interface ITvdbSeason
    {
        int get_season_num();
        void add_episode( ITvdbEpisode tvdbEpisode );
        List< ITvdbEpisode > get_episodes();
    }
}