namespace NBROS.Builds
{
    public enum VersionIncrementType
    {
        None,

        /// <summary>
        /// The build number.
        /// </summary>
        BuildNumber,
        /// <summary>
        /// The third number.
        /// </summary>
        Patch,
        /// <summary>
        /// The second number.
        /// </summary>
        MinorUpdate,
        /// <summary>
        /// The first number. e.g v2.0
        /// </summary>
        MajorUpdate
    }
}
