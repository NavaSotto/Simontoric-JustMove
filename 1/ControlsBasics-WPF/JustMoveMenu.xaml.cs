//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using System.Windows.Media;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Media;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using ShapeGame.Speech;
    using ShapeGame.Utils;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class JustMoveMenu
    {
        public static readonly DependencyProperty PageLeftEnabledProperty = DependencyProperty.Register(
            "PageLeftEnabled", typeof(bool), typeof(SimontoricMenu), new PropertyMetadata(false));

        public static readonly DependencyProperty PageRightEnabledProperty = DependencyProperty.Register(
            "PageRightEnabled", typeof(bool), typeof(SimontoricMenu), new PropertyMetadata(false));

        private const double ScrollErrorMargin = 0.001;

        private const int PixelScrollByAmount = 20;

        private readonly KinectSensorChooser sensorChooser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public JustMoveMenu()
        {
            this.InitializeComponent();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            // Clear out placeholder content
            //this.wrapPanel.Children.Clear();

            // Add in display content
            //for (var index = 0; index < 3; ++index)
            //{
            //    //var button = new KinectTileButton { Label = (index + 1).ToString(CultureInfo.CurrentCulture) };
            //    this.wrapPanel.Children.Add(button);
            //}

            //// Bind listner to scrollviwer scroll position change, and check scroll viewer position
            //this.UpdatePagingButtonState();
            //scrollViewer.ScrollChanged += (o, e) => this.UpdatePagingButtonState();
        }

        ///// <summary>
        ///// CLR Property Wrappers for PageLeftEnabledProperty
        ///// </summary>
        //public bool PageLeftEnabled
        //{
        //    get
        //    {
        //        return (bool)GetValue(PageLeftEnabledProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(PageLeftEnabledProperty, value);
        //    }
        //}

        ///// <summary>
        ///// CLR Property Wrappers for PageRightEnabledProperty
        ///// </summary>
        //public bool PageRightEnabled
        //{
        //    get
        //    {
        //        return (bool)GetValue(PageRightEnabledProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(PageRightEnabledProperty, value);
        //    }
        //}

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

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void KinectTileButton_Click_1(object sender, RoutedEventArgs e)
        {
            //var button = (KinectTileButton)e.OriginalSource;
            //var selectionDisplay = new SelectionDisplay(button.Label as string);
            //this.kinectRegionGrid.Children.Add(selectionDisplay);
            //e.Handled = true;
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();


            var w1 = new ShapeGame.JustMoveMainWindow();

            w1.TypeOfGame = "קל";
            w1.Show();


            this.Close(); //GoodLuckWindow
            //this.sensorChooser.Stop();

            //GoodLuck w = new GoodLuck();
            //w.typeOfGame = "קל";
            //w.Show();
            ////Close();


            //backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\‏‏‏‏justMoveScreen1.jpg"));

           
        }

        private void KinectTileButton_Click_2(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();
            var w1 = new ShapeGame.JustMoveMainWindow();

            w1.TypeOfGame = "בינוני";
            w1.Show();


            this.Close(); //GoodLuckWindow
            //this.sensorChooser.Stop();

            //GoodLuck w = new GoodLuck();
            //w.typeOfGame = "קל";
            //w.Show();
            ////Close();
           
            backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\justMoveScreen2.jpg"));

           

        }

        private void KinectTileButton_Click_3(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();
            var w1 = new ShapeGame.JustMoveMainWindow();

            w1.TypeOfGame = "קשה";
            w1.Show();


            this.Close(); //GoodLuckWindow
            //this.sensorChooser.Stop();

            //GoodLuck w = new GoodLuck();
            //w.typeOfGame = "קל";
            //w.Show();
            ////Close();            backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\פרויקטים מעודכנים\Images\‏‏‏‏justMoveScreen3.jpg"));
           
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();
            Menu w = new Menu();
            w.Show();
            Close();
        }

        private void InstructionsClick(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser.Stop();
            JustMoveInstructions w = new JustMoveInstructions();
            w.Show();
            Close();
        }


        ///// <summary>
        ///// Change button state depending on scroll viewer position
        ///// </summary>
        //private void UpdatePagingButtonState()
        //{
        //    this.PageLeftEnabled = scrollViewer.HorizontalOffset > ScrollErrorMargin;
        //    this.PageRightEnabled = scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth - ScrollErrorMargin;
        //}
    }
}
