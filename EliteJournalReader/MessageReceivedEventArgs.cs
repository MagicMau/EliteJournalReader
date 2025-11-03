using Newtonsoft.Json.Linq;
using System;

namespace EliteJournalReader
{
    public class MessageReceivedEventArgs(JournalEventArgs args, string eventType, string json) : EventArgs
    {
        public JToken JToken { get; private set; } = args.OriginalEvent?.DeepClone();
        public string EventType { get; private set; } = eventType;
        public DateTime Timestamp { get; private set; } = args.Timestamp;
        public JournalEventArgs EventArgs { get; private set; } = args;
        public string Json { get; private set; } = json;
    }
}