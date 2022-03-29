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



    /*
     * מקרים שונים ללחיצות הכפתורים הצבעוניים וכפתור האינפורמישיין
         התחלה
    *****************
    מקרה 1-לוחץ על צבעוני-לא קורה כלום(מצב התחלה)
    מקרה 2-לוחץ על סטרט-המשחק מתחיל(מצב התחלה)


    לאחר ניצחון
    **********************
    מקרה 3-מעדכן בwinner ממשיך כרגיל  הניקוד לא מתאפס אבל מעלה מהירות(מצב ניצחון)
    מקרה 4-לוחץ על צבעוני -לא קורה כלום(מצב ניצחון)
    מקרה 5-לוחץ על כפתור אינפורמיישן שכרגע מוצג עליו "winner"-לא קורה כלום(מצב ניצחון)


    לאחר פסילה
    ***************
    מקרה 6-לוחץ על צבעוני-לא קורה כלום(מצב פסילה)
    מקרה 7-לוחץ על פליי אגיין-המשחק מתחיל מחדש(מצב פסילה)


    תור מחשב
    *************************
    מקרה 8-לוחץ על צבעוני-לא קורה כלום(מצב מחשב)
    מקרה 9-לוחץ על סקור-לא קורה כלום(מצב מחשב)


    תור שחקן
    **********************
    מקרה 10-לוחץ על אינפורמיישן כשמוצג לו כרגע הניקוד(כיוון שכשמוצג בסטרט,פליי אגיין ווינר אף פעם זה לא תור השחקן)-לא קורה כלום(מצב שחקן)
    מקרה 11-לוחץ על צבעוני הבא נכון-נותן ללחוץ. משמיע צליל נכון ואנימציה(מצב שחקן)
    מקרה 12-לוחץ על צבעוני האחרון בסדרה נכון.משמיע צליל נכון ואנימציה .מעלה ניקוד ומשמיע תסדרה הבאה(מצב שחקן)
    מקרה 13-לוחץ על צבעוני הבא לא נכון-נותן ללחוץ. משמיע צליל שגוי ואנימציה ומעדכן לפליי אגיין(מצב שחקן)  

        */




    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class SimontoricMainWindow
    {

        private readonly KinectSensorChooser sensorChooser1;

        static Random random = new Random();

        /// <summary>
        ///  create 2 variables: 1. A list which store series of colors the player has to remember and press on 
        ///                         the button with the specific color. 
        ///                      2. An index that runs in this list. 
        /// </summary>

        List<KeyValuePair<SolidColorBrush, String>> colorList = new List<KeyValuePair<SolidColorBrush, String>>();
        int i;
        int j = 0;
        Boolean isPlayerTurn = false;                      //תור  שחקן
        Boolean isSequenceCorrect = true;               //הרצף עד כה נכון/לא
        Boolean isGameStarted = false;                  //המשחק התחיל או לא  
        Boolean isDoingAnimation = false;                  //באמצע לעשות אנימציה או לא
        double SpeedRatio;
        int currentScore;
        public string TypeOfGame;
        private int MaxLengthOfSeries;
        private int maxDisqualification;
        private int currentDisqu=0;
        private int numOfLevels;
        private int currentLevel=0;


        //#############################################################################################################################
        //DoubleAnimation da = new DoubleAnimation();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public SimontoricMainWindow()
        {

            this.InitializeComponent();
             InitAcocordingTypeOfShape();
            this.sensorChooser1 = new KinectSensorChooser();
            this.sensorChooser1.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser1;
            this.sensorChooser1.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser1 };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

        }



        //11111111111111111111111111111111111111111111111111111111111
        //מקרה 2
        //מקרה 7
        private void InformationCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (informationCenterButton.Content.Equals("start!") || informationCenterButton.Content.Equals("play again!"))
            {
                //if (informationCenterButton.Opacity != 0)
                // {
                try
                {
                    StartGame();

                }
                catch (Exception ex)
                {

                    Console.Write("error!" + ex);
                }
                //}
            }
            //מקרה 5
            //מקרה 9
            //מקרה 10
            else if (informationCenterButton.Content.Equals("score: " + currentScore.ToString()) || informationCenterButton.Content.Equals("you win!"))
            {
                ;

            }
            else
            {
                ;
                // Do nothing
            }
        }
        // #############################//111111//#######################

        //22222222222222222222222222222222222222222222222222222222222222222222222222222222222222
        private void StartGame()
        {
            //int a = colorList.Count;
            //informationCenterButton.Content = a;

            colorList.Clear();
            setColorButton();

            //a = colorList.Count;
            //informationCenterButton.Content = a;
            isGameStarted = true;
            isPlayerTurn = false;
            informationCenterButton.Content = "score :" + colorList.Count;
            currentScore = colorList.Count;
            AddColorToList();
            isSequenceCorrect = true;
            i = 0;
            j = 0;
            InitAcocordingTypeOfShape();
            ContinueGame();

        }
        // #############################//22222//#######################
        private void ContinueGame()
        {
            //מטפל בשני מקרים: הכנסה של  צבעים חדשים  והשמעתם הסדרה  או השמעת סדרה בלבד
            if (!isPlayerTurn)
            {
                BlinkingSeries(); //מחשב מציג סדרה

            }


        }
        /// 3333333333333333333333333333333333333333333333333333333333333333333

        /// <summary>
        /// 
        /// </summary>
        private void AddColorToList()
        {
            random.GetHashCode();
            int randomNumber = random.Next(0, 4);  //from minValue until maxValue-1 -not includin 4
            switch (randomNumber)
            {
                case 0:
                    colorList.Add(new KeyValuePair<SolidColorBrush, String>(Brushes.Red, "red"));
                    break;

                case 1:
                    colorList.Add(new KeyValuePair<SolidColorBrush, String>(Brushes.Blue, "blue"));
                    break;

                case 2:
                    colorList.Add(new KeyValuePair<SolidColorBrush, String>(Brushes.Green, "green"));
                    break;

                case 3:
                    colorList.Add(new KeyValuePair<SolidColorBrush, String>(Brushes.Yellow, "yellow"));
                    break;

                default:
                    break;
            }

        }

        // #############################//333333//#######################


        //444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444


        private void BlinkingSeries()  //come from ContinueGame
        {

            if (!isDoingAnimation)
            {

                isDoingAnimation = true;
                //informationCenterButton.Content = "remember!";
                //SoundPlayer player1 = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\" + "startSeries" + ".wav");
                //player1.Load();
                //player1.Play();

                BeginStoryboard r_sb = this.FindResource("storyboardStartSeries") as BeginStoryboard;
                r_sb.Storyboard.SpeedRatio = SpeedRatio;
                r_sb.Storyboard.Begin();



            }

        }


        // #############################//444444//#######################
        void blinksButton(string color)    //come from runButton
        {


            if (!isSequenceCorrect && !isPlayerTurn)
            {
                SoundPlayer player1 = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\" + "worng" + ".wav");
                player1.Load();
                player1.Play();
                

                  BeginStoryboard r_sb = this.FindResource("storyboard" + color + "Button") as BeginStoryboard;
                r_sb.Storyboard.SpeedRatio = SpeedRatio;

                r_sb.Storyboard.Begin();
            }
            else //צליל כפתור 
            {
                BeginStoryboard r = this.FindResource("storyboard" + color + "Button") as BeginStoryboard;

                SoundPlayer player = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\" + color + ".wav");
                player.Load();
                player.Play();


                if (isPlayerTurn)//לחיצת שחקן בתורו
                {
                    isDoingAnimation = true;
                    BeginStoryboard r_sb = this.FindResource("storyboard" + color + "Button" + "Click") as BeginStoryboard;
                    r_sb.Storyboard.SpeedRatio = SpeedRatio;

                    r_sb.Storyboard.Begin();
                    isDoingAnimation = false;
                }
                else//אנימצית הצגת הסדרה
                {

                    BeginStoryboard r_sb = this.FindResource("storyboard" + color + "Button") as BeginStoryboard;
                    r_sb.Storyboard.SpeedRatio = SpeedRatio;

                    if (informationCenterButton.Content.Equals("you win!"))
                    {
                        informationCenterButton.Content = "score :" + currentScore;

                    }
                    r_sb.Storyboard.Begin();

                }
            }


        }

        //55555555555555555555555555555555555555555555555555555555555555555555555555555555555555555
        private void runButton(int i)  //come from BlinkingSeries
        {

            if (i < colorList.Count)
            {

                var str = colorList[i].Value;

                blinksButton(str);

            }
            else
            {
                isPlayerTurn = true;

            }

        }
        // #############################//555555//#######################

        //6666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666
        private void storyboardAnimationComplete(object sender, object e)  //after 
        {

            isDoingAnimation = false;
            setColorButton();


            if (!isDoingAnimation)
            {

                if (!isPlayerTurn)
                {

                    runButton(i);
                    i++;


                }
            }


        }



        // #############################//66666//#######################

        //77777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777
        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void KinectTileButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (KinectTileButton)e.OriginalSource;
            var NameOfButtonClicked = button.Name;


            //מקרה 1
            //מקרה 4
            //מקרה 6
            if (informationCenterButton.Content.Equals("play again!") || informationCenterButton.Content.Equals("start!") || informationCenterButton.Content.Equals("you win!"))
            {
                ;
            }

            //מקרה 8
            else if (!isPlayerTurn)
            {
                ;

            }





            //מקרה 13
            else if (isPlayerTurn)    //תור שחקן 
            {

                if (button.Name == colorList[j].Value)
                {
                    blinksButton(NameOfButtonClicked);//מראה צבע כפתור ומשמיע צליל כפתור
                    j++;

                    if (isSequenceCorrect)
                    {
                        //מקרה 11
                        if (j < colorList.Count)  //הגיע לסוף הרצף בהצלחה   
                        {
                            ;
                        }
                        //מקרה 12
                        else if (j >= colorList.Count)
                        {
                            j = 0;
                            i = 0;
                            isPlayerTurn = false;
                            informationCenterButton.Content = "score :" + colorList.Count;
                            currentScore = colorList.Count;
                            // מקרה 3
                            if (colorList.Count % MaxLengthOfSeries == 0 && colorList.Count != 0)
                            {
                                informationCenterButton.Content = "you win!";
                                

                                SoundPlayer player1 = new SoundPlayer($@"{new FileInfo(Environment.CurrentDirectory).Directory.FullName}\Music\" + "winner" + ".wav");
                                player1.Load();
                                player1.Play();

                                currentLevel++;
                                if (currentLevel > numOfLevels)
                                {
                                    //$$load winner window
                                    this.sensorChooser1.Stop();
                                    SimontoricWinOrLose w = new SimontoricWinOrLose();
                                    w.backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\simontoricWinnerscreen.jpg"));
                                    w.Show();
                                    Close();
                                }

                                SpeedRatio = SpeedRatio + 0.7;


                              
                                //$$
                            }
                            AddColorToList();
                            ContinueGame();//isSequenceCorrect = true; עדיין נשאר
                        }
                    }

                }

                //מקרה 13
                else if (button.Name != colorList[j].Value)
                {
                    isSequenceCorrect = false;
                    isPlayerTurn = false;
                    blinksButton(NameOfButtonClicked);
                    //isDoingAnimation = false;
                    informationCenterButton.Content = "play again!";
                    currentDisqu++;
                    if(currentDisqu>maxDisqualification)
                    {
                        //$$load game over window
                        this.sensorChooser1.Stop();
                        SimontoricWinOrLose w = new SimontoricWinOrLose();
                        w.backgroundWindow.ImageSource = new BitmapImage(new Uri(@"C:\Users\nava\source\repos\כל גרסאות הפרויקטים-במחשב\גיבוי אחרון\Images\gameOverWinnerscreen.jpg"));
                        w.Show();
                        Close();
                    }
                }




            }
        }

        // #############################//777777//#######################
        private void setColorButton()
        {
            red.Background = Brushes.Red;
            blue.Background = Brushes.Blue;
            green.Background = Brushes.Green;
            yellow.Background = Brushes.Yellow;
        }

        ////////////////////////888888888888888///////////////////////

        //private void BackButtonClick(object sender, RoutedEventArgs e)
        //{
        //    this.sensorChooser1.Stop();
        //    BasicGameInstructions w = new BasicGameInstructions();
        //    w.Show();
        //    Close();
        //}
        ////999999999999999999999999999999///
        private void InitAcocordingTypeOfShape()
        {
            if (TypeOfGame == "קל")
            {
                MaxLengthOfSeries = 3;
                numOfLevels = 1;
                SpeedRatio = 1;
                maxDisqualification = 3;

            }
            else if (TypeOfGame == "בינוני")
            {
                SpeedRatio = 2;
                MaxLengthOfSeries = 4;
                numOfLevels = 4;
                maxDisqualification = 2;


            }
            else if (TypeOfGame == "קשה")
            {
                SpeedRatio = 3;
                MaxLengthOfSeries = 5;
                numOfLevels = 5;
                maxDisqualification = 1;


            }
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
            this.sensorChooser1.Stop();
        }

        private void backClick(object sender, RoutedEventArgs e)
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //כדי שהמצלמה תעבוד במסך החדש שנפתח נעצור את הסנסור הנוכחי של המצלמה
            this.sensorChooser1.Stop();
            SimontoricMenu w = new SimontoricMenu();
            w.Show();
            Close();
        }


        //####################################END OF KINECT_FUNCTION  ################################################

    }
}




//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888

// #############################//888888//#######################

//999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999

// #############################//99999//#######################

//101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010

// #############################//101010//#######################