using System.Collections.Generic;

namespace EliteJournalReader.Events
{
    public class TransferMicroResourcesEvent : JournalEvent<TransferMicroResourcesEvent.TransferMicroResourcesEventArgs>
    {
        public TransferMicroResourcesEvent() : base("TransferMicroResources") { }

        public class TransferMicroResourcesEventArgs : JournalEventArgs
        {
            public List<MicroResource> Transfers { get; set; }
        }
    }
}