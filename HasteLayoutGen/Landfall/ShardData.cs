using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HasteLayoutGen.Landfall
{
    public struct RunConfig
    {
        public int NrOfLevels { get; set; }
    };


    public static class ShardData
    {
        public static readonly Dictionary<int, RunConfig> Shards = new(){
            { 0, new RunConfig { NrOfLevels = 12 } },
            { 1, new RunConfig { NrOfLevels = 13 } },
            { 2, new RunConfig { NrOfLevels = 14 } },
            { 3, new RunConfig { NrOfLevels = 15 } },
            { 4, new RunConfig { NrOfLevels = 16 } },
            { 5, new RunConfig { NrOfLevels = 17 } },
            { 6, new RunConfig { NrOfLevels = 18 } },
            { 7, new RunConfig { NrOfLevels = 19 } },
            { 8, new RunConfig { NrOfLevels = 20 } },
            { 9, new RunConfig { NrOfLevels = 25 } },
        };
    }

}
