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
                From_Localised = evt.Value<string>("From_Localised");
                Message = evt.Value<string>("Message");
                Message_Localised = evt.Value<string>("Message_Localised");
                Channel = evt.Value<string>("Channel").ToEnum(TextChannel.Unknown);
            }

            public string From { get; set; }
            public string From_Localised { get; set; }
            public string Message { get; set; }
            public string Message_Localised { get; set; }
            public TextChannel Channel { get; set; }
        }
    }

    public static class TextChannelExtensions
    {
        public static bool IsPlayerChannel(this TextChannel channel)
        {
            return channel != TextChannel.Unknown && channel != TextChannel.NPC;
        }
    }
}
