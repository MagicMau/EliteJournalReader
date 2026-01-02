namespace EliteJournalReader.Events
{
    //When written: Player rewarded for taking part in a combat zone
    //Parameters: 
    //�	Reward
    //�	AwardingFaction
    //�	VictimFaction
    public class FactionKillBondEvent : JournalEvent<FactionKillBondEvent.FactionKillBondEventArgs>
    {
        public FactionKillBondEvent() : base("FactionKillBond") { }

        public class FactionKillBondEventArgs : JournalEventArgs
        {
            public string AwardingFaction { get; set; }
            public string VictimFaction { get; set; }
            public int Reward { get; set; }
        }
    }
}
