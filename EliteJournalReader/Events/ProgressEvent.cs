using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup
    //Parameters:
    //•	Combat: percent progress to next rank
    //•	Trade: 		“
    //•	Explore: 	“
    //•	Empire: 	“
    //•	Federation: 	“
    //•	CQC: 		“
    public class ProgressEvent : JournalEvent<ProgressEvent.ProgressEventArgs>
    {
        public ProgressEvent() : base("Progress") { }

        public class ProgressEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Combat = evt.Value<int>("Combat");
                Trade = evt.Value<int>("Trade");
                Explore = evt.Value<int>("Explore");
                Empire = evt.Value<int>("Empire");
                Federation = evt.Value<int>("Federation");
                CQC = evt.Value<int>("CQC");
            }

            public int Combat { get; set; }
            public int Trade { get; set; }
            public int Explore { get; set; }
            public int Empire { get; set; }
            public int Federation { get; set; }
            public int CQC { get; set; }
        }
    }
}
