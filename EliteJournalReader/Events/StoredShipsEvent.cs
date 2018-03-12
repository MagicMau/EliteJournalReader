using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //    When written: when visiting shipyard

    //    Parameters:
    //�	MarketID
    //�	StationName
    //�	StarSystem
    //�	ShipsHere: (array of objects)
    //o ShipID
    //o ShipType
    //o Name(if named)
    //o Value
    //�	ShipsRemote: (array of objects)
    //o ShipID
    //o ShipType
    //o Name(if named)
    //o Value
    //o StarSystem
    //o ShipMarketID
    //o TransferPrice
    //o TransferType
    public class StoredShipsEvent : JournalEvent<StoredShipsEvent.StoredShipsEventArgs>
    {
        public StoredShipsEvent() : base("StoredShips") { }

        public class StoredShipsEventArgs : JournalEventArgs
        {
            public struct StoredShip
            {
                public int ShipID;
                public string ShipType;
                public string ShipType_Localised;
                public string Name;
                public int Value;
                public bool Hot;
                public string StarSystem;
                public long ShipMarketID;
                public int TransferPrice;
                public string TransferType;
            }

            public long MarketID { get; set; }
            public string StationName { get; set; }
            public string StarSystem { get; set; }
            public List<StoredShip> ShipsHere { get; set; }
            public List<StoredShip> ShipsRemote { get; set; }
        }
    }
}