namespace EliteJournalReader.Events
{
    //When written: another player has joined the wing
    //Parameters:
    //�	Name
    public class WingInviteEvent : JournalEvent<WingInviteEvent.WingInviteEventArgs>
    {
        public WingInviteEvent() : base("WingInvite") { }

        public class WingInviteEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
        }
    }
}
