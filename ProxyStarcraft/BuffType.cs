namespace ProxyStarcraft
{
    public enum BuffType // TODO: Split this into multiple categories e.g. "BuildingBuff", "ChannelingAbilityDebuff", "LocationBuffType" etc?
    {
        GravitonPrison,
        Stasis,
        Revelation,
        PsionicStorm,
        Slow,
        Disabled, // Might be a single-player thing ("DisableAbils") but we'll throw Stuns from other stuff in this.
        CarryingGas,
        CarryingMinerals,

        CloakField, // Represents Mothership-style Cloaking Fields.
        SelfCloak, // Represents Ghost/Wraith/Banshee transient / DT/Stasis Ward/etc permanent cloak buffs.

        PulsarBeam,
        GravitonBeam,
        ProtectiveBarrier,
        GuardianShield,
        PowerUserWarpable, // I *think* this has something to do with Warp Prisms?
        PrismaticAlignment, // May be VoidRaySwarmDamageBoost internally?
        Charging,
        OracleWeapon,
        OracleStasisTrapTarget,
        TemporalRiftUnit,
        PsiStorm,
        PhaseShield,
        VoidSiphon,
        VortexBehavior,
        VortexBehaviorEnemy, // ??
        PurificationNova,

        Overcharge, // Buff on Pylons from the Photon Overcharge ability?
        OverchargeDamage, // ??
        TimeWarpProduction,
        ChronoBoosted, // Better name for TimeWarpProduction?

        // Zerg Unit-targetted ability effects
        FungalGrowth,
        NeuralParasite,
        Corruption,
        BlindingCloud,
        ParasiticBomb,
        LurkerHoldFire,

        // Zerg Building-targetted ability effects
        Contaminated,
        ViperConsume,
        SpawnLarva,

        //Terran
        LockOn,
        Stimpack,
        GhostSnipeDoT,
        EMPDecloak,
        MedivacSpeedBoost,
        GhostHoldFire,
        //There might (?) be other buffs that I don't have (e.g. Tactical Jump for Battlecruisers)

        SupplyDrop,
        
        //Leech, // these seem.. important, but only vaguely so.
        //Ethereal,
                
        //Hidden,
        
        
    }
}
