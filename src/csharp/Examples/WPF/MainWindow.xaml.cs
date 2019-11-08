using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Kinect.BodyTracking.Examples.WPFViewer
{
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Azure Kinect sensor
        /// </summary>
        private readonly Device kinect = null;
         
        /// <summary>
        /// Bitmap to display
        /// </summary>
        private readonly WriteableBitmap bitmap = null;

        private readonly WriteableBitmap bodybitmap = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// The width in pixels of the color image from the Azure Kinect DK
        /// </summary>
        private readonly int colorWidth = 0;

        /// <summary>
        /// The height in pixels of the color image from the Azure Kinect DK
        /// </summary>
        private readonly int colorHeight = 0;

        /// <summary>
        /// Status of the application
        /// </summary>
        private bool running = true;

        private Tracker tracker;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // Open the default device
            int cnt = Device.GetInstalledCount();
            if (cnt == 0)
            {
                _ = MessageBox.Show("No Azure Kinect camera detected", "Error");
                Application.Current.Shutdown();
                return;
            }
            this.kinect = Device.Open();

            // Configure camera modes
            this.kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                // ColorResolution = ColorResolution.Off,
                // DepthMode = DepthMode.NFOV_Unbinned,
                ColorResolution = ColorResolution.R1080p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true
            });

            calibration = this.kinect.GetCalibration();
            this.bitmap = new WriteableBitmap(calibration.ColorCameraCalibration.ResolutionWidth, calibration.ColorCameraCalibration.ResolutionHeight, 96.0, 96.0, PixelFormats.Bgra32, null);
            List<Color> colorPallete = new List<Color>(256){
                Colors.Red, Colors.Green, Colors.Blue, Colors.Cyan, Colors.Yellow, Colors.Magenta, Colors.Transparent,
            };
            while(colorPallete.Count < 255)
            {
                colorPallete.Add(Colors.CornflowerBlue);
            }
            colorPallete.Add(Colors.Transparent);
            this.bodybitmap = new WriteableBitmap(calibration.DepthCameraCalibration.ResolutionWidth, calibration.DepthCameraCalibration.ResolutionHeight, 96.0, 96.0, PixelFormats.Indexed8, new BitmapPalette(colorPallete));

            this.DataContext = this;
            tracker = new Tracker(calibration, new TrackerConfiguration() { CpuOnly = false });
            this.transform = calibration.CreateTransformation();
            this.InitializeComponent();
        }
        private Calibration calibration;
        private Transformation transform;

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.bitmap;
            }
        }
        public ImageSource BodyImageSource
        {
            get
            {
                return this.bodybitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            running = false;
            if (this.tracker != null)
            {
                this.tracker.Shutdown();
                this.tracker.Dispose();
            }
            if (this.kinect != null)
            {
                this.kinect.Dispose();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            while (running)
            {
                using (Capture capture = await Task.Run(() => { return this.kinect.GetCapture(); }))
                {
                    this.bitmap.Lock();

                    var color = capture.Color;
                    var region = new Int32Rect(0, 0, color.WidthPixels, color.HeightPixels);

                    unsafe
                    {
                        using (var pin = color.Memory.Pin())
                        {
                            this.bitmap.WritePixels(region, (IntPtr)pin.Pointer, (int)color.Size, color.StrideBytes);
                        }
                    }

                    this.bitmap.AddDirtyRect(region);
                    this.bitmap.Unlock();
                    tracker.EnqueueCapture(capture);
                    int bodyCount = 0;
                    using (var frame = await Task.Run(() => tracker.PopResult()))
                    {
                        bodyCount = frame.NumberOfBodies;
                        using (var map = frame.GetBodyIndexMap())
                        {
                            this.bodybitmap.Lock();
                            region = new Int32Rect(0, 0, map.WidthPixels, map.HeightPixels);

                            unsafe
                            {
                                using (var pin = map.Memory.Pin())
                                {
                                    this.bodybitmap.WritePixels(region, (IntPtr)pin.Pointer, (int)map.Size, map.StrideBytes);
                                }
                            }
                            this.bodybitmap.AddDirtyRect(region);
                            this.bodybitmap.Unlock();
                        }
                        if (bodyCount > 0)
                        {
                            var skeleton = frame.GetBodySkeleton(0);
                            skeletonRenderer.Calibration = calibration;
                            skeletonRenderer.Skeleton = skeleton;
                            skeletonRenderer.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            skeletonRenderer.Visibility = Visibility.Collapsed;
                        }
                    }
                    this.StatusText = "Received Capture: " + capture.Depth.DeviceTimestamp + " Body count: " + bodyCount.ToString();
                }
            }
        }
    }
}