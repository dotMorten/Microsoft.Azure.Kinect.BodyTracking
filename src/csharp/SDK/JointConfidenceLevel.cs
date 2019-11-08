namespace Microsoft.Azure.Kinect.BodyTracking
{
    /// <summary>
    /// This enumeration specifies the joint confidence level.
    /// </summary>
    public enum JointConfidenceLevel
    {
        /// <summary>
        /// The joint is out of range (too far from depth camera)
        /// </summary>
        None = 0,
        /// <summary>
        /// The joint is not observed (likely due to occlusion), predicted joint pose
        /// </summary>
        Low = 1,
        /// <summary>
        /// Medium confidence in joint pose. Current SDK will only provide joints up to this confidence level
        /// </summary>
        Medium = 2,
        /// <summary>
        /// High confidence in joint pose. Placeholder for future SDK
        /// </summary>
        High = 3,
    }
}
