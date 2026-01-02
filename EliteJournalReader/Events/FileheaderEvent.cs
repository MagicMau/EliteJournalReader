namespace EliteJournalReader.Events
{
    public class FileheaderEvent : JournalEvent<FileheaderEvent.FileheaderEventArgs>
    {
        public FileheaderEvent() : base("Fileheader") { }

        public class FileheaderEventArgs : JournalEventArgs
        {
            public string GameVersion { get; set; }
            public string Build { get; set; }
            public string Language { get; set; }
            public int Part { get; set; }
            public bool Odyssey { get; set; }
        }
    }
}
