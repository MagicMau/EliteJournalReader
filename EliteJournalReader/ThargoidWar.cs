namespace EliteJournalReader
{
    //"ThargoidWar":{ "CurrentState":"", "NextStateSuccess":"Unknown", "NextStateFailure":"Unknown",
    //"SuccessStateReached":false, "WarProgress":0.000000, "RemainingPorts":0 }

    public class ThargoidWar
    {
        public string CurrentState { get; set; }
        public string NextStateSuccess { get; set; }
        public string NextStateFailure { get; set; }
        public bool SuccessStateReached { get; set; }
        public double WarProgress { get; set; }
        public long RemainingPorts { get; set; }
    }
}
