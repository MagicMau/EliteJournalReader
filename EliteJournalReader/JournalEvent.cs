using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace EliteJournalReader
{
    public abstract class JournalEvent
    {
        public string[] EventNames { get; }

        public string OriginalEvent { get; protected set; }

        protected JournalEvent(params string[] eventNames)
        {
            EventNames = eventNames;
        }

        internal abstract JournalEventArgs FireEvent(object sender, JObject evt);
    }

    public abstract class JournalEvent<TJournalEventArgs> : JournalEvent
        where TJournalEventArgs : JournalEventArgs, new()
    {
        public event EventHandler<TJournalEventArgs> Fired;

        protected JournalEvent(params string[] eventNames) : base(eventNames)
        {
        }

        public void AddHandler(EventHandler<TJournalEventArgs> eventHandler) => Fired += eventHandler;

        public void RemoveHandler(EventHandler<TJournalEventArgs> eventHandler) => Fired -= eventHandler;

        internal override JournalEventArgs FireEvent(object sender, JObject evt)
        {
            var eventArgs = evt.ToObject<TJournalEventArgs>();

            eventArgs.OriginalEvent = evt;
            eventArgs.Timestamp = DateTime.Parse(evt.Value<string>("timestamp"),
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

#if DEBUG
            Type argsType = typeof(TJournalEventArgs);
            var eventName = evt["event"];

            var argsPropertyNames = argsType.GetProperties().Select(p => p.Name).ToList();
            string[] ignoreProperties = new string[] { "event" };
            foreach (var jProperty in evt.Properties())
            {
                string jsonPropertyName = jProperty.Name;
                if (ignoreProperties.Contains(jsonPropertyName))
                {
                    // ignore anything in the ignore list
                }
                else if (jsonPropertyName.EndsWith("_Localised", StringComparison.CurrentCultureIgnoreCase))
                {
                    // ignore localised
                }
                else if (!argsPropertyNames.Any(x => string.Compare(jsonPropertyName, x, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    // found something missing
                    Trace.TraceInformation($"EventArgs for {eventName} does not contain property {jsonPropertyName}");
                    //Debugger.Break();
                }

            }
#endif
            eventArgs.PostProcess(evt);

            Fired?.Invoke(sender, eventArgs);

            return eventArgs;
        }
    }
}
