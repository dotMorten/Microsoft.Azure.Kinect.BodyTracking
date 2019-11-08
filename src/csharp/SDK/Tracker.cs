using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.Native;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Kinect.BodyTracking
{

    /// <summary>
    /// k4a body tracking component.
    /// </summary>
    public class Tracker : IDisposable
    {
        private readonly NativeMethods.k4abt_tracker_t handle;

        /// <summary>
        /// Default configuraton
        /// </summary>
        public static readonly TrackerConfiguration DefaultTrackerConfiguration = new TrackerConfiguration { SensorOrientation = SensorOrientation.Default, CpuOnly = false };

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="calibration"></param>
        /// <param name="configuration"></param>
        public Tracker(Calibration calibration, TrackerConfiguration configuration)
        {
            NativeMethods.k4a_result_t result = NativeMethods.k4abt_tracker_create(ref calibration, configuration, out this.handle);
            if (result == NativeMethods.k4a_result_t.K4A_RESULT_FAILED)
            {
                throw new AzureKinectException("Couldn't create tracker");
            }
        }

        /// <summary>
        /// Add a k4a sensor capture to the tracker input queue to generate its body tracking result asynchronously.
        /// </summary>
        /// <param name="capture">sensor capture returned by <see cref="Device.GetCapture(TimeSpan)"/>() from k4a SDK. It should contain the depth data for this function to work.Otherwise the function will return failure.</param>
        public void EnqueueCapture(Capture capture)
        {
            this.EnqueueCapture(capture, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Add a k4a sensor capture to the tracker input queue to generate its body tracking result asynchronously.
        /// </summary>
        /// <param name="capture">sensor capture returned by <see cref="Device.GetCapture(TimeSpan)"/>() from k4a SDK. It should contain the depth data for this function to work.Otherwise the function will return failure.</param>
        /// <param name="timeout">
        /// Specifies the time the function should block waiting to add the sensor capture to the tracker
        /// process queue. 0 is a check of the status without blocking. Passing a value of TimeSpan.FromMilliseconds(-1) will block
        /// indefinitely until the capture is added to the process queue.
        /// </param>
        /// <remarks>
        /// <para>
        /// Add a k4a capture to the tracker input queue so that it can be processed asynchronously to generate the body tracking
        /// result. The processed results will be added to an output queue maintained by k4abt_tracker_t instance. Call
        /// <see cref="PopResult(TimeSpan)"/> to get the result and pop it from the output queue.
        /// If the input queue or output queue is full, this function will block up until the timeout is reached.
        /// Once body_frame data is read, the user must call <see cref="Frame.Dispose()"/>() to return the allocated memory to the SDK</para>
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
        public void EnqueueCapture(Capture capture, TimeSpan timeout)
        {
            SafeHandleZeroOrMinusOneIsInvalid captureHandle = (SafeHandleZeroOrMinusOneIsInvalid)typeof(Capture).GetField("handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(capture);
            NativeMethods.k4a_wait_result_t result = NativeMethods.k4abt_tracker_enqueue_capture(this.handle.DangerousGetHandle(), captureHandle.DangerousGetHandle(), (int)timeout.TotalMilliseconds);
            if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_TIMEOUT)
            {
                throw new TimeoutException();
            }
            else if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_FAILED)
            {
                throw new AzureKinectException();
            }
        }

        /// <summary>
        /// Gets the next available body frame.
        /// </summary>
        public Frame PopResult() => this.PopResult(TimeSpan.FromMilliseconds(-1));

        /// <summary>
        /// Gets the next available body frame.
        /// </summary>
        /// <param name="timeout">The time in milliseconds the function should block waiting for the body frame. 
        /// TimeSpan.Zero is a check of the queue without blocking. Passing a value of -1ms will blocking indefinitely.</param>
        /// <returns></returns>
        /// <remarks>
        /// Retrieves the next available body frame result and pop it from the output queue in the <see cref="Tracker"/>. If a new body
        /// frame is not currently available, this function will block up until the timeout is reached.The SDK will buffer at
        /// least three body frames worth of data before stopping new capture being queued by <see cref="EnqueueCapture(Capture, TimeSpan)"/>.
        /// Once body_frame data is read, the user must call Frame.Dispose() to return the allocated memory to the SDK.
        /// </remarks>
        public Frame PopResult(TimeSpan timeout)
        {
            NativeMethods.k4a_wait_result_t result = NativeMethods.k4abt_tracker_pop_result(this.handle.DangerousGetHandle(), out NativeMethods.k4abt_frame_t frame, (int)timeout.TotalMilliseconds);
            if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_TIMEOUT)
            {
                throw new TimeoutException();
            }
            else if (result == NativeMethods.k4a_wait_result_t.K4A_WAIT_RESULT_FAILED)
            {
                throw new AzureKinectException();
            }
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

        /// <summary>
        /// Shutdown the tracker so that no further capture can be added to the input queue.
        /// </summary>
        /// <remarks>
        /// <para>Once the tracker is shutdown, <see cref="EnqueueCapture(Capture)"/>() API will always immediately return failure.</para>
        /// <para>If there are remaining catpures in the tracker queue after the tracker is shutdown, <see cref="PopResult(TimeSpan)"/>() can
        /// still return successfully. Once the tracker queue is empty, the <see cref="PopResult(TimeSpan)"/>() call will always immediately
        /// return failure.</para>
        /// <para>This function may be called while another thread is blocking in <see cref="Capture"/> or <see cref="PopResult(TimeSpan)"/>.
        /// Calling this function while another thread is in that function will result in that function returning a failure.</para>
        /// </remarks>
        public void Shutdown()
        {
            NativeMethods.k4abt_tracker_shutdown(this.handle);
        }

        /// <inheritdoc/>
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
    }
}
