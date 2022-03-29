//------------------------------------------------------------------------------
// <copyright file="FallingThings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// This module contains code to do display falling shapes, and do
// hit testing against a set of segments provided by the Kinect NUI, and
// have shapes react accordingly.


//=========================================================================
namespace ShapeGame
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using ShapeGame.Utils;


//=========================================================================
// FallingThings is the main class to draw and maintain positions of falling shapes.  It also does hit testing
// and appropriate bouncing.
    public class FallingThings
    {
     //=========================================================================


        private const double BaseGravity = 0.017;
        private const double BaseAirFriction = 0.994;
        private TimeSpan myTime = DateTime.Now.Subtract(DateTime.Now);
        private Boolean ifPress = false; //אם כבר פגע בצורה למעלה או לא
        private readonly Dictionary<PolyType, PolyDef> polyDefs = new Dictionary<PolyType, PolyDef>
            {
                { PolyType.Triangle, new PolyDef { Sides = 3, Skip = 1 } },
                { PolyType.Star, new PolyDef { Sides = 5, Skip = 2 } },
                { PolyType.Pentagon, new PolyDef { Sides = 5, Skip = 1 } },
                { PolyType.Square, new PolyDef { Sides = 4, Skip = 1 } },
                { PolyType.Hex, new PolyDef { Sides = 6, Skip = 1 } },
                { PolyType.Star7, new PolyDef { Sides = 7, Skip = 3 } },
                { PolyType.Circle, new PolyDef { Sides = 1, Skip = 1 } },
                { PolyType.Bubble, new PolyDef { Sides = 0, Skip = 1 } }
            };

        private readonly List<Thing> things = new List<Thing>();
        private readonly Random rnd = new Random();
        private readonly int maxThings;
        private readonly int intraFrames = 1;
        //private readonly Dictionary<int, int> scores = new Dictionary<int, int>();
        private const double DissolveTime = 0.4;
        private Rect sceneRect;
        private double targetFrameRate = 60;
        private double dropRate = 2.0;
        private double shapeSize = 1.0;
        private double baseShapeSize = 20;
        private GameMode gameMode = GameMode.Off;
        private double gravity = BaseGravity;
        private double gravityFactor = 1.0;
        private double airFriction = BaseAirFriction;
        private int frameCount;
        private bool doRandomColors = true;
        private double expandingRate = 1.0;
        private System.Windows.Media.Color baseColor = System.Windows.Media.Color.FromRgb(0, 0, 0);
        private PolyType polyTypes = PolyType.All;
        public DateTime gameStartTime;
        public DateTime endGameTime;
        public Boolean isEndGame = false;
        public string endGameMode;
        public Boolean doAnimation=false;
        public Boolean toUpdateScore = false;
        public Boolean nextItration = false;
        public enum ThingState
        {
            Falling = 0, //$$-נופל
            Bouncing = 1,//$$-מתפוצץ
            Dissolving = 2,//$$-מעומעם
            Remove = 3//$$-מוסר
        }
        //score
        public int scoreGameToAdd; //לפי רמת הקושי יש ניקוד קבע שמוסיפים לכל פגיעה
        private int numOfScore = 0;//ניקוד להוספה לניקוד הקיים
        public int fine = 0; //$$לתת קנס במצב שהזמן המוקצב בין תפיסת צורה להכנסתה עבר 
        public int maxDisqu = 5;
        public int numOfDisqu = 0;
        public int sumOfScore = 0;

        private KeyValuePair<string, int> typeAndIdPlayerOfCurrentHitThing = new KeyValuePair<string, int>(); //$$לדעת מהי העצם שפגע ב"סינג"
        private string currentTypeOfHit;//$$סוג הפגיעה הנוכחית ראש/יד/רגל
        
        private Tuple<Point, Point, KeyValuePair<List<double>, List<double>>, KeyValuePair<string, string>> _wantedGesture; //הפגיעות הרצויות וזוג תפוח-סל הרצוי
        private int currentIndexshapeBasketList = 0; //אינדקס שרץ על אוסף התפוח-סל
        private bool isTimeOut = false;//לדעת אם הזמן עבר
        private int NumOfItrations; //"וזהו בעצם מספר האיטרציות על אוסף התפוח-סל מאותחל בתחילת המשחק לפי בחירת רמת קושי ב"נמאופלבל
        private int currentItration = 0;//מספר איטרציה נוכחית
        private string text; //טקסט של הטיימר שמוצג
        private Label labelOfSCores;//תווית בשביל הניקוד שמוצג
        //private KeyValuePair<string, bool> currentTypeOfShape;//לדעת איזה פגיעה  אנו מחפשים של תפוח או של סל
        
        public bool loadNewThingsImg = false; //יהיה ערך אמת לאחר פגיעה בסל(וקודם בתפוח) ואם יש גם מתנה אז גם לאחר פגיעה בה הזמן המוקצב לכך
        public int maxSecondForThingsPair; //מאותחל בתחילת המשחק לפי בחירת רמת קושי וזהו בעצם מה שקובע לאחר כמה זמן הטיימר מתאפס ויורד ניקוד
        public bool addGift = false;//צריך להוסיף סינג מסוג מתנה
        public string typeOfGift;//$$סוג המתנה שצריך להוסיף
        public bool isGiftHit = false;//האם היתה פגיעה במתנה
        public string currentHitThing;//משתנה המחזיק את סוג הסינג הנוכחי בו אנו מצפים שתהיה פגיעה
        public Boolean removeThingImg;
        public Boolean loadNewShapesThings = false;
        

        //$$רשימה שמכילה  אוסף המורכב מרשימות של:
        //מיקום תפוח,מיקום סל,מרגין של תפוחתמרגין של סל,באיזה יד על הבן אדם לתפוס את התפוחתבאיזה יד על הבן אדם לתפוס את הסל
        List<Tuple<Point, Point, KeyValuePair<List<double>, List<double>>, KeyValuePair<string, string>>> _shapeBasketList = new List<Tuple<Point, Point, KeyValuePair<List<double>, List<double>>, KeyValuePair<string, string>>>()
        {
          new Tuple<Point, Point, KeyValuePair<List<double>, List<double>>,KeyValuePair<string, string>>(/*posShapeLeftUp*/new System.Windows.Point(480, 400), /*posBasketRightDown*/new System.Windows.Point(780, 725),new KeyValuePair<List<double>, List<double>>( new List<double>() { 130, 100, 730, 1450 }, new List<double>() { 400, 250,300, 1050 }), /*"D1 right"*/new KeyValuePair<string, string>("HandRight","HandRight")),
          new Tuple<Point, Point, KeyValuePair<List<double>, List<double>>,KeyValuePair<string, string>>(/*posShapeRightUp*/new System.Windows.Point(880, 400), /*posBasketLeftDown*/new System.Windows.Point(530, 710),new KeyValuePair<List<double>, List<double>>( new List<double>() {530, 100, 340, 1450 }, new List<double>() { 100, 250,600, 1050 }),/*"D1 left"*/new KeyValuePair<string, string>("HandLeft","HandLeft")),
          new Tuple<Point, Point, KeyValuePair<List<double>, List<double>>,KeyValuePair<string, string>>(/*posShapeRightUp*/new System.Windows.Point(880, 400), /*posBasketLeftDown*/new System.Windows.Point(530, 710),new KeyValuePair<List<double>, List<double>>( new List<double>() {  530, 100, 340, 1450  }, new List<double>() { 100, 250,600, 1050}), /*"D2 right"*/new KeyValuePair<string, string>("HandRight","HandRight")),
          new Tuple<Point, Point, KeyValuePair<List<double>, List<double>>,KeyValuePair<string, string>>(/*posShapeLeftUp*/new System.Windows.Point(480, 400), /*posBasketRightDown*/new System.Windows.Point(780, 725),new KeyValuePair<List<double>, List<double>>( new List<double>() { 130, 100, 730, 1450 }, new List<double>() { 400, 250,300, 1050  }),/*"D2 left"*/new KeyValuePair<string, string>("HandLeft","HandLeft")),


        };



        public Tuple<Point, Point, KeyValuePair<List<double>, List<double>>, KeyValuePair<string, string>> GetCurrentThings(int index) { return _shapeBasketList[index]; }
        //public KeyValuePair<string, bool> GetCurrentTypeOfShape() { return currentTypeOfShape; }
        public void ClearThingsList() { this.things.Clear(); }
        public void SetNumOfItrations(int num) { NumOfItrations = num; }
        public bool GetIsTimeOut() { return isTimeOut; }
        public bool GetIfloadNewMovmentImg() { return loadNewThingsImg; }
        public int GetCurrentItration() { return currentItration; }

        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ 


        //=========================================================================
        //1-ctor


        public FallingThings(int maxThings, double framerate, int intraFrames)
        {
            this.maxThings = maxThings;
            this.intraFrames = intraFrames;
            this.targetFrameRate = framerate * intraFrames;
            this.SetGravity(this.gravityFactor);
            this.sceneRect.X = this.sceneRect.Y = 0;
            this.sceneRect.Width = this.sceneRect.Height = 100;
            this.shapeSize = this.sceneRect.Height * this.baseShapeSize / 1000.0;
            this.expandingRate = Math.Exp(Math.Log(6.0) / (this.targetFrameRate * DissolveTime));

            _wantedGesture = _shapeBasketList[0];
            var w = _wantedGesture.Item3.Key[0].ToString();
            var w3 = _wantedGesture.Item3.Value[0].ToString();
            maxDisqu = maxDisqu * NumOfItrations;

        }
        //~1


        //=========================================================================
        //2

        public static Label MakeSimpleLabel(string text, Rect bounds, System.Windows.Media.Brush brush)
        {
            Label label = new Label { Content = text };
            if (bounds.Width != 0)
            {
                label.SetValue(Canvas.LeftProperty, bounds.Left);
                label.SetValue(Canvas.TopProperty, bounds.Top);
                label.Width = bounds.Width;
                label.Height = bounds.Height;
            }

            label.Foreground = brush;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontWeight = FontWeight.FromOpenTypeWeight(600);
            label.FontStyle = FontStyles.Normal;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            return label;
        }
        //~2

        //=========================================================================
        //3
        public void SetFramerate(double actualFramerate)
        {
            this.targetFrameRate = actualFramerate * this.intraFrames;
            this.expandingRate = Math.Exp(Math.Log(6.0) / (this.targetFrameRate * DissolveTime));
            if (this.gravityFactor != 0)
            {
                this.SetGravity(this.gravityFactor);
            }
        }

        //~3
        //=========================================================================
        //4
        public void SetBoundaries(Rect r)
        {
            this.sceneRect = r;
            this.shapeSize = r.Height * this.baseShapeSize / 1000.0;
        }
        //~4
        //=========================================================================
        //5
        public void SetDropRate(double f)
        {
            this.dropRate = f;
        }
        //~5
        //=========================================================================

        //6
        public void SetSize(double f)
        {
            this.baseShapeSize = f;
            this.shapeSize = this.sceneRect.Height * this.baseShapeSize / 1000.0;
        }
        //~6
        //=========================================================================
        //7
        public void SetShapesColor(System.Windows.Media.Color color, bool doRandom)
        {
            this.doRandomColors = doRandom;
            this.baseColor = color;
        }
        //~7
        //=========================================================================
        //8
        public void Reset()
        {
            for (int i = 0; i < this.things.Count; i++)
            {
                Thing thing = this.things[i];
                if ((thing.State == ThingState.Bouncing) || (thing.State == ThingState.Falling))//bbb
                {
                    thing.State = ThingState.Dissolving;//bbb
                    thing.Dissolve = 0;
                    this.things[i] = thing;
                }
            }

            this.gameStartTime = DateTime.Now;
            //this.scores.Clear();
        }
        //~8
        //=========================================================================
        //9
        public void SetGameMode(GameMode mode)
        {
            this.gameMode = mode;
            this.gameStartTime = DateTime.Now;
            //this.scores.Clear();
        }
        //~9
        //=========================================================================

        //10
        public void SetGravity(double f)
        {
            this.gravityFactor = f;
            this.gravity = f * BaseGravity / this.targetFrameRate / Math.Sqrt(this.targetFrameRate) / Math.Sqrt(this.intraFrames);
            this.airFriction = f == 0 ? 0.997 : Math.Exp(Math.Log(1.0 - ((1.0 - BaseAirFriction) / f)) / this.intraFrames);

            if (f == 0)
            {
                // Stop all movement as well!
                for (int i = 0; i < this.things.Count; i++)
                {
                    Thing thing = this.things[i];
                    thing.XVelocity = thing.YVelocity = 0;
                    this.things[i] = thing;
                }
            }
        }
        //~10

        //=========================================================================

        //11
        public void SetPolies(PolyType polies)
        {
            this.polyTypes = polies;
        }
        //~11

        //=========================================================================
        //12
        public HitType LookForHits(Dictionary<Bone, BoneData> segments, int playerId)
        {

            //if (myTime.Seconds > maxSecondForThingsPair)
            //{
            //    loadNewThingsImg = true;
            //    addGift = false;
            //    isGiftHit = true;
            //    // toUpdateScore = true;
            //}

            //if (currentTypeOfShape.Key == "shape")
            //    currentTypeOfShape = new KeyValuePair<string, bool>("basket", false);
            //else if (currentTypeOfShape.Key == "basket")
            //    currentTypeOfShape = new KeyValuePair<string, bool>("shape", false);

            DateTime cur = DateTime.Now;

            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//?
            //מאתחלים את ערך החזרת הפונקציה(סוג הפגיעה-פןפ,שום פגיעה,סקוויז וכו)בשום פגיעה
            HitType allHits = HitType.None;   //bbb

            // Zero out score if necessary
            //if (!this.scores.ContainsKey(playerId))
            //{
            //    //ניקוד מאותחל ב-0
            //    this.scores.Add(playerId, 0);
            //    this.gameStartTime = DateTime.Now;

            //}
            //this.sumOfScore = 0;


            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//?
            //עוברים במקביל על כל מקטעי השלד ועל הצורות ובודקים אם היתה פגיעה
            foreach (var pair in segments)
            {
                for (int i = 0; i < this.things.Count; i++)
                {


                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                    //מאתחל משתנה שישמור את סוג הפגיעה לשום פגיעה

                    HitType hit = HitType.None; //bbb
                    Thing thing = this.things[i];



                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //בודק פגיעה בסינג בתנאי שהסינג היה במצב "פגיע" 0
                    if (thing.CanHit)
                    {
                        if (isTimeOut && isGiftHit)
                        {

                            allHits |= HitType.Popped; //bbb

                            return allHits; //bbb



                        }
                        switch (thing.State)
                        {

                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//?
                            //מצבים בהם ישנה אפשרות לפגוע
                            case ThingState.Bouncing://bbb
                            case ThingState.Falling://bbb
                                {
                                    var hitCenter = new System.Windows.Point(0, 0); //bbb
                                    double lineHitLocation = 0; //bbb
                                    Segment seg = pair.Value.GetEstimatedSegment(cur);

                                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                    //אם הפונקציה "איט" החזריה "טרו" סימן שהיתה פגיעה

                                    if (thing.Hit(seg, ref hitCenter, ref lineHitLocation)) //bbb
                                    {

                                        //$$$$$$$$$$$$$$$$$$$$$$
                                        //@@בשביל לצבוע את הסגמנט שפגע ב"סינג

                                        var w1 = pair.Key.Joint1;
                                        var w2 = pair.Key.Joint2;
                                        //@@
                                        JointType typeOfHit = pair.Key.Joint1;
                                        currentTypeOfHit = typeOfHit.ToString();
                                        typeAndIdPlayerOfCurrentHitThing = new KeyValuePair<string, int>(currentTypeOfHit, playerId);

                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//

                                        double fMs = 1000;

                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                                        //כאשר זמן פגיעה בצורה שונה מהערך שאתחלו אותה 
                                        if (thing.TimeLastHit != DateTime.MinValue)//bbb
                                        {

                                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                                            //fMs-מחשבים את מספר השניות שעברו מזמן שהצורה אותחלה והוצגה בעצם על המסך עד לזמן שנפגעה
                                            //ומחשבים את ממוצע משך זמן הפגיעה
                                            //cur-זהו הזמן האמיתי של הפגיעה
                                            fMs = cur.Subtract(thing.TimeLastHit).TotalMilliseconds; //bbb
                                            thing.AvgTimeBetweenHits = (thing.AvgTimeBetweenHits * 0.8) + (0.2 * fMs); //bbb
                                        }
                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                        //לשם בדיקה-מתי הזמן של הסינג נשאר אותו דבר
                                        else if (thing.TimeLastHit != DateTime.MinValue)
                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                        {
                                            ;
                                        }

                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                                        //נעדכן את "סינג" בזמן האמיתי של הפגיעה
                                        thing.TimeLastHit = cur; //bbb

                                        // Bounce off head and hands

                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                                        //פגיעה בעצמות ראש/ידיים/רגלים
                                        if (seg.IsCircle())
                                        {

                                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                            //שמתי בהערה כדי שלא תזוז הצורה בתנועה לא נכונה
                                            // Bounce off of hand/head/foot
                                            //thing.BounceOff(
                                            //    hitCenter.X,
                                            //    hitCenter.Y,
                                            //    seg.Radius,
                                            //    pair.Value.XVelocity / this.targetFrameRate,
                                            //    pair.Value.YVelocity / this.targetFrameRate);
                                            if (fMs > 100.0)
                                            {
                                                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                                                //במצב שהיתה פגיעה חלשה--נגיעה קלה עם היד
                                                hit |= HitType.Hand; //bbb
                                            }
                                        }
                                        else
                                        {
                                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                            //שמתי בהערה כדי שלא תזוז הצורה בתנועה לא נכונה

                                            // Bounce off line segment
                                            //double velocityX = (pair.Value.XVelocity * (1.0 - lineHitLocation)) + (pair.Value.XVelocity2 * lineHitLocation);
                                            //double velocityY = (pair.Value.YVelocity * (1.0 - lineHitLocation)) + (pair.Value.YVelocity2 * lineHitLocation);

                                            //thing.BounceOff(
                                            //    hitCenter.X,
                                            //    hitCenter.Y,
                                            //    seg.Radius,
                                            //    velocityX / this.targetFrameRate,
                                            //    velocityY / this.targetFrameRate);

                                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                            //במצב של פגיעה בזרוע
                                            if (fMs > 100.0)
                                            {
                                                hit |= HitType.Arm; //bbb
                                            }
                                        }

                                        if (this.gameMode == GameMode.TwoPlayer)
                                        {
                                            if (thing.State == ThingState.Falling)//bbb
                                            {
                                                thing.State = ThingState.Bouncing;//bbb
                                                thing.TouchedBy = playerId;
                                                thing.Hotness = 1;
                                                thing.FlashCount = 0;
                                            }
                                            else if (thing.State == ThingState.Bouncing)//bbb
                                            {
                                                if (thing.TouchedBy != playerId)
                                                {
                                                    if (seg.IsCircle())
                                                    {
                                                        thing.TouchedBy = playerId;
                                                        thing.Hotness = Math.Min(thing.Hotness + 1, 4);
                                                    }
                                                    else
                                                    {
                                                        hit |= HitType.Popped; //bbb
                                                    }
                                                }
                                            }
                                        }
                                        else if (this.gameMode == GameMode.Solo)
                                        {

                                            //ניתן להוסיף חלוקה בין סוגי הפגיעות :פגיעה מסוג ראש/רגל/יד וכו
                                            
                                            
                                            
                                            //switch (typeOfHit)
                                            //{
                                            //    case JointType.AnkleLeft:
                                            //        //Console.WriteLine("The color is red");
                                            //        //numOfScore = 21;
                                            //        break;
                                            //    case JointType.AnkleRight:
                                            //        //Console.WriteLine("The color is green");
                                            //       // numOfScore = 2;
                                            //        break;
                                            //    case JointType.ElbowLeft:
                                            //        //Console.WriteLine("The color is blue");
                                            //        //numOfScore = 3;
                                            //        break;
                                            //    case JointType.ElbowRight:
                                            //        //Console.WriteLine("The color is red");
                                            //        //numOfScore = 4;
                                            //        break;
                                            //    case JointType.FootLeft:
                                            //        //Console.WriteLine("The color is green");
                                            //        //numOfScore = 5;
                                            //        break;
                                            //    case JointType.FootRight:
                                            //        //Console.WriteLine("The color is blue");
                                            //        //numOfScore = 6;
                                            //        break;
                                            //    case JointType.HandLeft:
                                            //        //Console.WriteLine("The color is red");
                                            //        //numOfScore = 7;
                                            //        break;
                                            //    case JointType.HandRight:
                                            //        //Console.WriteLine("The color is green");
                                            //        //numOfScore = 8;
                                            //        break;
                                            //    case JointType.Head:
                                            //        //Console.WriteLine("The color is blue");
                                            //        //numOfScore = 9;
                                            //        break;
                                            //    case JointType.HipCenter:
                                            //        //Console.WriteLine("The color is red");
                                            //        //numOfScore = 10;
                                            //        break;
                                            //    case JointType.HipLeft:
                                            //        //Console.WriteLine("The color is green");
                                            //        //numOfScore = 11;
                                            //        break;
                                            //    case JointType.HipRight:
                                            //        //Console.WriteLine("The color is blue");
                                            //        //numOfScore = 12;
                                            //        break;
                                            //    case JointType.KneeLeft:
                                            //        //Console.WriteLine("The color is red");
                                            //        //numOfScore = 13;
                                            //        break;
                                            //    case JointType.KneeRight:
                                            //        //Console.WriteLine("The color is green");
                                            //       //numOfScore = 14;
                                            //        break;
                                            //    case JointType.ShoulderCenter:
                                            //        //Console.WriteLine("The color is blue");
                                            //        //numOfScore = 15;
                                            //        break;
                                            //    case JointType.ShoulderLeft:
                                            //        //Console.WriteLine("The color is red");
                                            //        numOfScore = 16;
                                            //        break;
                                            //    case JointType.ShoulderRight:
                                            //        //Console.WriteLine("The color is red");
                                            //        numOfScore = 17;
                                            //        break;
                                            //    case JointType.Spine:
                                            //        //Console.WriteLine("The color is green");
                                            //        //numOfScore = 18;
                                            //        break;
                                            //    case JointType.WristLeft:
                                            //        //Console.WriteLine("The color is blue");
                                            //        //numOfScore = 19;
                                            //        break;
                                            //    case JointType.WristRight:
                                            //        //Console.WriteLine("The color is red");
                                            //        //numOfScore = 20;
                                            //        break;
                                            //    default:
                                            //        //Console.WriteLine("The color is unknown.");
                                            //        // numOfScore = 200;
                                            //        break;
                                            //}


                                          
                                            if (seg.IsCircle())
                                            {

                                                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                                //היתה פגיעה והצורה היתה במצב נפילה(מצב בו היא מאותחלת ברגע שנכנסת למערך ה"סינגים" ןמןצגת על המסך) תעדכן אותה להיות במצב מתפוצץ
                                                if (thing.State == ThingState.Falling)
                                                {
                                                    thing.State = ThingState.Bouncing;
                                                    thing.TouchedBy = playerId;
                                                    thing.Hotness = 1;
                                                    thing.FlashCount = 0;
                                                }
                                                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                                //כאשר ה"סינג" כבר במצב מתמוסס-נגדיר את סוג הפגיעה להיות "פופ" ונרצה להציג ניקוד על הפגיעה(במקום הצורה שנפגעה) ולעדכן ניקוד כללי
                                                else if ((thing.State == ThingState.Bouncing) && (fMs > 100.0))//bbb
                                                {

                                                    hit |= HitType.Popped; //bbb


                                                }
                                            }
                                        }

                                        this.things[i] = thing;
                                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                                        //אם ממוצע משך זמן הפגיעה קטן מ8 זה אומר שהפגיעה היתה מסוג "פופ" או "סקייז
                                        if (thing.AvgTimeBetweenHits < 8) //bbb
                                        {
                                            hit |= HitType.Popped | HitType.Squeezed; //bbb


                                        }
                                    }
                                }

                                break;
                        }
                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                        //היתה פגיעה והיא והיא מסוג "פופ" תעדכןאת מצב ה"סינג" להיות מעומעם ותעדכן כמה דברים שצריך בכמה דברים שדרושים כדי להיכנס למצב הזה
                        //הוספנו אפשרות ל"סקייז"
                        if ((hit & HitType.Popped) != 0 || (hit & HitType.Squeezed) != 0)//*//* /*|| (hit & HitType.Arm)!=0 || (hit & HitType.Hand)!=0 )  //$$
                        {
                            if (thing.TypeOfShape == "shape")
                            {
                                if (currentTypeOfHit == _wantedGesture.Item4.Key)
                                {
                                    currentHitThing = "shape"; //סוג הצורה שבה אנו מצפים שיפגעו
                                    removeThingImg = true;

                                    //currentTypeOfShape = new KeyValuePair<string, bool>("shape", true);

                                    this.gameStartTime = DateTime.Now;
                                    ifPress = true;
                                    thing.State = ThingState.Dissolving;
                                    thing.Dissolve = 0;
                                    thing.XVelocity = thing.YVelocity = 0;
                                    thing.SpinRate = (thing.SpinRate * 6) + 0.2;
                                    this.things[i] = thing;

                                    numOfScore = 5;

                                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                    //כאשר ה"סינג" כבר במצב מעומעם נרצה להציג ניקוד על הפגיעה(במקום הצורה שהתפוצצה) ולעדכן ניקוד כללי אך זאת בתנאי שמצב המשחק אינו כבוי כלומר יש 1/2 שחקנים במשחק 

                                    if (this.gameMode != GameMode.Off)
                                    {
                                        thing.TouchedBy = playerId;

                                        var b = numOfScore;
                                        sumOfScore = sumOfScore + numOfScore;
                                        //this.AddToScore(
                                        //                       thing.TouchedBy,
                                        //                       numOfScore,
                                        //                       thing.Center, "joint" + " " + currentTypeOfHit);
                                    }




                                }

                            }
                            else if (thing.TypeOfShape == "basket")
                            {
                                if (currentTypeOfHit == _wantedGesture.Item4.Value)
                                {
                                    currentHitThing = "basket"; //סוג הצורה שבה אנו מצפים שיפגעו
                                    removeThingImg = true;

                                    // currentTypeOfShape = new KeyValuePair<string, bool>("basket", true);
                                    //text = "";
                                    thing.State = ThingState.Dissolving;
                                    thing.Dissolve = 0;
                                    thing.XVelocity = thing.YVelocity = 0;
                                    thing.SpinRate = (thing.SpinRate * 6) + 0.2;
                                    this.things[i] = thing;


                                    numOfScore = 3;

                                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                    //כאשר ה"סינג" כבר במצב מעומעם נרצה להציג ניקוד על הפגיעה(במקום הצורה שהתפוצצה) ולעדכן ניקוד כללי אך זאת בתנאי שמצב המשחק אינו כבוי כלומר יש 1/2 שחקנים במשחק 

                                    if (this.gameMode != GameMode.Off)
                                    {
                                        thing.TouchedBy = playerId;

                                        var b = numOfScore;
                                        //this.AddToScore(
                                        //                       thing.TouchedBy,
                                        //                       numOfScore,
                                        //                       thing.Center, "joint" + " " + currentTypeOfHit);
                                        sumOfScore = sumOfScore + numOfScore;

                                    }




                                    if (!addGift)
                                    {
                                        loadNewThingsImg = true;
                                        ifPress = false;
                                    }



                                }


                            }

                            else if (thing.TypeOfShape == "gift" ) //במקרה שנוספה מתנה לזוג התפוח-סל
                            {
                                currentHitThing = "gift"; //סוג הצורה שבה אנו מצפים שיפגעו
                                removeThingImg = true;

                                var x2 = typeOfGift;
                                var x3 = currentTypeOfHit;
                                //if(addGift)
                                //{


                                    if (myTime.Seconds < maxSecondForThingsPair)
                                    {
                                    
                                        if (typeOfGift == "SR")
                                            ;
                                        else if ((typeOfGift == "coin" && currentTypeOfHit == "Head") || (typeOfGift == "normalGift"))
                                        {

                                            isGiftHit = true;//מעדכן שהיתה פגיעה במתנה
                                            loadNewThingsImg = true;

                                        // currentTypeOfShape = new KeyValuePair<string, bool>("basket", true);
                                        ifPress = false;
                                            //text = "";
                                            thing.State = ThingState.Dissolving;
                                            thing.Dissolve = 0;
                                            thing.XVelocity = thing.YVelocity = 0;
                                            thing.SpinRate = (thing.SpinRate * 6) + 0.2;
                                            this.things[i] = thing;


                                            numOfScore = typeOfGift == "coin" ? 2 : 1;

                                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                                            //כאשר ה"סינג" כבר במצב מעומעם נרצה להציג ניקוד על הפגיעה(במקום הצורה שהתפוצצה) ולעדכן ניקוד כללי אך זאת בתנאי שמצב המשחק אינו כבוי כלומר יש 1/2 שחקנים במשחק 

                                            if (this.gameMode != GameMode.Off)
                                            {
                                                thing.TouchedBy = playerId;

                                                var b = numOfScore;
                                            sumOfScore = sumOfScore + numOfScore;

                                            //this.AddToScore(
                                            //                       thing.TouchedBy,
                                            //                       numOfScore,
                                            //                       thing.Center, "joint" + " " + currentTypeOfHit);
                                        }





                                    }
                                    //}
                                   
                                }
                                




                            }




                            allHits |= hit; //bbb
                        }
                    }
                }
            }
            return allHits; //bbb
        }
        //~12

        //=========================================================================

        //13
        public void AdvanceFrame()
        {
            //if (isGiftHit)
            //{
            //    this.things.Clear();
            //    loadNewThingsImg = true;
            //}

            // Move all things by one step, accounting for gravity
            for (int thingIndex = 0; thingIndex < this.things.Count; thingIndex++)
            {
                Thing thing = this.things[thingIndex];
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                //כך מונעים תזוזה של צורה-הן רק הסתבובבו אך לא יזוזו ממש
                //thing.Center.Offset(thing.XVelocity, thing.YVelocity);//$$
                thing.YVelocity += this.gravity * this.sceneRect.Height;
                thing.YVelocity *= this.airFriction;
                thing.XVelocity *= this.airFriction;
                thing.Theta += thing.SpinRate;

                // bounce off walls
                if ((thing.Center.X - thing.Size < 0) || (thing.Center.X + thing.Size > this.sceneRect.Width))
                {
                    thing.XVelocity = -thing.XVelocity;
                    thing.Center.X += thing.XVelocity;
                }

                // Then get rid of one if any that fall off the bottom
                if (thing.Center.Y - thing.Size > this.sceneRect.Bottom)
                {
                    thing.State = ThingState.Remove;//bbb

                }
                //if (thing.TypeOfShape == "gift" && isGiftHit)
                //{
                //    thing.State = ThingState.Remove;//bbb
                //    addGift = false;
                //    isGiftHit = false;
                //}

                // Get rid of after dissolving.


                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                //   במצב שה"סינג" במצב מעומעם תעמעם את ה"סינג" שלו כאשר המשתנה "דיסולב" שהפך בין מצב התפוצצות למצב עמעום ל0 
                //עולה לאט לאט וכשהוא גדול שווה ל1 מצב ה"סינג" הופך להיות במצב מוסר
                if (thing.State == ThingState.Dissolving)//bbb
                {

                    thing.Dissolve += 1 / (this.targetFrameRate * DissolveTime);
                    //$$$$$$$$$$$$$$$כך גודל הצורה לא משתנה כאשר נוגעים ביד הלא נכונה
                    //thing.Size *= this.expandingRate;
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    
                    
                   if (thing.Dissolve >= 1.0 )
                    {
                        thing.State = ThingState.Remove;//bbb

                    }

                }

                this.things[thingIndex] = thing;
            }

            // Then remove any that should go away now

            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
            //לאחר שעבר על מערך ה"סינגים" בודק אם יש "סינגים" שצריך להסיר

            for (int i = 0; i < this.things.Count; i++)
            {
                Thing thing = this.things[i];
                if (thing.State == ThingState.Remove)//bbb
                {
                    this.things.Remove(thing);


                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                    //לאחר שפגע בצורה למעלה מאפשר פגיעה בסל
                    //בהתחלה ברשימה הופיה קודם הצורה למעלה ומיד אחריה הצורה שבסל
                    // כיוון שהסרנו את הצורה למעלה הצורה של הסל תפסה את מקומה 
                    int index = i;
                    for (int k = 0; k < things.Count; k++)
                    {
                      
                        if (k == index)
                        {
                            Thing thingToUpdate = this.things[k];
                                thingToUpdate.CanHit = true;
                            this.things[k] = thingToUpdate;
                        }
                    }
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                    i--;
                }
            }
            var x4 = this.things.Count;
            // Create any new things to drop based on dropRate
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //if we want a few shape together we need to change code to:
            //if ((this.things.Count < this.maxThings) && (this.rnd.NextDouble() < this.dropRate / this.targetFrameRate) && (this.polyTypes != PolyType.None))
            if (/*loadNewShapesThings ||/* (isTimeOut && currentHitThing=="basket" )   ||*/ isGiftHit || ((this.things.Count == 0) && (this.rnd.NextDouble() < this.dropRate / this.targetFrameRate) && (this.polyTypes != PolyType.None))) //$$*/
          {
               // text = myTime.Minutes.ToString("00") + ":" + myTime.Seconds.ToString("00");
               
                isTimeOut = false;
                isGiftHit = false;
                currentHitThing = "shape";
                this.things.Clear();
                PolyType[] alltypes =
                {
                    PolyType.Triangle, PolyType.Square, PolyType.Star, PolyType.Pentagon,
                    PolyType.Hex, PolyType.Star7, PolyType.Circle, PolyType.Bubble
                };
                byte r;
                byte g;
                byte b;

                if (this.doRandomColors)
                {
                    r = (byte)(this.rnd.Next(215) + 40);
                    g = (byte)(this.rnd.Next(215) + 40);
                    b = (byte)(this.rnd.Next(215) + 40);
                }
                else
                {
                    r = (byte)Math.Min(255.0, this.baseColor.R * (0.7 + (this.rnd.NextDouble() * 0.7)));
                    g = (byte)Math.Min(255.0, this.baseColor.G * (0.7 + (this.rnd.NextDouble() * 0.7)));
                    b = (byte)Math.Min(255.0, this.baseColor.B * (0.7 + (this.rnd.NextDouble() * 0.7)));
                }

                PolyType tryType;
                do
                {
                    tryType = alltypes[this.rnd.Next(alltypes.Length)];
                }
                while ((this.polyTypes & tryType) == 0);
                if(!loadNewThingsImg )
                {
                    int x = currentIndexshapeBasketList % 4;
                    this.DropNewThing(tryType, this.shapeSize, System.Windows.Media.Color.FromRgb(r, g, b), x);
                    currentIndexshapeBasketList++;

                    //בשביל כמה איטרציות על רשימת הסינגים
                    if (currentIndexshapeBasketList % 4 == 0)//במצב שאנחנו כבר באיטרציה הבאה על אוסף התפוח-סל שלנו
                    {
                        currentItration++;
                        nextItration = true;
                    }
                    if (currentItration > NumOfItrations)//במצב שרצנו כמספר האיטרציות שהגדרנו בתחילת המשחק-המשחק נגמר
                    {
                        //endGame = new DateTime();
                        //endGame = DateTime.Now;
                        isEndGame = true;
                        endGameMode = "winner";
                        this.endGameTime = DateTime.Now;

                    }

                }


            }
            
        }

        //~13
        //=========================================================================
        //14
        public void DrawFrame(UIElementCollection children)
        {
            this.frameCount++;

            // Draw all shapes in the scene
            for (int i = 0; i < this.things.Count; i++)
            {
                Thing thing = this.things[i];
                if (thing.Brush == null)
                {
                    thing.Brush = new SolidColorBrush(thing.Color);
                    double factor = 0.4 + (((double)thing.Color.R + thing.Color.G + thing.Color.B) / 1600);
                    thing.Brush2 =
                        new SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(
                                (byte)(255 - ((255 - thing.Color.R) * factor)),
                                (byte)(255 - ((255 - thing.Color.G) * factor)),
                                (byte)(255 - ((255 - thing.Color.B) * factor))));
                    thing.BrushPulse = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                }


                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                //כאשר היתה פגיעה והצורה היתה במצב מתפוצץ
                if (thing.State == ThingState.Bouncing)//bbb
                {
                    // Pulsate edges
                    double alpha = Math.Cos((0.15 * (thing.FlashCount++) * thing.Hotness) * 0.5) + 0.5;
                    int type = i;
                    children.Add(
                        this.MakeSimpleShape(
                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                            //החלפנו לצורך צורות מוגדרות מראש
                            //this.polyDefs[thing.Shape].Sides,
                            //this.polyDefs[thing.Shape].Skip,
                            //thing.Size,
                            //thing.Theta,
                            //thing.Center,
                            //thing.Brush,
                            //thing.BrushPulse,
                            //thing.Size * 0.1,
                            //alpha));
                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                            thing.TypeOfShape,
                            i,
                            this.polyDefs[thing.Shape].Sides,
                            this.polyDefs[thing.Shape].Skip,
                            thing.Size,
                            thing.Theta,
                            thing.Center,
                            Brushes.Transparent,//צבע הצורה
                            Brushes.Blue,//צבע מתאר צורה
                            6,//עובי מתאר צורה
                            alpha));
                    this.things[i] = thing;
                }
                else
                {
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                    //במצב מעומעם נרצה לשנות את אטימות הצבע של ה"סינג
                    if (thing.State == ThingState.Dissolving)//bbb
                    {
                        thing.Brush.Opacity = 1.0 - (thing.Dissolve * thing.Dissolve);
                    }
                    int type = i;//$$
                    children.Add(
                        this.MakeSimpleShape(

                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                            //החלפנו לצורך צורות מוגדרות מראש
                            //this.polyDefs[thing.Shape].Sides,
                            //this.polyDefs[thing.Shape].Skip,
                            //thing.Size,
                            //thing.Theta,
                            //thing.Center,
                            //thing.Brush,
                            //(thing.State == ThingState.Dissolving) ? null : thing.Brush2,
                            //1,
                            //1));
                            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                            thing.TypeOfShape,
                            i,
                            this.polyDefs[thing.Shape].Sides,
                            this.polyDefs[thing.Shape].Skip,
                            thing.Size,
                            thing.Theta,
                            thing.Center,
                            Brushes.Transparent,//צבע הצורה
                            Brushes.Blue,//צבע מתאר צורה
                            6,//עובי מתאר צורה
                            1));
                }
            }
           

            // Show game timer
            if (this.gameMode != GameMode.Off)
            {
                //if (loadNewThingsImg)
                //{
                //    text = myTime.Minutes.ToString("00") + ":" + myTime.Seconds.ToString("00");
                //    ifPress = false;
                //}

                if (ifPress)
                {
                    doAnimation = false;
                    myTime = DateTime.Now.Subtract(this.gameStartTime);

                    text = myTime.Minutes.ToString(CultureInfo.InvariantCulture) + ":" + myTime.Seconds.ToString("00");
                }
                

                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                //להגביל את המשתמש לעשות את התרגילים בזמן מוגבל מסוים ובמקרה ולא מצליח להוריד לו נקודות(ולאפשר לו לנסות שוב)0
                if (myTime.Seconds > maxSecondForThingsPair || doAnimation)
                {
                    if(doAnimation)
                    {
                        this.gameStartTime = DateTime.Now;
                        myTime = DateTime.Now.Subtract(this.gameStartTime);
                        text = myTime.Minutes.ToString("00") + ":" + myTime.Seconds.ToString("00");
                    }
                       

                    else if (currentHitThing=="basket" && addGift )//במקרה שנמצא במצב תפיסת מתנה והזמן עבר-הוא פספס את ההזדמנות
                    {
                        isTimeOut = true;
                        isGiftHit = true;
                        text = myTime.Minutes.ToString("00") + ":" + myTime.Seconds.ToString("00");
                        ifPress = false;

                    }
                    else if(myTime.Seconds > maxSecondForThingsPair)
                    {
                        isTimeOut = true;

                        FlyingText.NewFlyingText(10, new Point(310, 500), "-1");
                        //sumOfScore = sumOfScore - 1;
                         numOfDisqu++;
                        
                        var x = 4;
                        //if (numOfDisqu >= maxDisqu)
                        //{

                        //    isEndGame = true;
                        //    this.endGameTime = DateTime.Now;
                        //    endGameMode = "lost";
                        //}
                        //toUpdateScore = true;


                        //Show scores after remove timeOut fine
                       
                        //        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$44
                        //        //נסתיר את הניקוד האמיתיו ונראה ניקוד זמני שיחושב לאחר הורדת הקנס על הזמן שעבר.
                        //        //וכאשר יכניס את הצורה לסל נפחית מהניקוד האמיתי את הקנס.
                                labelOfSCores.Visibility = Visibility.Visible;


                       
                        labelOfSCores.Visibility = Visibility.Visible;
                        //doAnimation = false;
                        //מתחילים את הספירה מחדש
                        myTime = DateTime.Now.Subtract(this.gameStartTime);
                        text = myTime.Minutes.ToString("00") + ":" + myTime.Seconds.ToString("00"); //עדיין לא הכניס את התפוח לסל ועבר הזמן
                       



                    }

                }
                //else 
                //    text = myTime.Minutes.ToString("00") + ":" + myTime.Seconds.ToString("00");


                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                //if (doAnimation)
                //    ;

               

                Label timeText = MakeSimpleLabel(
                  text,
                    new Rect(
                    0.001 * this.sceneRect.Width, 0.001 * this.sceneRect.Height + 150, //מיקום הטיימר -"וואי" יותר גבוה-אוביקט יותר נמוך
                     0.89 * this.sceneRect.Width, 0.72 * this.sceneRect.Height),
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(160, 255, 255, 255)));
                timeText.FontSize = Math.Max(1, this.sceneRect.Height / 16);
                //timeText.Margin = new Thickness(20, 247, 855.2, 1110.4);
                //timeText.Margin(200, 315, 650, 1090.4);
                timeText.HorizontalContentAlignment = HorizontalAlignment.Right;
                timeText.VerticalContentAlignment = VerticalAlignment.Bottom;
                children.Add(timeText);


                // Show scores
                // if (this.scores.Count != 0)
                //{
                //int i = 0;
                //foreach (var score in this.scores)
                //{
                // if (score.Value == 0)
                //  ;

                        //sumOfScore=  sumOfScore - fine;
                        labelOfSCores = MakeSimpleLabel(
                            sumOfScore.ToString(CultureInfo.InvariantCulture),
                            new Rect(
                                (0.02 + (0 * 0.6)) * this.sceneRect.Width,
                                0.01 * this.sceneRect.Height,
                                0.4 * this.sceneRect.Width,
                                0.3 * this.sceneRect.Height),

                                new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255)));

                        labelOfSCores.FontSize = Math.Max(1, Math.Min(this.sceneRect.Width / 12, this.sceneRect.Height / 12));
                        children.Add(labelOfSCores);
                        //i++;
                   // }
                //}
               

            }


        }
        //~14
        //=========================================================================
        //15
        private static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            return ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
        }
        //~15
        //=========================================================================

        //16
        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
        //הוספנו קבלת סוג הפגיעה

        //private void AddToScore(int player, int points, System.Windows.Point center, string typeOfHit)
        //{

          
        //    sumOfScore = sumOfScore + points + scoreGameToAdd;
        //    var p = new Point(300, 300);
            
        //    int score = points + scoreGameToAdd;

        //    FlyingText.NewFlyingText(this.sceneRect.Width / 300, p, "+" + score.ToString());

        //}
        //~16

        //=========================================================================
        //17
        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
        //הוספנו קבלת nextPos
        private void DropNewThing(PolyType newShape, double newSize, System.Windows.Media.Color newColor, int nextPos)
        {
            // Only drop within the center "square" area 
            double dropWidth = this.sceneRect.Bottom - this.sceneRect.Top;
            if (dropWidth > this.sceneRect.Right - this.sceneRect.Left)
            {
                dropWidth = this.sceneRect.Right - this.sceneRect.Left;
            }

            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //רצינו צורות עם מאפינים קבועים ולכן החלפנו 

            //var newThing = new Thing
            //{
            //    Size = newSize,
            //    YVelocity = ((0.5 * this.rnd.NextDouble()) - 0.25) / this.targetFrameRate,
            //    XVelocity = 0,
            //    Shape = newShape,
            //    Center = new System.Windows.Point((this.rnd.NextDouble() * dropWidth) + ((this.sceneRect.Left + this.sceneRect.Right - dropWidth) / 2), this.sceneRect.Top - newSize),
            //    SpinRate = ((this.rnd.NextDouble() * 12.0) - 6.0) * 2.0 * Math.PI / this.targetFrameRate / 4.0,
            //    Theta = 0,
            //    TimeLastHit = DateTime.MinValue,
            //    AvgTimeBetweenHits = 100,
            //    Color = newColor,
            //    Brush = null,
            //    Brush2 = null,
            //    BrushPulse = null,
            //    Dissolve = 0,
            //    State = ThingState.Falling,
            //    TouchedBy = 0,
            //    Hotness = 0,
            //    FlashCount = 0
            //};
           //this.things.Clear();
            //this.things.Add(newThing);
            int i = 0;
            for (; i <= _shapeBasketList.Count; i++)

            {
                

                if (i == nextPos && !loadNewThingsImg)

                {
                    isGiftHit = false;//לפני שניצור סינגים חדשים נעדכן את משתנה ששומר פגיעה במתנה

                    if (nextPos % 2 == 0)
                        addGift = true;

                    else
                        addGift = false;

                    _wantedGesture = _shapeBasketList[i];

                    var newThing = new Thing
                    {
                        Size = 45,
                        YVelocity = ((0.5 * this.rnd.NextDouble()) - 0.25) / this.targetFrameRate,
                        XVelocity = 0,
                        Shape = PolyType.Circle,
                        Center = _shapeBasketList[i].Item1,
                        SpinRate = ((this.rnd.NextDouble() * 12.0) - 6.0) * 2.0 * Math.PI / this.targetFrameRate / 4.0,
                        Theta = 0,
                        TimeLastHit = DateTime.MinValue,
                        AvgTimeBetweenHits = 100,
                        Color = newColor,
                        //Brush = Brushes.Transparent,
                        //Brush2 = null,
                        //BrushPulse = Brushes.Transparent,
                        Brush = null,
                        Brush2 = null,
                        BrushPulse = null,
                        Dissolve = 0,
                        State = ThingState.Falling,
                        CanHit = true,
                        TypeOfShape = "shape",
                        TouchedBy = 0,
                        Hotness = 0,
                        FlashCount = 0,

                      
                    };
                    this.things.Add(newThing);


                    var basket = new Thing
                    {

                        Size = 130,
                        YVelocity = ((0.5 * this.rnd.NextDouble()) - 0.25) / this.targetFrameRate,
                        XVelocity = 0,
                        Shape = PolyType.Circle,
                        Center = _shapeBasketList[i].Item2,

                        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                        //מנענו את תזוזת הסל
                        //SpinRate = ((this.rnd.NextDouble() * 12.0) - 6.0) * 2.0 * Math.PI / this.targetFrameRate / 4.0,
                        SpinRate = 0,//$$
                        Theta = 0,
                        TimeLastHit = DateTime.MinValue,
                        AvgTimeBetweenHits = 100,
                        //Color = Color.FromRgb(255, 255, 255),  //צבע שקוף
                        Color = newColor,
                        //Brush = Brushes.Transparent,
                        //Brush2 = null,
                        //BrushPulse = Brushes.Transparent,
                        Brush = null,
                        Brush2 = null,
                        BrushPulse = null,
                        Dissolve = 0,
                        State = ThingState.Falling,
                        CanHit = false,
                        TypeOfShape = "basket",
                        TouchedBy = 0,
                        Hotness = 0,
                        FlashCount = 0
                    };
                    this.things.Add(basket);

                    //if (addGift /*&& typeOfGift!="SR"*/)//זיהוי קולי הוא היחיד שלא מצריך הוספת סינג
                    //{
                        var gift = new Thing
                        {
                            Size = 45,
                            YVelocity = ((0.5 * this.rnd.NextDouble()) - 0.25) / this.targetFrameRate,
                            XVelocity = 0,
                            Shape = PolyType.Circle,
                            Center = new Point(_shapeBasketList[i].Item2.X,_shapeBasketList[i].Item1.Y),
                            SpinRate = ((this.rnd.NextDouble() * 12.0) - 6.0) * 2.0 * Math.PI / this.targetFrameRate / 4.0,
                            Theta = 0,
                            TimeLastHit = DateTime.MinValue,
                            AvgTimeBetweenHits = 100,
                            Color = newColor,
                            //Brush = Brushes.Transparent,
                            //Brush2 = null,
                            //BrushPulse = Brushes.Transparent,
                            Brush = null,
                            Brush2 = null,
                            BrushPulse = null,
                            Dissolve = 0,
                            State = ThingState.Falling,
                            CanHit = false,
                            TypeOfShape = "gift",
                            TouchedBy = 0,
                            Hotness = 0,
                            FlashCount = 0,

                        };
                        this.things.Add(gift);
                    // }
                    if (!addGift)
                        this.things.Remove(gift);


                    // loadNewThings = false;


                }

            }
           
        }
        //~17
        //=========================================================================

        //18
        private Shape MakeSimpleShape(
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //הוספנו מאפיינים לצורה
            string typeOfShape,//$$
            int type,//$$
            int numSides,
            int skip,
            double size,
            double spin,
            System.Windows.Point center,
            System.Windows.Media.Brush brush,
            System.Windows.Media.Brush brushStroke,
            double strokeThickness,
            double opacity)
        {
            if (numSides <= 1)
            {
                var circle = new Ellipse { Width = size * 2, Height = size * 2, Stroke = brushStroke };
                //הורדנו בשביל הרקע שקוף
                //if (circle.Stroke != null)
                //{
                //    circle.Stroke.Opacity = opacity;
                //}
                circle.StrokeThickness = strokeThickness * ((numSides == 1) ? 1 : 2);

                //circle.Fill = (numSides == 1) ? brush : null;//צבע צורה כאשר הצורה היא עיגול-ביטלנו ע"י שעשינו תמיד צבע שקוף
                circle.Fill = (numSides == 1) ? Brushes.Transparent : null;
                circle.SetValue(Canvas.LeftProperty, center.X - size);
                circle.SetValue(Canvas.TopProperty, center.Y - size);

                return circle;
            }

            var points = new PointCollection(numSides + 2);
            double theta = spin;
            for (int i = 0; i <= numSides + 1; ++i)
            {
                points.Add(new System.Windows.Point((Math.Cos(theta) * size) + center.X, (Math.Sin(theta) * size) + center.Y));
                theta = theta + (2.0 * Math.PI * skip / numSides);
            }

            var polyline = new Polyline { Points = points, Stroke = brushStroke };
            if (polyline.Stroke != null)
            {
                polyline.Stroke.Opacity = opacity;
            }

            polyline.Fill = brush;
            polyline.FillRule = FillRule.Nonzero;
            polyline.StrokeThickness = strokeThickness;
            return polyline;
        }
        //~18
        //=========================================================================

        //19
        internal struct PolyDef
        {
            public int Sides;
            public int Skip;
        }
        //~19

        //=========================================================================
        //20

        // The Thing struct represents a single object that is flying through the air, and
        // all of its properties.
        private struct Thing
        {
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //הוספנו מאפיינים לצורה

            public System.Windows.Media.ImageBrush image;//$$
            public bool CanHit;//$$
            public string TypeOfShape;//regular shape or basket //$$
            public System.Windows.Point Center;
            public double Size;
            public double Theta;
            public double SpinRate;
            public double YVelocity;
            public double XVelocity;
            public PolyType Shape;
            public System.Windows.Media.Color Color;
            public System.Windows.Media.Brush Brush;
            public System.Windows.Media.Brush Brush2;
            public System.Windows.Media.Brush BrushPulse;
            public double Dissolve;
            public ThingState State;
            public DateTime TimeLastHit;
            public double AvgTimeBetweenHits;
            public int TouchedBy;               // Last player to touch this thing-השחקן שנגע בצורה
            public double marginX;
            public double margin;

            public int Hotness;                 // Score level
            public int FlashCount;


            //~20


            //=========================================================================
            //21

            // Hit testing between this thing and a single segment.  If hit, the center point on
            // the segment being hit is returned, along with the spot on the line from 0 to 1 if
            // a line segment was hit.

            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //מחזירה טרו אם היתה פגיעה בין "סינג" ואחת מהסגמטים=מקטעי השלד
            public bool Hit(Segment seg, ref System.Windows.Point hitCenter, ref double lineHitLocation)
            {
                double minDxSquared = this.Size + seg.Radius;
                minDxSquared *= minDxSquared;

                // See if falling thing hit this body segment
                if (seg.IsCircle())
                {
                    if (SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) <= minDxSquared)
                    {
                        hitCenter.X = seg.X1; //bbb
                        hitCenter.Y = seg.Y1;//bbb
                        lineHitLocation = 0;//bbb
                        return true;
                    }
                }
                else
                {
                    double sqrLineSize = SquaredDistance(seg.X1, seg.Y1, seg.X2, seg.Y2);
                    if (sqrLineSize < 0.5)
                    {
                        // if less than 1/2 pixel apart, just check dx to an endpoint
                        return SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) < minDxSquared;
                    }

                    // Find dx from center to line
                    double u = ((this.Center.X - seg.X1) * (seg.X2 - seg.X1)) + (((this.Center.Y - seg.Y1) * (seg.Y2 - seg.Y1)) / sqrLineSize);
                    if ((u >= 0) && (u <= 1.0))
                    {   // Tangent within line endpoints, see if we're close enough
                        double intersectX = seg.X1 + ((seg.X2 - seg.X1) * u);
                        double intersectY = seg.Y1 + ((seg.Y2 - seg.Y1) * u);

                        if (SquaredDistance(this.Center.X, this.Center.Y, intersectX, intersectY) < minDxSquared)
                        {
                            lineHitLocation = u;//bbb
                            hitCenter.X = intersectX;//bbb
                            hitCenter.Y = intersectY;//bbb
                            return true;
                        }
                    }
                    else
                    {
                        // See how close we are to an endpoint
                        if (u < 0)
                        {
                            if (SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) < minDxSquared)
                            {
                                lineHitLocation = 0;//bbb
                                hitCenter.X = seg.X1;//bbb
                                hitCenter.Y = seg.Y1;//bbb
                                return true;
                            }
                        }
                        else
                        {
                            if (SquaredDistance(this.Center.X, this.Center.Y, seg.X2, seg.Y2) < minDxSquared)
                            {
                                lineHitLocation = 1;//bbb
                                hitCenter.X = seg.X2;//bbb
                                hitCenter.Y = seg.Y2;//bbb
                                return true;
                            }
                        }
                    }

                    return false;
                }

                return false;
            }

            //~21

            //=========================================================================
            //22

            // Change our velocity based on the object's velocity, our velocity, and where we hit.
            public void BounceOff(double x1, double y1, double otherSize, double fXv, double fYv)
            {
                double x0 = this.Center.X;
                double y0 = this.Center.Y;
                double xv0 = this.XVelocity - fXv;
                double yv0 = this.YVelocity - fYv;
                double dist = otherSize + this.Size;
                double dx = Math.Sqrt(((x1 - x0) * (x1 - x0)) + ((y1 - y0) * (y1 - y0)));
                double xdif = x1 - x0;
                double ydif = y1 - y0;
                double newvx1 = 0;
                double newvy1 = 0;

                x0 = x1 - (xdif / dx * dist);
                y0 = y1 - (ydif / dx * dist);
                xdif = x1 - x0;
                ydif = y1 - y0;

                double bsq = dist * dist;
                double b = dist;
                double asq = (xv0 * xv0) + (yv0 * yv0);
                double a = Math.Sqrt(asq);
                if (a > 0.000001)
                {
                    // if moving much at all...
                    double cx = x0 + xv0;
                    double cy = y0 + yv0;
                    double csq = ((x1 - cx) * (x1 - cx)) + ((y1 - cy) * (y1 - cy));
                    double tt = asq + bsq - csq;
                    double bb = 2 * a * b;
                    double power = a * (tt / bb);
                    newvx1 -= 2 * (xdif / dist * power);
                    newvy1 -= 2 * (ydif / dist * power);
                }

                this.XVelocity += newvx1;
                this.YVelocity += newvy1;
                this.Center.X = x0;
                this.Center.Y = y0;
            }


        }
        //~22
        //=========================================================================


        //23

        public KeyValuePair<string, int> GetTypeAndIdPlayerOfCurrentHitThing()
        {

            return typeAndIdPlayerOfCurrentHitThing;

        }

        //~23
        //=========================================================================
    }
}
