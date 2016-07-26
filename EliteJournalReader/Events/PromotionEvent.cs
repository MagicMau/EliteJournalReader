using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when the player’s rank increases
    //Parameters: one of the following
    //•	Combat: new rank
    //•	Trade: new rank
    //•	Explore: new rank
    //•	CQC: new rank
    public class PromotionEvent : JournalEvent<PromotionEvent.PromotionEventArgs>
    {
        public PromotionEvent() : base("Promotion") { }

        public class PromotionEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Combat = evt.Value<int?>("Combat");
                Trade = evt.Value<int?>("Trade");
                Explore = evt.Value<int?>("Explore");
                CQC = evt.Value<int?>("CQC");
            }

            public int? Combat { get; set; }
            public int? Trade { get; set; }
            public int? Explore { get; set; }
            public int? CQC { get; set; }
        }
    }
}
