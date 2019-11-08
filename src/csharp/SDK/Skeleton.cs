using System.Runtime.InteropServices;
using Microsoft.Azure.Kinect.Sensor.Native;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    /// <summary>
    /// Structure to define joints for skeleton
    /// </summary>
    [NativeReference("k4abt_skeleton_t")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Skeleton
    {
        /// <summary>
        /// The joints for the body
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public Joint[] Joints;
    }
}
