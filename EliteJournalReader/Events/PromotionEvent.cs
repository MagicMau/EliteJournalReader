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
                Combat = (CombatRank?)evt.Value<int?>("Combat");
                Trade = (TradeRank?)evt.Value<int?>("Trade");
                Explore = (ExplorationRank?)evt.Value<int?>("Explore");
                CQC = (CQCRank?)evt.Value<int?>("CQC");
            }

            public CombatRank? Combat { get; set; }
            public TradeRank? Trade { get; set; }
            public ExplorationRank? Explore { get; set; }
            public CQCRank? CQC { get; set; }
        }
    }
}
