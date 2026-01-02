namespace EliteJournalReader.Events
{
    //When written: when approaching a planetary settlement
    //Parameters:
    //�	Name
    public class ApproachSettlementEvent : JournalEvent<ApproachSettlementEvent.ApproachSettlementEventArgs>
    {
        public ApproachSettlementEvent() : base("ApproachSettlement") { }

        public class ApproachSettlementEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public long MarketID { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public long SystemAddress { get; set; }
            public string BodyName { get; set; }
            public int BodyID { get; set; }
            public Faction StationFaction { get; set; }
            public string StationGovernment { get; set; }
            public string StationGovernment_Localised { get; set; }
            public string StationAllegiance { get; set; }
            public string[] StationServices { get; set; }
            public string StationEconomy { get; set; }
            public string StationEconomy_Localised { get; set; }
            public Economy[] StationEconomies { get; set; }
        }
    }
}
