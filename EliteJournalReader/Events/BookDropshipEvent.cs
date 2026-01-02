namespace EliteJournalReader.Events
{
    public class BookDropshipEvent : JournalEvent<BookDropshipEvent.BookDropshipEventArgs>
    {
        public BookDropshipEvent() : base("BookDropship") { }

        public class BookDropshipEventArgs : JournalEventArgs
        {
            public long Cost { get; set; }

            public string DestinationSystem { get; set; }

            public string DestinationLocation { get; set; }
        }
    }
}