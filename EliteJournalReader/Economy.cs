using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader
{
    public class Economy
    {
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public double Proportion { get; set; }

        public override bool Equals(object obj) => Equals(obj as Economy);

        public bool Equals(Economy that) => that != null
            && that.Name?.Equals(Name) == true
            && that.Proportion == Proportion;

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Name, Proportion);
        }

        public Economy Clone() => (Economy)MemberwiseClone();
    }
}
