using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader
{
    public enum StarType
    {
        Unknown,

        // main sequence
        O,
        B,
        A,
        F,
        G,
        K,
        M,
        L,
        T,
        Y,

        // proto stars
        TTS,
        AeBe,

        // wolf-rayet
        W,
        WN,
        WNC,
        WC,
        WO,

        // carbon stars
        CS,
        C,
        CN,
        CJ,
        CH,
        CHd,

        MS,
        S,

        // white drafs
        D,
        DA,
        DAB,
        DAO,
        DAZ,
        DAV,
        DB,
        DBZ,
        DBV,
        DO,
        DOV,
        DQ,
        DC,
        DCV,
        DX,

        // neutron
        N,

        // black hole
        H,

        // exotic
        X,

        // other
        SupermassiveBlackHole,
        A_BlueWhiteSuperGiant,
        F_WhiteSuperGiant,
        M_RedSuperGiant,
        M_RedGiant,
        K_OrangeGiant,
        RoguePlanet,
        Nebula,
        StellarRemnantNebula
    }


    public enum PlanetClass
    {
        Unknown,
        MetalRichBody,
        HighMetalContentBody,
        RockyBody,
        IcyBody,
        RockyIceBody,
        EarthlikeBody,
        WaterWorld,
        AmmoniaWorld,
        WaterGiant,
        WaterGiantWithLife,
        GasGiantWithWaterBasedLife,
        GasGiantWithAmmoniaBasedLife,
        SudarskyClassIGasGiant,
        SudarskyClassIIGasGiant,
        SudarskyClassIIIGasGiant,
        SudarskyClassIVGasGiant,
        SudarskyClassVGasGiant,
        HeliumRichGasGiant,
        HeliumGasGiant
    }

    public enum AtmosphereClass
    {
        Unknown,
        NoAtmosphere,
        SuitableForWaterBasedLife,
        AmmoniaAndOxygen,
        Ammonia,
        Water,
        CarbonDioxide,
        SulphurDioxide,
        Nitrogen,
        WaterRich,
        MethaneRich,
        AmmoniaRich,
        CarbonDioxideRich,
        Methane,
        Helium,
        SilicateVapour,
        MetallicVapour,
        NeonRich,
        ArgonRich,
        Neon,
        Argon,
        Oxygen
    }

    public enum AtmosphereType
    {
        Unknown,
        None,
        EarthLike,
        AmmoniaOxygen,
        Oxygen,
        Ammonia,
        Water,
        CarbonDioxide,
        SulphurDioxide,
        Nitrogen,
        WaterRich,
        MethaneRich,
        AmmoniaRich,
        CarbonDioxideRich,
        Methane,
        Helium,
        Neon,
        Argon,
        NeonRich,
        ArgonRich,
        SilicateVapour,
        MetallicVapour
    }

    public enum VolcanismClass
    {
        Unknown,
        None,
        WaterMagma,
        SulphurDioxideMagma,
        AmmoniaMagma,
        MethaneMagma,
        NitrogenMagma,
        SilicateMagma,
        MetallicMagma,
        WaterGeysers,
        CarbonDioxideGeysers,
        AmmoniaGeysers,
        MethaneGeysers,
        NitrogenGeysers,
        HeliumGeysers,
        SilicateVapourGeysers
    }

    public enum CrimeType
    {
        Unknown,
        Assault,
        Murder,
        Piracy,
        Interdiction,
        IllegalCargo,
        DisobeyPolice,
        FireInNoFireZone,
        FireInStation,
        DumpingDangerous,
        DumpingNearStation,
        DockingMinor_BlockingAirlock,
        DockingMajor_BlockingAirlock,
        DockingMinor_BlockingLandingPad,
        DockingMajor_BlockingLandingPad,
        DockingMinor_Trespass,
        DockingMajor_Trespass,
        CollidedAtSpeedInNoFireZone,
        CollidedAtSpeedInNoFireZone_HullDamage
    }

    public enum BodyType
    {
        Unknown,
        Null,
        Star,
        Planet,
        PlanetaryRing,
        StellarRing,
        Station,
        AsteroidCluster
    }

    public enum GameMode
    {
        Unknown,
        Open,
        Solo,
        Group
    }

    public enum PowerplayState
    {
        Unknown,
        InPrepareRadius,
        Prepared,
        Exploited,
        Contested,
        Controlled,
        Turmoil,
        HomeSystem
    }

    public enum TerraformState
    {
        None,
        Terraformable,
        Terraforming,
        Terraformed
    }
}
