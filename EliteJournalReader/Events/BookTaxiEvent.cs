namespace EliteJournalReader.Events
{
    public class BookTaxiEvent : JournalEvent<BookTaxiEvent.BookTaxiEventArgs>
    {
        public BookTaxiEvent() : base("BookTaxi") { }

        public class BookTaxiEventArgs : JournalEventArgs
        {
            public long Cost { get; set; }

            public string DestinationSystem { get; set; }

            public string DestinationLocation { get; set; }
        }
    }
}