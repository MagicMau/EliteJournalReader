using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu
    //Parameters:
    //•	Inventory: array of cargo, with Name and Count for each
    public class FSSBodySignalsEvent : JournalEvent<FSSBodySignalsEvent.FSSBodySignalsEventArgs>
    {
        public FSSBodySignalsEvent() : base("FSSBodySignals") { }

        public class FSSBodySignalsEventArgs : JournalEventArgs
        {
            public string BodyName { get; set; }
            public int BodyID { get; set; }
            public long SystemAddress { get; set; }
            public Signal[] Signals { get; set; }
        }
    }
}
