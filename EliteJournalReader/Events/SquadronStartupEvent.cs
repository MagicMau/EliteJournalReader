namespace EliteJournalReader.Events
{
    public class SquadronStartupEvent : JournalEvent<SquadronStartupEvent.SquadronStartupEventArgs>
    {
        public SquadronStartupEvent() : base("SquadronStartup") { }

        public class SquadronStartupEventArgs : JournalEventArgs
        {
            public long SquadronID { get; set; }
            public string SquadronName { get; set; }
            public long CurrentRank { get; set; }
            public string CurrentRankName { get; set; }
            public string CurrentRankName_Localised { get; set; }
        }
    }
}
