namespace EliteJournalReader.Events
{
    //When written: when voting for a system expansion
    //Parameters:
    //�	Power
    //�	Votes
    //�	System
    public class PowerplayVoteEvent : JournalEvent<PowerplayVoteEvent.PowerplayVoteEventArgs>
    {
        public PowerplayVoteEvent() : base("PowerplayVote") { }

        public class PowerplayVoteEventArgs : JournalEventArgs
        {
            public string Power { get; set; }
            public string System { get; set; }
            public int Votes { get; set; }
        }
    }
}
