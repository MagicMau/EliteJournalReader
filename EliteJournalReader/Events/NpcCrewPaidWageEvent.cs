namespace EliteJournalReader.Events
{
    //This is written when crew receive wages
    //Parameters:
    //�	NpcCrewId
    //�	Amount
    public class NpcCrewPaidWageEvent : JournalEvent<NpcCrewPaidWageEvent.NpcCrewPaidWageEventArgs>
    {
        public NpcCrewPaidWageEvent() : base("NpcCrewPaidWage") { }

        public class NpcCrewPaidWageEventArgs : JournalEventArgs
        {
            public string NpcCrewName { get; set; }
            public long NpcCrewId { get; set; }
            public long Amount { get; set; }
        }
    }
}
