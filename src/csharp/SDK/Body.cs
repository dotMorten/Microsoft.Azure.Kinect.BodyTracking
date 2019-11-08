using System.Runtime.InteropServices;
using Microsoft.Azure.Kinect.Sensor.Native;

namespace Microsoft.Azure.Kinect.BodyTracking
{

    /// <summary>
    /// Structure to define joints for skeleton
    /// </summary>
    [NativeReference("k4abt_body_t")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Body
    {
        /// <summary>
        /// An id for the body that can be used for frame-to-frame correlation
        /// </summary>
        public uint Id;

        /// <summary>
        /// The skeleton information for the body 
        /// </summary>
        public Skeleton Skeleton;
    }
}
