﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EliteJournalReader
{
    public class EngineeringModifiers
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ModuleAttribute Label { get; set; }

        public double Value { get; set; }
        public string ValueStr { get; set; }
        public double OriginalValue { get; set; }
        public string OriginalValueStr { get; set; }
        public bool LessIsGood { get; set; }
    }
}