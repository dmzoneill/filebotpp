using System.Collections.Generic;
using FileBotPP.Metadata.tvdb;

namespace FileBotPP.Interfaces
{
    public interface ISeason
    {
        int get_season_num();
        void add_episode( IEpisode episode );
        List<IEpisode> get_episodes();
    }
}