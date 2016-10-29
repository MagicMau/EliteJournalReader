using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when switching control between the main ship and a fighter
    //Parameters:
    //•	To: ( Mothership/Fighter)
    public class VehicleSwitchEvent : JournalEvent<VehicleSwitchEvent.VehicleSwitchEventArgs>
    {
        public VehicleSwitchEvent() : base("VehicleSwitch") { }

        public class VehicleSwitchEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                To = evt.Value<string>("To").ToEnum(Vehicle.Unknown);
            }

            public Vehicle To { get; set; }
        }
    }

    public enum Vehicle
    {
        Unknown,
        Mothership,
        Fighter
    }
}
