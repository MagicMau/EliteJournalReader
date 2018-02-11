using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader.Events
{
    public class StatusFileEvent
    {
        public StatusFlags Flags { get; set; }
        public int[] Pips { get; set; }
        public int Firegroup { get; set; }
        public StatusGuiFocus GuiFocus { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Heading { get; set; }
    }

    [Flags]
    public enum StatusFlags
    {
        Docked = 0x00000001,
        Landed = 0x00000002,
        LandingGearDown = 0x00000004,
        ShieldsUp = 0x00000008,
        Supercruise = 0x00000010,
        FlightAssistOff = 0x00000020,
        HardpointsDeployed = 0x00000040,
        InWing = 0x00000080,
        LightsOn = 0x00000100,
        CargoScoopDeployed = 0x00000200,
        SilentRunning = 0x00000400,
        ScoopingFuel = 0x00000800,
        SrvHandbrake = 0x00001000,
        SrvTurret = 0x00002000,
        SrvUnderShip = 0x00004000,
        SrvDriveAssist = 0x00008000,
        FsdMassLocked = 0x00010000,
        FsdCharging = 0x00020000,
        FsdCooldown = 0x00040000,
        LowFuel = 0x00080000,
        Overheating = 0x00100000,
        HasLatLong = 0x00200000,
        IsInDanger = 0x00400000,
        BeingInterdicted = 0x00800000,
        InMainShip = 0x01000000,
        InFighter = 0x02000000,
        InSRV = 0x04000000
    }

    public enum StatusGuiFocus
    {
        NoFocus = 0,
        InternalPanel = 1,
        ExternalPanel = 2,
        CommsPanel = 3,
        RolePanel = 4,
        StationServices = 5,
        GalaxyMap = 6,
        SystemMap = 7
    }
}
