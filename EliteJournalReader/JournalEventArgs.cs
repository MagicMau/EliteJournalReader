using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace EliteJournalReader
{
    public class JournalEventArgs : EventArgs
    {
        public JObject OriginalEvent { get; private set; }

        public DateTime Timestamp { get; set; }

        public JournalEventArgs()
        {
        }

        public virtual void Initialize(JObject evt)
        {
            OriginalEvent = evt;
            Timestamp = DateTime.Parse(evt.Value<string>("timestamp"),
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
    }
}