namespace EliteJournalReader.Events
{
    //When Written: when paying legacy fines
    //Parameters:
    //�	Amount
    public class PayLegacyFinesEvent : JournalEvent<PayLegacyFinesEvent.PayLegacyFinesEventArgs>
    {
        public PayLegacyFinesEvent() : base("PayLegacyFines") { }

        public class PayLegacyFinesEventArgs : JournalEventArgs
        {
            public int Amount { get; set; }
            public double? BrokerPercentage { get; set; }
        }
    }
}
