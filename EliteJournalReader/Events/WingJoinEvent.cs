namespace EliteJournalReader.Events
{
    //When written: this player has joined a wing
    //Parameters:
    //�	Others: JSON array of other player names already in wing
    public class WingJoinEvent : JournalEvent<WingJoinEvent.WingJoinEventArgs>
    {
        public WingJoinEvent() : base("WingJoin") { }

        public class WingJoinEventArgs : JournalEventArgs
        {
            public string[] Others { get; set; }
        }
    }
}
