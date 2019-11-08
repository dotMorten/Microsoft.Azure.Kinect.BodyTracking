using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Azure.Kinect.Sensor;

namespace Microsoft.Azure.Kinect.BodyTracking.Examples.WPFViewer
{
    public class SkeletonRenderer : Canvas
    {
        private SolidColorBrush sharedBrush;
        private System.Windows.Shapes.Ellipse[] joints;
        private Path skeletonPath;
        private const double JointSize = 30;

        public SkeletonRenderer()
        {
            sharedBrush = new SolidColorBrush(Color);

            skeletonPath = new Path()
            {
                Stroke = sharedBrush, StrokeThickness = 10
            };
            Children.Add(skeletonPath);

            joints = new System.Windows.Shapes.Ellipse[32];
            var fill = new SolidColorBrush(Colors.White);
            for (int i = 0; i < joints.Length; i++)
            {
                joints[i] = new System.Windows.Shapes.Ellipse()
                {
                    Fill = fill,
                    Stroke = sharedBrush,
                    StrokeThickness = 10,
                    Width = JointSize,
                    Height = JointSize,
                    Visibility = Visibility.Collapsed
                };
                Children.Add(joints[i]);
            }
        }


        private void UpdateSkeleton()
        {
            if (Calibration == null)
                return;
            var j = Skeleton.Joints;
            for (int i = 0; i < j.Length; i++)
            {
                var elm = joints[i];
                var joint = j[i];
                elm.Visibility = Visibility.Collapsed;
                if (joint.ConfidenceLevel > JointConfidenceLevel.None)
                {
                    var loc = Calibration.TransformTo2D(joint.Position, CalibrationDeviceType.Depth, CalibrationDeviceType.Color);
                    if (loc.HasValue)
                    {
                        Canvas.SetLeft(elm, loc.Value.X - (JointSize * .5));
                        Canvas.SetTop(elm, loc.Value.Y - (JointSize * .5));
                        elm.Visibility = Visibility.Visible;
                    }
                }
            }
            var path = new PathGeometry();
            var handLeft = TransformJoint(Skeleton, JointId.HandLeft);
            var wristLeft = TransformJoint(Skeleton, JointId.WristLeft);
            var elbowLeft = TransformJoint(Skeleton, JointId.ElbowLeft);
            var shoulderLeft = TransformJoint(Skeleton, JointId.ShoulderLeft);
            var spineChest = TransformJoint(Skeleton, JointId.SpineChest);
            var shoulderRight = TransformJoint(Skeleton, JointId.ShoulderRight);
            var elbowRight = TransformJoint(Skeleton, JointId.ElbowRight);
            var wristRight = TransformJoint(Skeleton, JointId.WristRight);
            var handRight = TransformJoint(Skeleton, JointId.HandRight);
            var neck = TransformJoint(Skeleton, JointId.Neck);
            var head = TransformJoint(Skeleton, JointId.Head);
            var nose = TransformJoint(Skeleton, JointId.Nose);
            var spineNaval = TransformJoint(Skeleton, JointId.SpineNaval);
            var hipLeft = TransformJoint(Skeleton, JointId.HipLeft);
            var hipRight = TransformJoint(Skeleton, JointId.HipRight);
            var kneeLeft = TransformJoint(Skeleton, JointId.KneeLeft);
            var kneeRight = TransformJoint(Skeleton, JointId.KneeRight);
            var ankleLeft = TransformJoint(Skeleton, JointId.AnkleLeft);
            var ankleRight = TransformJoint(Skeleton, JointId.AnkleRight);
            var footLeft = TransformJoint(Skeleton, JointId.FootLeft);
            var footRight = TransformJoint(Skeleton, JointId.FootRight);

            if (handLeft.HasValue && wristLeft.HasValue) path.AddGeometry(new LineGeometry(handLeft.Value, wristLeft.Value));
            if (wristLeft.HasValue && elbowLeft.HasValue) path.AddGeometry(new LineGeometry(wristLeft.Value,elbowLeft.Value));
            if (elbowLeft.HasValue && shoulderLeft.HasValue) path.AddGeometry(new LineGeometry(elbowLeft.Value, shoulderLeft.Value));
            if (shoulderLeft.HasValue && spineChest.HasValue) path.AddGeometry(new LineGeometry(shoulderLeft.Value, spineChest.Value));
            if (spineChest.HasValue && shoulderRight.HasValue) path.AddGeometry(new LineGeometry(spineChest.Value, shoulderRight.Value));
            if (shoulderRight.HasValue && elbowRight.HasValue) path.AddGeometry(new LineGeometry(shoulderRight.Value, elbowRight.Value));
            if (elbowRight.HasValue && wristRight.HasValue) path.AddGeometry(new LineGeometry(elbowRight.Value, wristRight.Value));
            if (wristRight.HasValue && handRight.HasValue) path.AddGeometry(new LineGeometry(wristRight.Value, handRight.Value));
            if (spineChest.HasValue && neck.HasValue) path.AddGeometry(new LineGeometry(spineChest.Value, neck.Value));
            if (head.HasValue && neck.HasValue) path.AddGeometry(new LineGeometry(head.Value, neck.Value));
            if (head.HasValue && nose.HasValue) path.AddGeometry(new LineGeometry(head.Value, nose.Value));
            if (spineChest.HasValue && spineNaval.HasValue) path.AddGeometry(new LineGeometry(spineChest.Value, spineNaval.Value));
            if (spineNaval.HasValue && hipLeft.HasValue) path.AddGeometry(new LineGeometry(spineNaval.Value, hipLeft.Value));
            if (spineNaval.HasValue && hipRight.HasValue) path.AddGeometry(new LineGeometry(spineNaval.Value, hipRight.Value));
            if (hipLeft.HasValue && kneeLeft.HasValue) path.AddGeometry(new LineGeometry(hipLeft.Value, kneeLeft.Value));
            if (kneeLeft.HasValue && ankleLeft.HasValue) path.AddGeometry(new LineGeometry(kneeLeft.Value, ankleLeft.Value));
            if (ankleLeft.HasValue && footLeft.HasValue) path.AddGeometry(new LineGeometry(ankleLeft.Value, footLeft.Value));
            if (hipRight.HasValue && kneeRight.HasValue) path.AddGeometry(new LineGeometry(hipRight.Value, kneeRight.Value));
            if (kneeRight.HasValue && ankleRight.HasValue) path.AddGeometry(new LineGeometry(kneeRight.Value, ankleRight.Value));
            if (ankleRight.HasValue && footRight.HasValue) path.AddGeometry(new LineGeometry(ankleRight.Value, footRight.Value));
            skeletonPath.Data = path;
        }

        private Point? TransformJoint(Skeleton skeleton, JointId id)
        {
            Joint joint = skeleton.Joints[(int)id];
            if (joint.ConfidenceLevel == JointConfidenceLevel.None)
            {
                return null;
            }
            System.Numerics.Vector2? v = Calibration.TransformTo2D(joint.Position, CalibrationDeviceType.Depth, CalibrationDeviceType.Color);
            if (!v.HasValue)
            {
                return null;
            }
            return new Point(v.Value.X, v.Value.Y);
        }


        public Calibration Calibration
        {
            get { return (Calibration)GetValue(CalibrationProperty); }
            set { SetValue(CalibrationProperty, value); }
        }

        public static readonly DependencyProperty CalibrationProperty =
            DependencyProperty.Register("Calibration", typeof(Calibration), typeof(SkeletonRenderer), new PropertyMetadata(null));

        public Skeleton Skeleton
        {
            get { return (Skeleton)GetValue(SkeletonProperty); }
            set { SetValue(SkeletonProperty, value); }
        }

        public static readonly DependencyProperty SkeletonProperty =
            DependencyProperty.Register("Skeleton", typeof(Skeleton), typeof(SkeletonRenderer), new PropertyMetadata(default(Skeleton), OnSkeletonPropertyChanged));

        private static void OnSkeletonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkeletonRenderer)d).UpdateSkeleton();
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(SkeletonRenderer), new PropertyMetadata(Colors.Red, OnColorPropertyChanged));

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SkeletonRenderer)d).sharedBrush.Color = (Color)e.NewValue;
        }
    }
}
