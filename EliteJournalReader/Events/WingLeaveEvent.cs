namespace EliteJournalReader.Events
{
    //When written: this player has left a wing
    //Parameters: none
    public class WingLeaveEvent : JournalEvent<WingLeaveEvent.WingLeaveEventArgs>
    {
        public WingLeaveEvent() : base("WingLeave") { }

        public class WingLeaveEventArgs : JournalEventArgs
        {
        }
    }
}
