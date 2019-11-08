using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.Native;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    internal class NativeMethods
    {
        private const CallingConvention k4aCallingConvention = CallingConvention.Cdecl;

        // These types are used internally by the interop dll for marshaling purposes and are not exposed
        // over the public surface of the managed dll.

        public class k4abt_tracker_t : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
        {
            private k4abt_tracker_t() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                NativeMethods.k4abt_tracker_destroy(handle);
                return true;
            }
        }

        public class k4abt_frame_t : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
        {
            private k4abt_frame_t() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                NativeMethods.k4abt_frame_release(handle);
                return true;
            }
        }
    

        [NativeReference]
        public enum k4a_result_t
        {
            K4A_RESULT_SUCCEEDED = 0,
            K4A_RESULT_FAILED,
        }
        [NativeReference]
        public enum k4a_wait_result_t
        {
            K4A_WAIT_RESULT_SUCCEEDED = 0,
            K4A_WAIT_RESULT_FAILED,
            K4A_WAIT_RESULT_TIMEOUT
        }

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern k4a_result_t k4abt_tracker_create([In] ref Calibration calibration, TrackerConfiguration config, out k4abt_tracker_t tracker_handle);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern void k4abt_tracker_destroy(IntPtr tracker_handle);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern k4a_wait_result_t k4abt_tracker_enqueue_capture(IntPtr calibration_handle, IntPtr capture_handle, int timeout_in_ms);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern k4a_wait_result_t k4abt_tracker_pop_result(IntPtr tracker_handle, out k4abt_frame_t body_frame_handle, int timeout_in_ms);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern void k4abt_tracker_set_temporal_smoothing(k4abt_tracker_t tracker_handle, float smoothing_factor);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern void k4abt_tracker_shutdown(k4abt_tracker_t tracker_handle);
        
        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern void k4abt_frame_release(IntPtr frame_handle);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern UIntPtr k4abt_frame_get_num_bodies(IntPtr frame_handle);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern k4a_result_t k4abt_frame_get_body_skeleton(k4abt_frame_t body_frame_handle, UIntPtr index, out Skeleton skeleton);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern uint k4abt_frame_get_body_id(k4abt_frame_t body_frame_handle, UIntPtr index);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern ulong k4abt_frame_get_device_timestamp_usec(k4abt_frame_t body_frame_handle);

        [DllImport("k4abt", CallingConvention = k4aCallingConvention)]
        [NativeReference]
        public static extern IntPtr k4abt_frame_get_body_index_map(k4abt_frame_t body_frame_handle);
    }
}
