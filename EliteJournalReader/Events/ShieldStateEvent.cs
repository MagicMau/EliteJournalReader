namespace EliteJournalReader.Events
{
    //When written: when shields are disabled in combat, or recharged
    //Parameters:
    //�	ShieldsUp 0 when disabled, 1 when restored
    public class ShieldStateEvent : JournalEvent<ShieldStateEvent.ShieldStateEventArgs>
    {
        public ShieldStateEvent() : base("ShieldState") { }

        public class ShieldStateEventArgs : JournalEventArgs
        {
            public bool ShieldsUp { get; set; }
        }
    }
}
