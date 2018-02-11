namespace EliteJournalReader
{
    public class Module
    {
        public string Slot { get; set; }
        public string Item { get; set; }
        public bool On { get; set; }
        public int Priority { get; set; }
        public double Health { get; set; }
        public long Value { get; set; }
        public int? AmmoInClip { get; set; }
        public int? AmmoInHopper { get; set; }
        public EngineeredModule Engineering { get; set; }
    }
}
