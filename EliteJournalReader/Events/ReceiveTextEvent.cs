using Newtonsoft.Json;

namespace EliteJournalReader.Events
{
    //When written: when a text message is received from another player
    //Parameters:
    //�	From
    //�	Message
    public class ReceiveTextEvent : JournalEvent<ReceiveTextEvent.ReceiveTextEventArgs>
    {
        public ReceiveTextEvent() : base("ReceiveText") { }

        public class ReceiveTextEventArgs : JournalEventArgs
        {
            public string From { get; set; }
            public string From_Localised { get; set; }
            public string Message { get; set; }
            public string Message_Localised { get; set; }

            [JsonConverter(typeof(ExtendedStringEnumConverter<TextChannel>))]
            public TextChannel Channel { get; set; }
        }
    }

    public static class TextChannelExtensions
    {
        public static bool IsPlayerChannel(this TextChannel channel) => channel != TextChannel.Unknown && channel != TextChannel.NPC;
    }
}
