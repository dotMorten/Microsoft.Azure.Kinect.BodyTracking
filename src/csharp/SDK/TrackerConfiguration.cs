using System.Runtime.InteropServices;
using Microsoft.Azure.Kinect.Sensor.Native;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    /// <summary>
    /// This enumeration specifies the sensor mounting orientation. Passing the correct orientation in k4abt_tracker_create()
    /// can help the body tracker to achieve more accurate body tracking.
    /// </summary>
    /// <remarks>
    /// The sensor orientation is defined while facing the camera.
    /// </remarks>
    public enum SensorOrientation
    {
        /// <summary>
        /// Mount the sensor at its default orientation
        /// </summary>
        Default = 0,
        /// <summary>
        /// Clockwisely rotate the sensor 90 degree
        /// </summary>
        Clockwise90,
        /// <summary>
        /// Counter-clockwisely rotate the sensor 90 degrees
        /// </summary>
        CounterClockwise90,
        /// <summary>
        /// Mount the sensor upside-down
        /// </summary>
        Flip180,
    }

    /// <summary>
    ///  Configuration parameters for a k4abt body tracker
    /// </summary>
    [NativeReference("k4abt_tracker_configuration_t")]
    [StructLayout(LayoutKind.Sequential)]
    public struct TrackerConfiguration
    {
        /// <summary>
        /// The sensor mounting orientation type.
        /// </summary>
        /// <remarks>Setting the correct orientation can help the body tracker to achieve more accurate body tracking results</remarks>
        public SensorOrientation SensorOrientation;
        /// <summary>
        /// Specify whether to use CPU only mode or GPU mode to run the tracker.
        /// </summary>
        /// <remarks>
        /// The CPU only mode doesn't require the machine to have a GPU to run this SDK. But it will be much slower than the GPU mode.
        /// </remarks>
        [MarshalAs(UnmanagedType.I1)]
        public bool CpuOnly;
    }
}
