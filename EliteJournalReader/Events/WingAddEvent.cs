namespace EliteJournalReader.Events
{
    //When written: another player has joined the wing
    //Parameters:
    //�	Name
    public class WingAddEvent : JournalEvent<WingAddEvent.WingAddEventArgs>
    {
        public WingAddEvent() : base("WingAdd") { }

        public class WingAddEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
        }
    }
}
