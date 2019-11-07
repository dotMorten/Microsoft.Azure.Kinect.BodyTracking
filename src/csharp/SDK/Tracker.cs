using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.Native;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Kinect.BodyTracking
{

    public class Tracker : IDisposable
    {
        private NativeMethods.k4abt_tracker_t handle;

        public static readonly TrackerConfiguration DefaultTrackerConfiguration = new TrackerConfiguration { SensorOrientation = SensorOrientation.Default, CpuOnly = false };

        public Tracker(Calibration calibration, TrackerConfiguration configuration)
        {
            var result = NativeMethods.k4abt_tracker_create(ref calibration, configuration, out this.handle);
            if (result == NativeMethods.k4a_result_t.K4A_RESULT_FAILED)
                throw new AzureKinectException("Couldn't create tracker");
        }

        public void EnqueueCapture(Capture capture)
        {
            EnqueueCapture(capture, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Add a k4a sensor capture to the tracker input queue to generate its body tracking result asynchronously.
        /// </summary>
        /// <param name="capture">sensor capture returned by <see cref="Device.GetCapture"/>() from k4a SDK. It should contain the depth data for this function to work.Otherwise the function will return failure.</param>
        /// <param name="timeout">
        /// Specifies the time the function should block waiting to add the sensor capture to the tracker
        /// process queue. 0 is a check of the status without blocking. Passing a value of TimeSpan.FromMilliseconds(-1) will block
        /// indefinitely until the capture is added to the process queue.
        /// </param>
        /// <remarks>
        /// <para>
        /// Add a k4a capture to the tracker input queue so that it can be processed asynchronously to generate the body tracking
        /// result. The processed results will be added to an output queue maintained by k4abt_tracker_t instance. Call
        /// <see cref="PopResult"/> to get the result and pop it from the output queue.
        /// If the input queue or output queue is full, this function will block up until the timeout is reached.
        /// Once body_frame data is read, the user must call <see cref="Frame.Dispose"/>() to return the allocated memory to the SDK</para>
        /// <para>
        /// Upon successfully insert a sensor capture to the input queue this function will return success.
        /// </para>
        /// <para>
        /// This function returns ::K4A_WAIT_RESULT_FAILED when either the tracker is shut down by k4abt_tracker_shutdown() API,
        /// or an internal problem is encountered before adding to the input queue: such as low memory condition,
        /// sensor_capture_handle not containing the depth data, or other unexpected issues.
        /// </para>
        /// </remarks>
        /// <seealso cref="Tracker"/>
 //* \returns
 //* ::K4A_WAIT_RESULT_SUCCEEDED if a sensor capture is successfully added to the processing queue. If the queue is still
 //* full before the timeout elapses, the function will return ::K4A_WAIT_RESULT_TIMEOUT. All other failures will return
 //* ::K4A_WAIT_RESULT_FAILED.

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

        /// <summary>
        /// Gets the next available body frame.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Control the temporal smoothing across frames
        /// </summary>
        /// <param name="smoothingFactor">
        /// Set between 0 for no smoothing and 1 for full smoothing. Less smoothing will increase the responsiveness of the detected skeletons but will cause more positional and orientational jitters.
        /// </param>
        public void SetTemporalSmoothing(float smoothingFactor)
        {
            NativeMethods.k4abt_tracker_set_temporal_smoothing(this.handle, smoothingFactor);
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
