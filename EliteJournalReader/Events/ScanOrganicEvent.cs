namespace EliteJournalReader.Events
{
    public class ScanOrganicEvent : JournalEvent<ScanOrganicEvent.ScanOrganicEventArgs>
    {
        public ScanOrganicEvent() : base("ScanOrganic") { }

        public class ScanOrganicEventArgs : JournalEventArgs
        {
            public string ScanType { get; set; }
            public string Genus { get; set; }
            public string Genus_Localised { get; set; }
            public string Species { get; set; }
            public string Species_Localised { get; set; }
            public string Variant { get; set; }
            public string Variant_Localised { get; set; }
            public long SystemAddress { get; set; }
            public long Body { get; set; }
        }
    }
}