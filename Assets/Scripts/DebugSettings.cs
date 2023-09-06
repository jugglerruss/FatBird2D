public struct DebugSettings
{
    public float Mass, MassMultiplier, GroundFriction, SkyFriction;

    public DebugSettings(float mass, float massMultiplier, float groundFriction, float skyFriction)
    {
        Mass = mass;
        MassMultiplier = massMultiplier;
        GroundFriction = groundFriction;
        SkyFriction = skyFriction;
    }
}