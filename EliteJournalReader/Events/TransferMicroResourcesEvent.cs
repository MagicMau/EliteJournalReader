using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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