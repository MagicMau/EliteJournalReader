namespace EliteJournalReader.Events
{
    //When Written: when refuelling (full tank)
    //Parameters:
    //�	Cost: cost of fuel
    //�	Amount: tons of fuel purchased
    public class RefuelAllEvent : JournalEvent<RefuelAllEvent.RefuelAllEventArgs>
    {
        public RefuelAllEvent() : base("RefuelAll") { }

        public class RefuelAllEventArgs : JournalEventArgs
        {
            public int Cost { get; set; }
            public double Amount { get; set; }
        }
    }
}
