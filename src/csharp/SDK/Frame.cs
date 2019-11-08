using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    /// <summary>
    /// k4a body tracking frame
    /// </summary>
    public class Frame : IDisposable
    {
        private readonly NativeMethods.k4abt_frame_t handle;
        
        internal Frame(NativeMethods.k4abt_frame_t frame)
        {
            this.handle = frame;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                this.handle.Dispose();
            }
        }

        /// <summary>
        /// Get the number of people from the
        /// </summary>
        /// <value>Gets the number of detected bodies. 0 if the function fails.</value>
        public uint NumberOfBodies => (uint)NativeMethods.k4abt_frame_get_num_bodies(this.handle.DangerousGetHandle());

        /// <summary>
        /// Get the joint information for a particular person index from the frame
        /// </summary>
        /// <param name="index">The index of the body of which the joint information is queried.</param>
        /// <returns>If successful this contains the body skeleton information.</returns>
        public Skeleton GetBodySkeleton(uint index)
        {
            NativeMethods.k4a_result_t result = NativeMethods.k4abt_frame_get_body_skeleton(this.handle, (UIntPtr)index, out Skeleton skeleton);
            return result == NativeMethods.k4a_result_t.K4A_RESULT_SUCCEEDED ? skeleton : (default);
        }

        /// <summary>
        /// Get the body id for a particular person index from the frame
        /// </summary>
        /// <param name="index">The index of the body of which the body id information is queried.</param>
        /// <returns>Returns the body id. All failures will return uint.MaxValue.</returns>
        public uint GetBodyId(uint index) => NativeMethods.k4abt_frame_get_body_id(this.handle, (UIntPtr)index);

        /// <summary>
        /// Get the body index map from the frame
        /// </summary>
        /// <returns> Call this function to access the body index map image.</returns>
        /// <remarks>
        /// <para></para>
        /// Called when the user has received a body frame handle and wants to access the data contained in it.
        /// <para>
        /// Body Index map is the body instance segmentation map. Each pixel maps to the corresponding pixel in the
        /// depth image or the ir image.The value for each pixel represents which body the pixel belongs to.It can be either
        /// background (value K4ABT_BODY_INDEX_MAP_BACKGROUND) or the index of a detected <see cref="Body"/>.
        /// </para>
        /// </remarks>
        public Sensor.Image GetBodyIndexMap()
        {
            Type k4a_image_t = typeof(Sensor.Image).Assembly.GetType("Microsoft.Azure.Kinect.Sensor.NativeMethods+k4a_image_t");
            System.Reflection.ConstructorInfo constructor = typeof(Sensor.Image).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new Type[] { k4a_image_t }, null);
            var ptr = NativeMethods.k4abt_frame_get_body_index_map(this.handle);
            var map = Activator.CreateInstance(k4a_image_t, true) as Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid;
            var sethandlem = k4a_image_t.GetMethod("SetHandle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            sethandlem.Invoke(map, new object[] { ptr });
            //var map = System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, k4a_image_t);
            return (Sensor.Image)constructor.Invoke(new object[] { map });
        }
        /// <summary>
        /// Get the body frame's device timestamp in microseconds
        /// </summary>
        /// <remarks>
        /// Returns the timestamp of the body frame. If the frame is invalid this function will return 0. It is
        /// also possible for 0 to be a valid timestamp originating from the beginning of a recording or the start of streaming.
        /// </remarks>
        public ulong DeviceTimestamp => NativeMethods.k4abt_frame_get_device_timestamp_usec(this.handle);
    }
}
