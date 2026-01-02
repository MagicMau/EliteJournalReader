using System.Collections.Generic;

namespace EliteJournalReader.Events
{
    public class SellOrganicDataEvent : JournalEvent<SellOrganicDataEvent.SellOrganicDataEventArgs>
    {
        public SellOrganicDataEvent() : base("SellOrganicData") { }

        public class SellOrganicDataEventArgs : JournalEventArgs
        {
            public long MarketID { get; set; }
            public List<BioData> BioData { get; set; }
        }
    }
}