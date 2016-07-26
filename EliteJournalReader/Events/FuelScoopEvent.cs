using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when scooping fuel from a star
    //Parameters:
    //•	Scooped: tons fuel scooped
    //•	Total: total fuel level after scooping
    public class FuelScoopEvent : JournalEvent<FuelScoopEvent.FuelScoopEventArgs>
    {
        public FuelScoopEvent() : base("FuelScoop") { }

        public class FuelScoopEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Scooped = evt.Value<decimal>("Scooped");
                Total = evt.Value<decimal>("Total");
            }

            public decimal Scooped { get; set; }
            public decimal Total { get; set; }
        }
    }
}
