namespace EliteJournalReader.Events
{
    //When Written: when purchasing ammunition
    //Parameters:
    //�	Cost
    public class BuyAmmoEvent : JournalEvent<BuyAmmoEvent.BuyAmmoEventArgs>
    {
        public BuyAmmoEvent() : base("BuyAmmo") { }

        public class BuyAmmoEventArgs : JournalEventArgs
        {
            public int Cost { get; set; }
        }
    }
}
