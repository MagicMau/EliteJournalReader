namespace EliteJournalReader.Events
{
    public class CancelTaxiEvent : JournalEvent<CancelTaxiEvent.CancelTaxiEventArgs>
    {
        public CancelTaxiEvent() : base("CancelTaxi") { }

        public class CancelTaxiEventArgs : JournalEventArgs
        {
            public int Refund { get; set; }
        }
    }
}