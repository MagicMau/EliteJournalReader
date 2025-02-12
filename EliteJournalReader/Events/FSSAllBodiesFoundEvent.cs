using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class FSSAllBodiesFoundEvent : JournalEvent<FSSAllBodiesFoundEvent.FSSAllBodiesFoundEventArgs>
    {
        public FSSAllBodiesFoundEvent() : base("FSSAllBodiesFound") { }

        public class FSSAllBodiesFoundEventArgs : JournalEventArgs
        {
            public string SystemName { get; set; }

            public long SystemAddress { get; set; }

            public int Count { get;set; }
            public int BodyCount { get; set; }
            public int NonBodyCount { get; set; }
        }
    }
}
