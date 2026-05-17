namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Color attribute carried by a <see cref="Shape"/>. <see cref="None"/> means
    /// "no color set" and acts as a wildcard during target matching — see
    /// <see cref="TargetComparator"/>.
    /// </summary>
    public enum ShapeColor
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Yellow = 3
    }
}
