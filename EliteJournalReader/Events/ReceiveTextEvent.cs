using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when a text message is received from another player
    //Parameters:
    //•	From
    //•	Message
    public class ReceiveTextEvent : JournalEvent<ReceiveTextEvent.ReceiveTextEventArgs>
    {
        public ReceiveTextEvent() : base("ReceiveText") { }

        public class ReceiveTextEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                From = evt.Value<string>("From");
                Message = evt.Value<string>("Message");
                Channel = evt.Value<string>("Channel");
            }

            public string From { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
        }
    }
}
