namespace EliteJournalReader.Events
{
    public class ColonisationConstructionDepotEvent : JournalEvent<ColonisationConstructionDepotEvent.ColonisationConstructionDepotEventArgs>
    {
        public ColonisationConstructionDepotEvent() : base("ColonisationConstructionDepot") { }

        public class ColonisationConstructionDepotEventArgs : JournalEventArgs
        {
            public long MarketID { get; set; }
            public double ConstructionProgress { get; set; }
            public bool ConstructionComplete { get; set; }
            public bool ConstructionFailed { get; set; }
            public ColonizationResource[] ResourcesRequired { get; set; }

        }
    }

    public class ColonizationResource
    {
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public int RequiredAmount { get; set; }
        public int ProvidedAmount { get; set; }
        public int Payment { get; set; }
    }
}