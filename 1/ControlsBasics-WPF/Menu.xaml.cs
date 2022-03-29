

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Media;
    using System.IO;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using Microsoft.Kinect;
    using System.Windows.Media;



    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Menu : Window
    {

        SoundPlayer player = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\" + "start_music" + ".wav");
        private readonly KinectSensorChooser sensorChooser;

        public Menu()
        {
            InitializeComponent();
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$4
            //מכאן הכיול עובד מעולה
            //ShapeGame.MainWindow w = new ShapeGame.MainWindow();
            //w.TypeOfGame = "קל";
            //w.Show();
            //Close();

            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            //Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);



            player.Load();
            player.Play();
           


        }

        //הולך למסך המשחק החוויתי
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // SoundPlayer p1 = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\start_music.wav");
            //p1.Load();
            // p1.Play();
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();
            player.Stop();

            //$$$$$$$$$$$$$$$$$$$$$$$$$$44
            //הכיול לא עובד טוב לא ולכן נעשה מסך רגיל שלא משתמש בסנסורי המצלמה
            //BasicGameInstruction w1 = new BasicGameInstructions();
            //w1.Show();
            JustMoveMenu w2 = new JustMoveMenu();
            w2.Show();
            Close();

        }
        //הולך למסך המשחק השיקומי

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // SoundPlayer p1 = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\start_music.wav");
            //p1.Load();
            // p1.Play();

            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();


            player.Stop();
            SimontoricMenu w1 = new SimontoricMenu();
            w1.Show();
            Close();

            

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
        //#########################################################################################################################################
        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            this.sensorChooser.Stop();
            Close();
        }

        

        //####################################END OF KINECT_FUNCTION  ################################################
    }

}
