using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Win32.SafeHandles;
using System;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    public class Tracker : IDisposable
    {
        private NativeMethods.k4abt_tracker_t handle;

        public Tracker(Calibration calibration, bool cpuOnlyMode)
        {
            var result = NativeMethods.k4abt_tracker_create(ref calibration, new NativeMethods.k4abt_tracker_configuration_t() { cpu_only_mode = cpuOnlyMode }, out this.handle);
            if (result == NativeMethods.k4a_result_t.K4A_RESULT_FAILED)
                throw new AzureKinectException("Couldn't create tracker");
        }

        public void EnqueueCapture(Capture capture)
        {
            EnqueueCapture(capture, TimeSpan.FromMilliseconds(-1));
        }

        public void EnqueueCapture(Capture capture, TimeSpan timeout)
        {
            var captureHandle = (SafeHandleZeroOrMinusOneIsInvalid)typeof(Capture).GetField("handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(capture);
            var result = NativeMethods.k4abt_tracker_enqueue_capture(handle.DangerousGetHandle(), captureHandle.DangerousGetHandle(), (int)timeout.TotalMilliseconds);
            if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_TIMEOUT)
                throw new TimeoutException();
            else if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_FAILED)
                throw new AzureKinectException();
        }

        public Frame PopResult() => PopResult(TimeSpan.FromMilliseconds(-1));

        public Frame PopResult(TimeSpan timeout)
        {
            NativeMethods.k4abt_frame_t frame = null;
            var result = NativeMethods.k4abt_tracker_pop_result(handle.DangerousGetHandle(), out frame, (int)timeout.TotalMilliseconds);
            if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_TIMEOUT)
                throw new TimeoutException();
            else if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_FAILED)
                throw new AzureKinectException();
            return new Frame(frame);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                handle.Dispose();
            }
        }
    }
}
