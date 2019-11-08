using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;

namespace PeopleAlerter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if(!Initialize())
            {
                return;
            }
            System.Threading.CancellationTokenSource tcs = new CancellationTokenSource();
            var task = Process(tcs.Token);
            _ = Console.ReadKey();
            tcs.Cancel();
            await task;
            if(tracker != null)
            {
                tracker.Shutdown();
                tracker.Dispose();
            }
            if(kinect != null)
            {
                kinect.Dispose();
            }
        }

        private static bool Initialize()
        {
            int cnt = Device.GetInstalledCount();
            if (cnt == 0)
            {
                Console.WriteLine("No device connected");
                return false;
            }
            try
            {
                Console.WriteLine("Opening device...");
                kinect = Device.Open();
                Console.WriteLine("Connected");
                Console.WriteLine("Starting device...");
                kinect.StartCameras(new DeviceConfiguration
                {
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R1080p,
                    DepthMode = DepthMode.NFOV_2x2Binned,
                    SynchronizedImagesOnly = true
                });
                Console.WriteLine("Started");
                var calibration = kinect.GetCalibration();
                Console.WriteLine("Creating tracker...");
                tracker = new Tracker(calibration, new TrackerConfiguration() { CpuOnly = false });
            }
            catch(System.Exception ex)
            {
                Console.WriteLine("Failed to start tracking");
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        static Tracker tracker;
        static Device kinect;
        static List<uint> bodyIds = new List<uint>();

        private static async Task Process(CancellationToken cancellationToken)
        {
            Console.WriteLine("Ready!");
            Console.WriteLine("**************************");
            while (!cancellationToken.IsCancellationRequested)
            {
                using (Capture capture = await Task.Run(() => { return kinect.GetCapture(); }))
                {
                    tracker.EnqueueCapture(capture);
                    using (var frame = await Task.Run(() => tracker.PopResult()))
                    {
                        var currentIds = Enumerable.Range(0, (int)frame.NumberOfBodies).Select(i => (index: (uint)i, id: frame.GetBodyId((uint)i))).ToList();
                        var bodiesLeft = bodyIds.Except(currentIds.Select(i=>i.id));
                        var bodiesEntered = currentIds.Where(c=>!bodyIds.Contains(c.id));
                        foreach (var body in bodiesLeft)
                        {
                            Console.WriteLine($"Person #{body} exited the view");
                        }
                        foreach (var (index, id) in bodiesEntered)
                        {
                            var s = frame.GetBodySkeleton(index);
                            var p = s.Joints[(int)JointId.Head].Position;
                            var distance = Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z)) / 1000;
                            Console.WriteLine($"Person #{id} entered the view. {distance.ToString("0.0")}m away");
                        }
                        bodyIds = currentIds.Select(c=>c.id).ToList();
                    }
                }
            }
        }
    }
}
