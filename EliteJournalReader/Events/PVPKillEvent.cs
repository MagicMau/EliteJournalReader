namespace EliteJournalReader.Events
{
    //When written: when this player has killed another player
    //Parameters: 
    //�	Victim: name of victim
    //�	CombatRank: victim�s rank in range 0..8
    public class PVPKillEvent : JournalEvent<PVPKillEvent.PVPKillEventArgs>
    {
        public PVPKillEvent() : base("PVPKill") { }

        public class PVPKillEventArgs : JournalEventArgs
        {
            public string Victim { get; set; }
            public CombatRank CombatRank { get; set; }
        }
    }
}
