using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Maps;

namespace ProxyStarcraft.Basic
{
    public class BasicMapData
    {
        private List<Area> areas;

        private MapArray<byte> areaGrid;

        private List<Deposit> deposits;

        public BasicMapData(IEnumerable<Area> areas, MapArray<byte> areaGrid, IEnumerable<Deposit> deposits)
        {
            this.areas = new List<Area>(areas);
            this.areaGrid = new MapArray<byte>(areaGrid);
            this.deposits = new List<Deposit>(deposits);
        }

        public IReadOnlyList<Area> Areas => this.areas;

        public MapArray<byte> AreaGrid => this.areaGrid;

        public IReadOnlyList<Deposit> Deposits => this.deposits;
        
        public IReadOnlyList<Deposit> GetControlledDeposits(List<Building> bases)
        {
            // TODO: Allow less-orthodox base placement? This assumes they will always be at the center of the minerals, basically.
            // TODO: Stop using magic numbers for "very close to" everywhere.
            return this.Deposits.Where(d => bases.Any(b => b.GetDistance(d.Center) < 10f)).ToList();
        }
    }
}
