//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// This module contains code to do Kinect NUI initialization,
// processing, displaying players on screen, and sending updated player
// positions to the game portion for hit testing.


// Tuple<string, string, KeyValuePair<DateTime, DateTime>> resultsGame = new Tuple<string, string, KeyValuePair<DateTime, DateTime>>("h", "m", new KeyValuePair<DateTime, DateTime>(new DateTime(), new DateTime()));

//פונקציות עיקריות
//fallingthings+-12 13 16 17 18

//mainwindow-5 16 17 18 19

//=========================================================================


namespace ShapeGame
{
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
    using Microsoft.Samples.Kinect.WpfViewers;
    using ShapeGame.Speech;
    using ShapeGame.Utils;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    //=========================================================================
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class JustMoveMainWindow : Window
    {

     //=========================================================================
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(JustMoveMainWindow),
                new PropertyMetadata(null));

        #region Private State
        private const int TimerResolution = 2;  // ms
        private const int NumIntraFrames = 3;
        private const int MaxShapes = 80;
        private const double MaxFramerate = 70;
        private const double MinFramerate = 15;
        private const double MinShapeSize = 12;
        private const double MaxShapeSize = 90;
        private const double DefaultDropRate = 2.5;
        private const double DefaultDropSize = 32.0;
        private const double DefaultDropGravity = 1.0;

        private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();
        private readonly SoundPlayer popSound = new SoundPlayer();
        private readonly SoundPlayer hitSound = new SoundPlayer();
        private readonly SoundPlayer squeezeSound = new SoundPlayer();
        private readonly SoundPlayer playMusic = new SoundPlayer();
        private readonly KinectSensorChooser sensorChooser1;

        private double dropRate = DefaultDropRate;
        private double dropSize = DefaultDropSize;
        private double dropGravity = DefaultDropGravity;
        private DateTime lastFrameDrawn = DateTime.MinValue;
        private DateTime predNextFrame = DateTime.MinValue;
        private double actualFrameTime;
        private Skeleton[] skeletonData;

        private double targetFramerate = MaxFramerate;
        private int frameCount;
        private bool runningGameThread;
        private FallingThings myFallingThings;
        private int playersAlive;
        private System.Threading.Thread myGameThread;
        private Rect playerBounds;  // Player(s) placement in scene (z collapsed):
        private Rect screenRect;   // Player(s) placement in scene (z collapsed):
        private int numOfPlayers = 0;
        //private Boolean animation = false;
        //private int currentIndexOfmovementImg = 0;
        private int currentIndexOfGiftImg = 0;
        private int currentImgIndex=0;
        private List<string> movementImgList = new List<string>() { "D1R", "D1L", "D2R", "D2L" };
        private List<string> shapeImgList = new List<string>() { "shape1", "shape2", "shape3", "shape4", "shape5", "shape6" };
        private bool onePlayer = false;
        private int NumOfLevels;
        private int currentPlayerId;
        private ResultGame resultGame = new ResultGame { };
        private SpeechRecognizer mySpeechRecognizer;
        private int maxDisqualification;
        public string playerMode;
        public string TypeOfGame;




        public List<KeyValuePair<string, List<double>>> giftImgList = new List<KeyValuePair<string, List<double>>>()
        {
           new KeyValuePair<string, List<double>> ( "coin",  new List<double>(){ 530, 100, 340, 1450 }) ,/*יכול לפגוע במתנה רק עם הראש */
           new KeyValuePair<string, List<double>>( "coin",new List<double>(){800, 150, -100, 1150 }),/*זיהוי קולי-לצעוק "ביגר" */
           new KeyValuePair<string, List<double>>( "normalGift",new List<double>(){130, 100, 730, 1450})/*יכול לפגוע במתנה עם כל הגוף */
        };


        private struct ResultGame
        {
            public string mode;
            public string level;
            public int scores;
            public string Disqualification;
            public int grade;
            public DateTime start;
            public DateTime end;
            //public int durations;
        }

       
     
        #endregion Private State


        //=========================================================================

        #region ctor + Window Events

        //1-ctor
        public JustMoveMainWindow()
        {

            InitializeComponent();
            

            this.KinectSensorManager = new KinectSensorManager();
            this.KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;
            this.DataContext = this.KinectSensorManager;


           
            ////תמונה של זיהוי קולי(תכלת)ת
            this.SensorChooserUI.KinectSensorChooser = sensorChooser1;
            
            ////תמונה של מצלמה(סגול)ת
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser1;
            this.sensorChooser1 = new KinectSensorChooser();
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser1;
            this.sensorChooser1.Start();
            sensorChooser1.Start();


            // Bind the KinectSensor from the sensorChooser to the KinectSensor on the KinectSensorManager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser1 };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            //this.RestoreWindowState();
        }
        //~1

        //=========================================================================
        //2

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }
        //~2
        //=========================================================================
        //3

        // Since the timer resolution defaults to about 10ms precisely, we need to
        // increase the resolution to get framerates above between 50fps with any
        // consistency.
        [DllImport("Winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern int TimeBeginPeriod(uint period);

        //~3
        //=========================================================================
        //4

        private void RestoreWindowState()
        {
            // Restore window state to that last used
            //Rect bounds = Properties.Settings.Default.PrevWinPosition;
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$44
            //$$שמתי בהערה כי הורס את הכיול כאשר מגיעים מהתפריט 
            //if (bounds.Right != bounds.Left)
            //{
            //    this.Top = bounds.Top;
            //    this.Left = bounds.Left;
            //    this.Height = bounds.Height;
            //    this.Width = bounds.Width;
            //}

            this.WindowState = (WindowState)Properties.Settings.Default.WindowState;
        }
        //~4
        //=========================================================================

        //5
        private void WindowLoaded(object sender, EventArgs e)
        {


            this.myFallingThings = new FallingThings(MaxShapes, this.targetFramerate, NumIntraFrames);

            
            InitAcocordingTypeOfShape();            //מאתחל נתונים לפי בחירת רמת הקושי

            resultGame.level = TypeOfGame; //קל/בינני/קשה-עבור תוצאות המשחק
           
            updateImgSource();

            playfield.ClipToBounds = true;
            this.UpdatePlayfieldSize();
            this.myFallingThings.SetNumOfItrations(NumOfLevels);//$$ //בזמן טעינת חלון המשחק עדכן את מספר שלבים לפי דרגת הקושי שנבחרה
            this.myFallingThings.SetGravity(this.dropGravity);
            this.myFallingThings.SetDropRate(this.dropRate);
            this.myFallingThings.SetSize(this.dropSize);
            this.myFallingThings.SetPolies(PolyType.All);

          
            this.myFallingThings.SetGameMode(GameMode.Off);//$$  //בזמן טעינת חלון המשחק מצב המשחק-לכבוי

            this.popSound.Stream = Properties.Resources.Pop_5;
            this.hitSound.Stream = Properties.Resources.Hit_2;
            this.squeezeSound.Stream = Properties.Resources.Squeeze;
            this.playMusic.Stream = Properties.Resources.playMusic;//$$מוזיקת התחלה
            

            this.popSound.Play();

            TimeBeginPeriod(TimerResolution);
            myGameThread = new Thread(this.GameThread);
            myGameThread.SetApartmentState(ApartmentState.STA);
            myGameThread.Start();

            double size = this.screenRect.Width / 30;
            var windowWidth = this.screenRect.Width/2;//$$770
            var windowHeight = this.screenRect.Height/2;//$$702
            System.Windows.Point positionOnScreen = new Point(this.screenRect.Width / 2, this.screenRect.Height / 2);
            string str = "Just Move";
            FlyingText.NewFlyingText(size, positionOnScreen, str);
            
            //$$$$$$$$$$$מיקום טקסט Just Move$$$$$$$$$$4
            //FlyingText.NewFlyingText(21, new Point(310, 344), "str");  //$$

        }

        //~5
      //=========================================================================
      //6
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            sensorChooser1.Stop();

            this.runningGameThread = false;
            Properties.Settings.Default.PrevWinPosition = this.RestoreBounds;
            Properties.Settings.Default.WindowState = (int)this.WindowState;
            Properties.Settings.Default.Save();

        }
        //~6
        //=========================================================================

        //7
        private void WindowClosed(object sender, EventArgs e)
        {
            this.KinectSensorManager.KinectSensor = null;
        }
        //~7
        //=========================================================================


        #endregion ctor + Window Events

        #region Kinect discovery + setup

        //8
        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
            {
                this.UninitializeKinectServices(args.OldValue);
            }

            // Only enable this checkbox if we have a sensor
            enableAec.IsEnabled = null != args.NewValue;

            if (null != args.NewValue)
            {
                this.InitializeKinectServices(this.KinectSensorManager, args.NewValue);
            }
        }
        //~8
        //=========================================================================

        //9
        // Kinect enabled apps should customize which Kinect services it initializes here.
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            sensor.SkeletonFrameReady += this.SkeletonsReady;
            kinectSensorManager.TransformSmoothParameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            kinectSensorManager.SkeletonStreamEnabled = true;
            kinectSensorManager.KinectSensorEnabled = true;

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // Start speech recognizer after KinectSensor started successfully.
                this.mySpeechRecognizer = SpeechRecognizer.Create();

                if (null != this.mySpeechRecognizer)
                {
                    this.mySpeechRecognizer.SaidSomething += this.RecognizerSaidSomething;
                    this.mySpeechRecognizer.Start(sensor.AudioSource);
                }

                enableAec.Visibility = Visibility.Visible;
                this.UpdateEchoCancellation(this.enableAec);
            }
        }
        //~9
        //=========================================================================

         //10
        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            sensor.SkeletonFrameReady -= this.SkeletonsReady;

            if (null != this.mySpeechRecognizer)
            {
                this.mySpeechRecognizer.Stop();
                this.mySpeechRecognizer.SaidSomething -= this.RecognizerSaidSomething;
                this.mySpeechRecognizer.Dispose();
                this.mySpeechRecognizer = null;
            }

            enableAec.Visibility = Visibility.Collapsed;
        }
        //~10
        //=========================================================================



        #endregion Kinect discovery + setup

        #region Kinect Skeleton processing
        private void SkeletonsReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    int skeletonSlot = 0;

                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);


                    foreach (Skeleton skeleton in this.skeletonData)
                    {

                        if (SkeletonTrackingState.Tracked == skeleton.TrackingState)
                        {
                            //Player player;
                            //if (this.players.ContainsKey(skeletonSlot))
                            //{
                            //    player = this.players[skeletonSlot];
                            //}
                            //else
                            //{
                            //    player = new Player(skeletonSlot);
                            //    player.SetBounds(this.playerBounds);
                            //    this.players.Add(skeletonSlot, player);
                            //}

                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                            //שינינו כדי לאפשר שחקן יחיד בלבד במשחק-אם רוצים כם 2 שחקנים צריך להחליף עם ההערה
                            //*בפעם הראשונה ->נכנסים ל"עלס" כיוון שלא קיים שחקן עם "איידי"="סקלטוןסלוט"   
                            //*כל עוד נקודת הכיול לא נעלמה ->ניכנס כל הזמן ל"עיף" כיוון שהאיידי של השחקן עדיין נמצא במערך
                            //*"כאשר נקודת הכיול נעלמת -הפונקציה "צקפליירס" מסירה את ה"איידי" של השחקן הנוכחי מהרשימה-->וניכנס ל"עלס" כיוון שלא קיים שחקן עם "איידי"="סקלטוןסלוט" 
                            //*(כאשר נכנס שחקן חדש->וניכנס ל"עלס" כיוון שלא קיים שחקן עם איידי=סקלטוןסלוט כיוון שישנו איידי ששוה לסקלטוןסלוט בעל ערך אחר של השחקן הראשון
                            //כי הרי הסקלטוןסלטו מתחיל ב-0 ועולה ב1 בכל הוספת שחקן למערך) ק

                            //מצבי שחקנים:
                            //*נכנס-בגלל שפלס נכנס ועושה טרו וכך נשאר
                            //*נכנס יוצא- בגלל שפלס נכנס ועושה טרו ביציאה מעדכן לפלס
                            //*נכנס ואז שחקן נכנס אחריו-בגלל שפלס נכנס ועושה טרו וכך 
                            //*נשאר ואז שחקן מגיע והוא טרו אז לא נכנס
                            // שחקנים נכנסים יחד-בגלל שפלס הראשון נכנס ועושה טרו השני רוצה להיכנס אבל בגלל שטרו לא נכנס

                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//

                            Player player = new Player(-1);

                            if (this.players.ContainsKey(skeletonSlot))
                            {
                                player = this.players[skeletonSlot];
                            }
                            else
                            {
                                if (!onePlayer)
                                {
                                    SensorChooserUI.Visibility = Visibility.Hidden;

                                    player = new Player(skeletonSlot);
                                    player.SetBounds(this.playerBounds);
                                    player.SetJointsAndBonesBrushesOfPlayers(0);
                                    this.players.Add(skeletonSlot, player);
                                    onePlayer = true;
                                }
                            }

                            player.LastUpdated = DateTime.Now;


                            //player.LastUpdated = DateTime.Now;

                            // Update player's bone and joint positions
                            if (skeleton.Joints.Count > 0)
                            {
                                player.IsAlive = true;

                                // Head, hands, feet (hit testing happens in order here)
                                player.UpdateJointPosition(skeleton.Joints, JointType.Head);
                                player.UpdateJointPosition(skeleton.Joints, JointType.HandLeft);
                                player.UpdateJointPosition(skeleton.Joints, JointType.HandRight);
                                player.UpdateJointPosition(skeleton.Joints, JointType.FootLeft);
                                player.UpdateJointPosition(skeleton.Joints, JointType.FootRight);

                                // Hands and arms
                                player.UpdateBonePosition(skeleton.Joints, JointType.HandRight, JointType.WristRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.WristRight, JointType.ElbowRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ElbowRight, JointType.ShoulderRight);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HandLeft, JointType.WristLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.WristLeft, JointType.ElbowLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ElbowLeft, JointType.ShoulderLeft);

                                // Head and Shoulders
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderCenter, JointType.Head);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderLeft, JointType.ShoulderCenter);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderRight);

                                // Legs
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipLeft, JointType.KneeLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.KneeLeft, JointType.AnkleLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.AnkleLeft, JointType.FootLeft);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HipRight, JointType.KneeRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.KneeRight, JointType.AnkleRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.AnkleRight, JointType.FootRight);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HipLeft, JointType.HipCenter);
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipCenter, JointType.HipRight);

                                // Spine
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipCenter, JointType.ShoulderCenter);
                            }
                        }

                        skeletonSlot++;
                    }
                }
            }
        }

        //=========================================================================


        private void CheckPlayers()
        {
            foreach (var player in this.players)
            {

                if (!player.Value.IsAlive)
                {
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //כאשר נקודת הכיול כבר לא קולטת את השחקן(לא בפעם הראשונה) 
                    var p = player.Value.GetId();//$$
                    // Player left scene since we aren't tracking it anymore, so remove from dictionary

                    this.players.Remove(player.Value.GetId());
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //לקבל אינדיקציה כדי לאפשר שחקן יחיד בלבד
                    onePlayer = false;//$$
                    break;
                }
            }

            // Count alive players
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //alive-מספר השחקנים שיש במשחק
            int alive = this.players.Count(player => player.Value.IsAlive);

            if (alive != this.playersAlive)
            {
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                //ישנם 2 שחקנים במשחק ולכן מצב המשחק =ל2 שחקנים
                if (alive == 2)
                {
                    this.myFallingThings.SetGameMode(GameMode.TwoPlayer);
                }
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                //ישנו שחקן אחד בלבד במשחק ולכן מצב המשחק =ל2שחקן אחד
                else if (alive == 1)
                {
                    this.myFallingThings.SetGameMode(GameMode.Solo);
                }
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                //אים אף שחקן במשחק ולכן מצב המשחק=ל כבוי
                else if (alive == 0)
                {
                    this.myFallingThings.SetGameMode(GameMode.Off);
                }

                if ((this.playersAlive == 0) && (this.mySpeechRecognizer != null))
                {
                    //$$הורדתי את הכיתוב שהופיע תמיד למטה
                    //BannerText.NewBanner(
                    //    Properties.Resources.Vocabulary,
                    //    this.screenRect,
                    //    true,
                    //    System.Windows.Media.Color.FromArgb(200, 255, 255, 255));
                }

                this.playersAlive = alive;
            }
        }

        //=========================================================================


        private void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePlayfieldSize();
        }


        //=========================================================================


        private void UpdatePlayfieldSize()
        {
            // Size of player wrt size of playfield, putting ourselves low on the screen.
            this.screenRect.X = 0;
            this.screenRect.Y = 0;
            this.screenRect.Width = this.playfield.ActualWidth;
            this.screenRect.Height = this.playfield.ActualHeight;

            BannerText.UpdateBounds(this.screenRect);

            //מיקום ציור השחקן הראשון
            this.playerBounds.X = 0;
            this.playerBounds.Width = this.playfield.ActualWidth;
            //var a1 = this.playerBounds.Width;//$$
            this.playerBounds.Y = this.playfield.ActualHeight * 0.2;
            //var a2 = this.playerBounds.Y;//$$
            this.playerBounds.Height = this.playfield.ActualHeight * 0.55;
            //var a3 = this.playerBounds.Height;//$$

            foreach (var player in this.players)
            {
                player.Value.SetBounds(this.playerBounds);
            }

            Rect fallingBounds = this.playerBounds;
            fallingBounds.Y = 0;
            fallingBounds.Height = playfield.ActualHeight;
            if (this.myFallingThings != null)
            {
                this.myFallingThings.SetBoundaries(fallingBounds);
            }
        }
        #endregion Kinect Skeleton processing

        //=========================================================================
        //11
        #region GameTimer/Thread
        private void GameThread()
        {
            this.runningGameThread = true;
            this.predNextFrame = DateTime.Now;
            this.actualFrameTime = 1000.0 / this.targetFramerate;

            // Try to dispatch at as constant of a framerate as possible by sleeping just enough since
            // the last time we dispatched.
            while (this.runningGameThread)
            {
                // Calculate average framerate.  
                DateTime now = DateTime.Now;

                resultGame.start = now; //$$

                if (this.lastFrameDrawn == DateTime.MinValue)
                {
                    this.lastFrameDrawn = now;
                }

                double ms = now.Subtract(this.lastFrameDrawn).TotalMilliseconds;
                this.actualFrameTime = (this.actualFrameTime * 0.95) + (0.05 * ms);
                this.lastFrameDrawn = now;

                // Adjust target framerate down if we're not achieving that rate
                this.frameCount++;
                if ((this.frameCount % 100 == 0) && (1000.0 / this.actualFrameTime < this.targetFramerate * 0.92))
                {
                    this.targetFramerate = Math.Max(MinFramerate, (this.targetFramerate + (1000.0 / this.actualFrameTime)) / 2);
                }

                if (now > this.predNextFrame)
                {
                    this.predNextFrame = now;
                }
                else
                {
                    double milliseconds = this.predNextFrame.Subtract(now).TotalMilliseconds;
                    if (milliseconds >= TimerResolution)
                    {
                        Thread.Sleep((int)(milliseconds + 0.5));
                    }
                }

                this.predNextFrame += TimeSpan.FromMilliseconds(1000.0 / this.targetFramerate);

                this.Dispatcher.Invoke(DispatcherPriority.Send, new Action<int>(this.HandleGameTimer), 0);
            }
        }
        //~11
        //=========================================================================
        //12
        private void HandleGameTimer(int param)
        {
            if (myFallingThings.isEndGame)
            {
                TimeSpan x = myFallingThings.endGameTime - myFallingThings.gameStartTime;
                
                ResultGame r_g = new ResultGame
                {


                    mode = myFallingThings.endGameMode,
                    level = this.TypeOfGame,
                    scores = myFallingThings.sumOfScore,
                    Disqualification = myFallingThings.numOfDisqu.ToString() + "/" + myFallingThings.maxDisqu.ToString(),
                    grade = 100 - 10 * myFallingThings.numOfDisqu,
                    start = myFallingThings.gameStartTime,
                    end = myFallingThings.endGameTime,
                    //durations =,

                    };

               




            }

                // if(myFallingThings.doAnimation)

                //if(myFallingThings.toUpdateScore)
                //{
                //    myFallingThings.toUpdateScore = false;
                //        this.myFallingThings.DrawFrame(this.playfield.Children);
                // }
                // Every so often, notify what our actual framerate is
                if ((this.frameCount % 100) == 0)
            {
                this.myFallingThings.SetFramerate(1000.0 / this.actualFrameTime);
            }

            // Advance animations, and do hit testing.
            for (int i = 0; i < NumIntraFrames; ++i)
            {
                foreach (var pair in this.players)
                {


                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //ברגע שיש כבר שחקן אחד במערך-ברגע שיציג שחקן לפחות אחד על המסך יתחיל לחפש פגיעה
                    HitType hit = this.myFallingThings.LookForHits(pair.Value.Segments, pair.Value.GetId());



                    //@@$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //@@אם היתה פגיעה ב"סינג" לא משנה מאיזה סוג
                    if (hit != HitType.None)
                    {

                        currentPlayerId = pair.Key;
                        // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                        // צביעת הסגמנט שפגע ב"סינג
                        this.players[pair.Key].isPlayerHit = true;
                        this.players[pair.Key].typeAndPlayerIdHit = myFallingThings.GetTypeAndIdPlayerOfCurrentHitThing();




                        //הסרת תמונת הסינג שנפגע
                        if (myFallingThings.removeThingImg)
                        {
                            //myFallingThings.removeThingImg = false;
                           
                            switch (myFallingThings.currentHitThing)
                            {

                                case "shape":
                                    shapeImg.Visibility = Visibility.Hidden;
                                    break;
                                case "basket":
                                    
                                    basketImg.Visibility = Visibility.Hidden;
                                    break;
                                case "gift":
                                    //$$במצב שכן נוספה אופציה של מתנה לאותו זוג תפוח-סל

                                    giftImg.Visibility = Visibility.Hidden;
                                    
                                    break;




                            }
                            if (myFallingThings.loadNewThingsImg)
                            {
                                fullBasketImg.Visibility = Visibility.Visible;
                                giftImg.Visibility = Visibility.Hidden;
                                fullBasketImg.Margin = new Thickness(200, 300, 500, 1000);
                                fullBasketImg.Height = 300;
                                fullBasketImg.Width = 400;
                                this.squeezeSound.Play();


                                BeginStoryboard sb = this.FindResource("fullBasketAnimation") as BeginStoryboard;
                                 myFallingThings.doAnimation = true;
                                //this.myFallingThings.DrawFrame(this.playfield.Children);
                                sb.Storyboard.Begin();
                            }
                        }



                    }//@@
                    if ((hit & HitType.Squeezed) != 0)//bbb
                    {
                        this.squeezeSound.Play();




                    }
                    else if ((hit & HitType.Popped) != 0)//bbb
                    {
                        this.popSound.Play();

                    }

                    else if ((hit & HitType.Hand) != 0)//bbb
                    {
                        this.hitSound.Play();
                    }
                    else if ((hit & HitType.Arm) != 0)//bbb
                    {
                        ;
                    }
                }
                //updateImgSource();
                //if (!animation)
                //{
                    this.myFallingThings.AdvanceFrame();
               // }//
               // else
                //{
                   // animation = false;
                //}






            }




            // Draw new Wpf scene by adding all objects to canvas
            playfield.Children.Clear();
            int a1 = myFallingThings.GetCurrentItration();

            if (!myFallingThings.isEndGame)
            {


                this.myFallingThings.DrawFrame(this.playfield.Children);
                foreach (var player in this.players)
                {

                    //$$ לדעת כמה שחקנים יש במערך בכל זמן-לשם בדיקה
                    if (players.Count > 1)
                        numOfPlayers = players.Count;//$$
                                                     //player.Value.playerCenter = new Point(700, 20);


                    //$$הצגת כל השחקנים במערך
                    player.Value.Draw(playfield.Children);

                }

                //$$הורדתי את הכיתוב 
                //BannerText.Draw(playfield.Children);
                //FlyingText.Draw(playfield.Children);

            }
            else if (myFallingThings.isEndGame)
            {
                if(myFallingThings.endGameMode == "winner")
                    backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\46\27\FinalProject\Images\justMoveWinnerscreen.jpg", UriKind.Relative));


                else if (myFallingThings.endGameMode == "lost")

                         backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\46\27\FinalProject\Images\justMoveGameOverScreen.jpg", UriKind.Relative));

                // backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\פרויקטים מעודכנים\27\FinalProject\Images\‏‏‏‏justMoveWinnerscreen.jpg"));
                foreach (var player in this.players)
                {

                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //לדעת כמה שחקנים יש במערך בכל זמן-לשם בדיקה
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//

                    if (players.Count > 1)//$$
                        numOfPlayers = players.Count;//$$
                                                     //player.Value.playerCenter = new Point(700, 20);

                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //הצגת כל השחקנים במערך
                    player.Value.Draw(playfield.Children);

                }
                movementImg.Visibility = Visibility.Hidden;
                shapeImg.Visibility = Visibility.Hidden;
                basketImg.Visibility = Visibility.Hidden;
                giftImg.Visibility = Visibility.Hidden;


            }



            this.CheckPlayers();

        }
        //~12

        //=========================================================================

        #endregion GameTimer/Thread


        #region Kinect Speech processing
        //=========================================================================
        //13
        private void RecognizerSaidSomething(object sender, SpeechRecognizer.SaidSomethingEventArgs e)
        {
            //$$add gift
            FlyingText.NewFlyingText(this.screenRect.Width / 30, new Point(this.screenRect.Width / 2, this.screenRect.Height / 2), "gift");
            

            switch (e.Verb)
            {
                case SpeechRecognizer.Verbs.Pause:
                    this.myFallingThings.SetDropRate(0);
                    this.myFallingThings.SetGravity(0);
                    break;
                case SpeechRecognizer.Verbs.Resume:
                    this.myFallingThings.SetDropRate(this.dropRate);
                    this.myFallingThings.SetGravity(this.dropGravity);
                    break;
                case SpeechRecognizer.Verbs.Reset:
                    this.dropRate = DefaultDropRate;
                    this.dropSize = DefaultDropSize;
                    this.dropGravity = DefaultDropGravity;
                    this.myFallingThings.SetPolies(PolyType.All);
                    this.myFallingThings.SetDropRate(this.dropRate);
                    this.myFallingThings.SetGravity(this.dropGravity);
                    this.myFallingThings.SetSize(this.dropSize);
                    this.myFallingThings.SetShapesColor(System.Windows.Media.Color.FromRgb(0, 0, 0), true);
                    this.myFallingThings.Reset();
                    break;
                case SpeechRecognizer.Verbs.DoShapes:
                    this.myFallingThings.SetPolies(e.Shape);
                    break;
                case SpeechRecognizer.Verbs.RandomColors:
                    this.myFallingThings.SetShapesColor(System.Windows.Media.Color.FromRgb(0, 0, 0), true);
                    break;
                case SpeechRecognizer.Verbs.Colorize:
                    this.myFallingThings.SetShapesColor(e.RgbColor, false);
                    break;
                case SpeechRecognizer.Verbs.ShapesAndColors:
                    this.myFallingThings.SetPolies(e.Shape);
                    this.myFallingThings.SetShapesColor(e.RgbColor, false);
                    break;
                case SpeechRecognizer.Verbs.More:
                    this.dropRate *= 1.5;
                    this.myFallingThings.SetDropRate(this.dropRate);
                    break;
                case SpeechRecognizer.Verbs.Fewer:
                    this.dropRate /= 1.5;
                    this.myFallingThings.SetDropRate(this.dropRate);
                    break;
                   //$$הוספנו תנאי למתנה
                case SpeechRecognizer.Verbs.Bigger:
                    if (myFallingThings.addGift && myFallingThings.typeOfGift == "SR" && !myFallingThings.GetIsTimeOut() && myFallingThings.currentHitThing == "basket")
                    {
                        myFallingThings.fine = -10;
                        FlyingText.NewFlyingText(this.screenRect.Width / 30, new Point(this.screenRect.Width / 2, this.screenRect.Height / 2), "+10");
                        giftImg.Visibility = Visibility.Hidden;
                        myFallingThings.AdvanceFrame();
                        myFallingThings.isGiftHit = true; //מעדכן שהיתה פגיעה במתנה


                    }
                    //$$
                    this.dropSize *= 1.5;
                    if (this.dropSize > MaxShapeSize)
                    {
                        this.dropSize = MaxShapeSize;
                    }

                    this.myFallingThings.SetSize(this.dropSize);
                    break;


                case SpeechRecognizer.Verbs.Biggest:
                    this.dropSize = MaxShapeSize;
                    this.myFallingThings.SetSize(this.dropSize);
                    break;
                case SpeechRecognizer.Verbs.Smaller:
                    this.dropSize /= 1.5;
                    if (this.dropSize < MinShapeSize)
                    {
                        this.dropSize = MinShapeSize;
                    }

                    this.myFallingThings.SetSize(this.dropSize);
                    break;
                case SpeechRecognizer.Verbs.Smallest:
                    this.dropSize = MinShapeSize;
                    this.myFallingThings.SetSize(this.dropSize);
                    break;
                case SpeechRecognizer.Verbs.Faster:
                    this.dropGravity *= 1.25;
                    if (this.dropGravity > 4.0)
                    {
                        this.dropGravity = 4.0;
                    }

                    this.myFallingThings.SetGravity(this.dropGravity);
                    break;
                case SpeechRecognizer.Verbs.Slower:
                    this.dropGravity /= 1.25;
                    if (this.dropGravity < 0.25)
                    {
                        this.dropGravity = 0.25;
                    }

                    this.myFallingThings.SetGravity(this.dropGravity);
                    break;



            }
        }
        //~13
        //=========================================================================
        //14
        private void EnableAecChecked(object sender, RoutedEventArgs e)
        {
            var enableAecCheckBox = (CheckBox)sender;
            this.UpdateEchoCancellation(enableAecCheckBox);
        }
        //~14
        //=========================================================================
        //15
        private void UpdateEchoCancellation(CheckBox aecCheckBox)
        {
            this.mySpeechRecognizer.EchoCancellationMode = aecCheckBox.IsChecked != null && aecCheckBox.IsChecked.Value
                ? EchoCancellationMode.CancellationAndSuppression
                : EchoCancellationMode.None;
        }
        //~15

        #endregion Kinect Speech processing

        //=========================================================================
        //16
        public void updateImgSource()
        {


            if (myFallingThings.GetCurrentItration() <= NumOfLevels)//במקרה שאנחנו עדיין עוברים על הסינגים ולא הגענו למספר האיטרציות שעלינו לעבור
            {
                //if (currentIndexOfmovementImg == movementImgList.Count)
                //    currentIndexOfmovementImg = 0;

                //else if (currentIndexOfShapeImg <= shapeImgList.Count)
                //    currentIndexOfShapeImg = 0;

                //וכעת ניתן לעדכן את התמונות המתאימות
                if (myFallingThings.GetCurrentItration() == 2)
                    currentImgIndex = 0;

                Tuple<Point, Point, KeyValuePair<List<double>, List<double>>, KeyValuePair<string, string>> currentThingsOnScreen = this.myFallingThings.GetCurrentThings((currentImgIndex) % 4);//סינגים נוכחים במסך

                //מקבל את הסוג ואת המיקום של הסינגים הנוכחיים רק בצורה של מרגין ולא פוינט

                Tuple<KeyValuePair<List<double>, string>, KeyValuePair<List<double>, string>, KeyValuePair<List<double>, string>> imagesToUpdate =
                 new Tuple<KeyValuePair<List<double>, string>, KeyValuePair<List<double>, string>, KeyValuePair<List<double>, string>>
                 (new KeyValuePair<List<double>, string>(new List<double>(), "movementImg"), new KeyValuePair<List<double>, string>(currentThingsOnScreen.Item3.Key, "shapeImg"), new KeyValuePair<List<double>, string>(currentThingsOnScreen.Item3.Value, "basketImg"));


                SetImg(imagesToUpdate);//מעדכן את התמונות על המסך

            }


        }
        //~16
        //=========================================================================$$

         //17
        private void SetImg(Tuple<KeyValuePair<List<double>, string>, KeyValuePair<List<double>, string>, KeyValuePair<List<double>, string>> updatedImgList)
        {
            if(myFallingThings.nextItration)
            {
                currentImgIndex = 0;
                currentIndexOfGiftImg = 0;

            }
            //myFallingThings.addGift = false;//צריך להוסיף סינג מסוג מתנה


            //margin
            //1-left
            //2-top
            //3-right
            //4-bottom

            //להזיז למטה-נוסיף ל2 ונוריד את אותו יחס ל-4
            //להזיז למעלה-נוריד ל2 ונוסיף את אותו יחס ל-4
            //להזיז ימינה-נוסיף ל1 ונוריד את אותו יחס ל-3
            //להזיז שמאלה-נוריד ל1 ונוסיף את אותו יחס ל-3


            //מעדכן את הצורה למעלה שמתעדכנת כל 2 פגיעות של סינגים כלומר לאחר הכנסת הסינג לסל
            if (currentImgIndex % 2 == 0)  //שואל האם הסינג הוכנס לסל
            {
                string specificMovementImg = movementImgList[currentImgIndex % 3];
                movementImg.Source = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\" + specificMovementImg + ".PNG"));
                //movementImg.Source = new BitmapImage(new Uri("Images/" + specificMovementImg + ".PNG", UriKind.Relative));
                movementImg.Visibility = Visibility.Visible;

            }




            //מעדכן את הצורה למעלה-התפוח

            string specificShapeImg = shapeImgList[currentImgIndex % 5];
            //shapeImg.Source = new BitmapImage(new Uri("Images/" + nameOfObject + ".png", UriKind.Relative));
            shapeImg.Source = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\" + specificShapeImg + ".png"));

            var marginShapeImg = updatedImgList.Item2.Key;
            shapeImg.Margin = new Thickness(marginShapeImg[0], marginShapeImg[1], marginShapeImg[2], marginShapeImg[3]);
            //shapeImg.Margin = new Thickness(530, 100, 340, 1450);
            shapeImg.Width = 100;
            shapeImg.Height = 100;
            shapeImg.Visibility = Visibility.Visible;




            //מעדכן את הצורה של הסל
            //basketImg.Source = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\46\27\FinalProject\Images\emptyBasket.png"));
            var marginBasketImg = updatedImgList.Item3.Key;
            basketImg.Margin = new Thickness(marginBasketImg[0], marginBasketImg[1], marginBasketImg[2], marginBasketImg[3]);
          
            //basketImg.Margin = new Thickness(100, 250,600, 1050);
            //basketImg.Width = 200;
            //basketImg.Height = 200;
            basketImg.Visibility = Visibility.Visible;




            //נעדכן מתנה כל 2 זוגות צורה-סל
            if (currentImgIndex % 2 == 0)
            {
                string specificGiftImg = giftImgList[/*rand.Next(0,2)*/currentIndexOfGiftImg % 3].Key;
                //giftImg.Source = new BitmapImage(new Uri("Images/" + specificGiftImg + ".png", UriKind.Relative));
                giftImg.Source = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\" + specificGiftImg + ".png"));
                var giftMarginImg = giftImgList[currentImgIndex % 3].Value;
                giftImg.Margin = new Thickness(giftMarginImg[0], giftMarginImg[1], giftMarginImg[2], giftMarginImg[3]);
                //giftImg.Margin = new Thickness(530, 100, 340, 1450);
                giftImg.Width = 100;
                giftImg.Height = 100;

                giftImg.Visibility = Visibility.Visible;
               
                 
                  //myFallingThings.addGift = true;//צריך להוסיף סינג מסוג מתנה
                  myFallingThings.typeOfGift = specificGiftImg;
                // myFallingThings.isGiftHit = false;
                currentIndexOfGiftImg++;
               
            }
           


            currentImgIndex++;
            
        }
        //~17
        //=========================================================================$$
        //18
        private void InitAcocordingTypeOfShape()
        {


            if (TypeOfGame == "קל")
            {
                //windowBackground.ImageSource = new BitmapImage(new Uri("Images/" + "b1.jpg", UriKind.Relative));
                backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\46\27\FinalProject\Images\‏‏‏‏justMoveGameScreen.png", UriKind.Relative));
                myFallingThings.maxSecondForThingsPair = 5;
                myFallingThings.scoreGameToAdd = 1;
                NumOfLevels = 5;
                maxDisqualification = 5;





            }
            else if (TypeOfGame == "בינוני")
            {
                //windowBackground.ImageSource = new BitmapImage(new Uri("Images/" + "b2.jpg", UriKind.Relative));
                backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\46\27\FinalProject\Images\‏‏‏‏justMoveGameScreen.png", UriKind.Relative));
                myFallingThings.maxSecondForThingsPair = 5;
                myFallingThings.scoreGameToAdd = 2;
                NumOfLevels = 3;
                maxDisqualification = 5;





            }
            else if (TypeOfGame == "קשה")
            {
                backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\46\27\FinalProject\Images\‏‏‏‏justMoveGameScreen.png", UriKind.Relative));
                //backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\פרויקטים מעודכנים\Images\‏‏‏‏justMoveScreen3.jpg"));
                myFallingThings.maxSecondForThingsPair = 5;
                myFallingThings.scoreGameToAdd = 3;
                NumOfLevels = 5;
                maxDisqualification = 5;




            }
        }
        //~18
        //=========================================================================$$
        //19

        private void storyboardAnimationComplete(object sender, EventArgs e)
        {
            //animation = true;
            this.fullBasketImg.Visibility = Visibility.Hidden; //נסתיר את הסל מלא תפוחים שהיה בזמן ריצת האנימציה
            this.playMusic.Stop();//נעצור את מנגינת ההצלחה
            updateImgSource();//  לאחר שסל נפגע עלינו לעדכן את 3 התמונות-צורה ,סל ומובמנט ומתנה-אם זה תור של מתנה
            myFallingThings.loadNewThingsImg = false;
            myFallingThings.loadNewShapesThings = true;
            myFallingThings.doAnimation = true;
            //this.myFallingThings.AdvanceFrame();
            myFallingThings.currentHitThing = "shape";
            



        }
        //~19



    //=========================================================================$$
    }


}

