namespace EliteJournalReader.Events
{
    //When written: when leaving a power
    //Parameters:
    //�	Power
    public class PowerplayLeaveEvent : JournalEvent<PowerplayLeaveEvent.PowerplayLeaveEventArgs>
    {
        public PowerplayLeaveEvent() : base("PowerplayLeave") { }

        public class PowerplayLeaveEventArgs : JournalEventArgs
        {
            public string Power { get; set; }
        }
    }
}
