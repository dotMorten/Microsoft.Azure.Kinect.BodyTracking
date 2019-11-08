using System.Runtime.InteropServices;
using Microsoft.Azure.Kinect.Sensor.Native;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    /// <summary>
    /// The position and orientation together defines the coordinate system for the given joint. They are defined relative
    /// to the sensor global coordinate system.
    /// </summary>
    [NativeReference("k4abt_joint_t")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Joint
    {
        /// <summary>
        /// The position of the joint specified in millimeters
        /// </summary>
        public System.Numerics.Vector3 Position;
        /// The orientation of the joint specified in normalized quaternion
        public System.Numerics.Quaternion Orientation;
        /// <summary>
        /// The confidence level of the joint
        /// </summary>
        public JointConfidenceLevel ConfidenceLevel;
    }
}
