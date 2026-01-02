using System;

namespace EliteJournalReader
{
    public class Faction
    {
        public string Name { get; set; }
        public string FactionState { get; set; }
        public string Government { get; set; }
        public double Influence { get; set; }
        public string Allegiance { get; set; }
        public string Happiness { get; set; }
        public string Happiness_Localised { get; set; }
        public double MyReputation { get; set; }
        public bool SquadronFaction { get; set; } = false;
        public bool HappiestSystem { get; set; } = false;
        public bool HomeSystem { get; set; } = false;

        public FactionStateChange[] PendingStates { get; set; }
        public FactionStateChange[] RecoveringStates { get; set; }
        public FactionStateChange[] ActiveStates { get; set; }

        public override bool Equals(object obj) => Equals(obj as Faction);

        public bool Equals(Faction that) => that != null
            && that.Name?.Equals(Name) == true
            && that.FactionState?.Equals(FactionState) == true
            && that.Government?.Equals(Government) == true
            && that.Influence == Influence
            && that.Allegiance?.Equals(Allegiance) == true
            && that.MyReputation == MyReputation
            && that.SquadronFaction == SquadronFaction
            && that.HappiestSystem == HappiestSystem
            && that.HomeSystem == HomeSystem
            && that.PendingStates?.Equals(PendingStates) == true
            && that.RecoveringStates?.Equals(RecoveringStates) == true
            && that.ActiveStates?.Equals(ActiveStates) == true;

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Name);
            hash.Add(FactionState);
            hash.Add(Government);
            hash.Add(Influence);
            hash.Add(Allegiance);
            hash.Add(MyReputation);
            hash.Add(SquadronFaction);
            hash.Add(HappiestSystem);
            hash.Add(HomeSystem);
            hash.Add(PendingStates);
            hash.Add(RecoveringStates);
            hash.Add(ActiveStates);
            return hash.ToHashCode();
        }

        public Faction Clone() => (Faction)MemberwiseClone();
    }

    public class FactionStateChange
    {
        public string State { get; set; }
        public int Trend { get; set; }
    }
}
