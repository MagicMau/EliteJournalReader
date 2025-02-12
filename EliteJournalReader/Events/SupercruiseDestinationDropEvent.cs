using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: leaving supercruise for normal space
    //Parameters:
    //•	Starsystem
    //•	Body
    public class SupercruiseDestinationDropEvent : JournalEvent<SupercruiseDestinationDropEvent.SupercruiseDestinationDropEventArgs>
    {
        public SupercruiseDestinationDropEvent() : base("SupercruiseDestinationDrop") { }

        public class SupercruiseDestinationDropEventArgs : JournalEventArgs
        {
            public string Type { get; set; }

            public int Threat { get; set; }

            public long MarketID { get; set; }
        }
    }
}
