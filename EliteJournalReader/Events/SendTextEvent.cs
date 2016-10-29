using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when a text message is sent to another player
    //Parameters:
    //•	To
    //•	Message
    public class SendTextEvent : JournalEvent<SendTextEvent.SendTextEventArgs>
    {
        public SendTextEvent() : base("SendText") { }

        public class SendTextEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                To = evt.Value<string>("To");
                Message = evt.Value<string>("Message");
                Channel = evt.Value<string>("Channel");
            }

            public string To { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
        }
    }
}
