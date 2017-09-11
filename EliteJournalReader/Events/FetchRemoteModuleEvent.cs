using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when requesting a module is transferred from storage at another station
    //Parameters:
    //•	StorageSlot
    //•	StoredItem
    //•	ServerId
    //•	TransferCost
    //•	Ship
    //•	ShipId
    //•	TransferTime: (in seconds)
    public class FetchRemoteModuleEvent : JournalEvent<FetchRemoteModuleEvent.FetchRemoteModuleEventArgs>
    {
        public FetchRemoteModuleEvent() : base("FetchRemoteModule") { }

        public class FetchRemoteModuleEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StorageSlot = evt.Value<string>("StorageSlot");
                StoredItem = evt.Value<string>("StoredItem_Localised") ?? evt.Value<string>("StoredItem");
                ServerId = evt.Value<string>("ServerId");
                TransferCost = evt.Value<int>("TransferCost");
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipId");
                TransferTime = evt.Value<int>("TransferTime");
            }

            public string StorageSlot { get; set; }
            public string Ship { get; set; }
            public int ShipId { get; set; }
            public string StoredItem { get; set; }
            public string ServerId { get; set; }
            public int TransferCost { get; set; }
            public int TransferTime { get; set; }
        }
    }
}
