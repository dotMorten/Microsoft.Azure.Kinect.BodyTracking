using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Kinect.BodyTracking
{
    public class Frame : IDisposable
    {
        private NativeMethods.k4abt_frame_t handle;
        
        internal Frame(NativeMethods.k4abt_frame_t frame)
        {
            this.handle = frame;
        }

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

        public int NumberOfBodies
        {
            get
            {
                return (int)NativeMethods.k4abt_frame_get_num_bodies(handle.DangerousGetHandle());
            }
        }
    }
}
