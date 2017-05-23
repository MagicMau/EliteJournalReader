using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //The "Passengers" event contains:

    //"Manifest": array of passenger records, each containing:
    //o MissionID (int)
    //o Type (string)
    //o VIP (bool)
    //o Wanted (bool)
    //o Count (int) 
    public class PassengersEvent : JournalEvent<PassengersEvent.PassengersEventArgs>
    {
        public PassengersEvent() : base("Passengers") { }

        public class PassengersEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);

                Manifest = evt["Manifest"]?.ToObject<Passenger[]>();
            }

            public Passenger[] Manifest { get; set; }
        }

        public struct Passenger
        {
            public int MissionID;
            public string Type;
            public string VIP;
            public bool Wanted;
            public int Count;
        }
    }
}
