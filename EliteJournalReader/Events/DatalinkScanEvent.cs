using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when scanning a data link
    //Parameters:
    //•	Message: message from data link
    public class DatalinkScanEvent : JournalEvent<DatalinkScanEvent.DatalinkScanEventArgs>
    {
        public DatalinkScanEvent() : base("DatalinkScan") { }

        public class DatalinkScanEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Message = evt.Value<string>("Message");
            }

            public string Message { get; set; }
        }
    }
}
