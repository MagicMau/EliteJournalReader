namespace EliteJournalReader.Events
{
    //    When written: when the player has broken up a �Motherlode� asteroid for mining
    //    Parameters:
    //�	Body: name of nearest body
    public class AsteroidCrackedEvent : JournalEvent<AsteroidCrackedEvent.AsteroidCrackedEventArgs>
    {
        public AsteroidCrackedEvent() : base("AsteroidCracked") { }

        public class AsteroidCrackedEventArgs : JournalEventArgs
        {
            public string Body { get; set; }
        }
    }
}
