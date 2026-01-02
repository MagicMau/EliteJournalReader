using System.Collections.Generic;

namespace EliteJournalReader.Events
{
    public class SellMicroResourcesEvent : JournalEvent<SellMicroResourcesEvent.SellMicroResourcesEventArgs>
    {
        public SellMicroResourcesEvent() : base("SellMicroResources") { }

        public class SellMicroResourcesEventArgs : JournalEventArgs
        {
            public List<MicroResource> MicroResources { get; set; }
            public int Price { get; set; }
            public long MarketID { get; set; }
        }
    }
}