namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using System.Media;
    using System.IO;
    using System.Windows.Media.Animation;
    using System.Threading;
    using System.ComponentModel;
    using System.Timers;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;



    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class JustMoveWinOrLose
    {

        private readonly KinectSensorChooser sensorChooser1;
        public string winOrLoseMode;



        //#############################################################################################################################
        //DoubleAnimation da = new DoubleAnimation();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public JustMoveWinOrLose()
        {

            this.InitializeComponent();
            this.sensorChooser1 = new KinectSensorChooser();
            this.sensorChooser1.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser1;
            this.sensorChooser1.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser1 };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
            //InitWindow();

        }






        //####################################  KINECT_FUNCTION  ################################################


        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        private void ToResultGame(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser1.Stop();
            JustMoveResultsGame w = new JustMoveResultsGame();
            w.Show();
            Close();

        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser1.Stop();
            JustMoveMenu w = new JustMoveMenu();
            w.Show();
            Close();
        }

        //#########################################################################################################################################
        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser1.Stop();
        }

       
        //####################################END OF KINECT_FUNCTION  ################################################
        private void InitWindow(object sender, EventArgs e)
        {
            if (winOrLoseMode == "win")
            {
                backgroundWindow.ImageSource = new BitmapImage(new Uri("Images/justMoveWinnerscreen.jpg", UriKind.Relative));
                resultGameButton.Visibility = Visibility.Visible; //הכפתור מוצג
                
            }

            else if (winOrLoseMode == "lose")
            {


                resultGameButton.IsEnabled = false;//הכפתור לא ניתן ללחיצה(גם אם בטעות נלחץ על הכפתור המוסתר)0
                backgroundWindow.ImageSource = new BitmapImage(new Uri("Images/justMoveGameOvarScreen.jpg", UriKind.Relative));

                backButton.Margin = new Thickness(1203, 722, 0, 224.4);

            }
        }

      

    }
}



