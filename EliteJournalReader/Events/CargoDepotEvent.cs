using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when collecting or delivering cargo for a wing mission, or if a wing member updates progress
    //    Parameters:
    //•	MissionID:(int)
    //•	UpdateType:(string) (one of: "Collect", "Deliver", "WingUpdate")
    //•	StartMarketID(int)
    //•	EndMarketID(int)
    //•	ItemsCollected(int)
    //•	ItemsDelivered(int)
    //•	TotalItemsToDeliver(int)
    //•	Progress:(float)
    public class CargoDepotEvent : JournalEvent<CargoDepotEvent.CargoDepotEventArgs>
    {
        public CargoDepotEvent() : base("CargoDepot") { }

        public class CargoDepotEventArgs : JournalEventArgs
        {
            public long MissionID { get; set; }
            public string UpdateType { get; set; }
            public long StartMarketID { get; set; }
            public long EndMarketID { get; set; }
            public int ItemsCollected { get; set; }
            public int ItemsDelivered { get; set; }
            public int TotalItemsToDeliver { get; set; }
            public double Progress { get; set; }
        }
    }
}
