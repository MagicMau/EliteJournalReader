namespace EliteJournalReader.Events
{
    //When Written: when refuelling (full tank)
    //Parameters:
    //�	Cost: cost of fuel
    //�	Amount: tons of fuel purchased
    public class RepairAllEvent : JournalEvent<RepairAllEvent.RepairAllEventArgs>
    {
        public RepairAllEvent() : base("RepairAll") { }

        public class RepairAllEventArgs : JournalEventArgs
        {
            public int Cost { get; set; }
        }
    }
}
