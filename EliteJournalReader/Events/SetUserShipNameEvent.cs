using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when assigning a name to the ship in Starport Services
    //Parameters:
    //•	Ship: Ship model(eg CobraMkIII)
    //•	ShipID: player's ship ID number
    //•	UserShipName: selected name
    //•	UserShipId: selected ship id
    public class SetUserShipNameEvent : JournalEvent<SetUserShipNameEvent.SetUserShipNameEventArgs>
    {
        public SetUserShipNameEvent() : base("SetUserShipName") { }

        public class SetUserShipNameEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipID");
                UserShipName = evt.Value<string>("UserShipName");
                UserShipId = evt.Value<string>("UserShipId");
            }

            public string Ship { get; set; }
            public int ShipId { get; set; }
            public string UserShipName { get; set; }
            public string UserShipId { get; set; }
        }
    }
}
