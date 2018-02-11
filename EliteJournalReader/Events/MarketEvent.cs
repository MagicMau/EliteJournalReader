using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when accessing the commodity market in a station
    //A separate file market.json is written to the same folder as the journal, containing full market price info
    //Parameters:
    //•	MarketID
    //•	StationName
    //•	StarSystem

    //The separate file also contains:
    //•	Items: array of objects
    //o   id
    //o   Name
    //o   BuyPrice
    //o   SellPrice
    //o   MeanPrice
    //o   StockBracket
    //o   DemandBracket
    //o   Stock
    //o   Demand
    //o   Consumer: bool
    //o   Producer: bool
    //o   Rare: bool
    public class MarketEvent : JournalEvent<MarketEvent.MarketEventArgs>
    {
        public MarketEvent() : base("Market") { }

        public class MarketEventArgs : JournalEventArgs
        {
            public string StationName { get; set; }
            public long MarketID { get; set; }
            public string StarSystem { get; set; }
        }
    }
}
