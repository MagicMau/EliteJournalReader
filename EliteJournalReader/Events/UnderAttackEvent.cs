namespace EliteJournalReader.Events
{
    //    When written: when under fire(same time as the Under Attack voice message)
    //Parameters:
    //�	Target: (Fighter/Mothership/You)
    public class UnderAttackEvent : JournalEvent<UnderAttackEvent.UnderAttackEventArgs>
    {
        public UnderAttackEvent() : base("UnderAttack") { }

        public class UnderAttackEventArgs : JournalEventArgs
        {
            public string Target { get; set; }
        }
    }
}
