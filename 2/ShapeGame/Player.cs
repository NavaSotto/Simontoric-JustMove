//------------------------------------------------------------------------------
// <copyright file="Player.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace ShapeGame
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using ShapeGame.Utils;

    public class Player
    {
        private const double BoneSize = 0.02;
        private const double HeadSize = 0.095;
        private const double HandSize = 0.04;

        // Keeping track of all bone segments of interest as well as head, hands and feet
        private /*readonly*/ Dictionary<Bone, BoneData> segments = new Dictionary<Bone, BoneData>();
        public /*$$ readonly $$*/ System.Windows.Media.Brush jointsBrush;
        public /*$$ readonly $$*/ System.Windows.Media.Brush bonesBrush;
        public /*$$ readonly $$*/ System.Windows.Media.Brush jointsBrushesOFsegmentHit = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 0)) /*צהוב*/;
        public /*$$ readonly $$*/ System.Windows.Media.Brush bonesBrushesOFsegmentHit = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 61))/*ירוק*/;
        public readonly int id;
        private static int colorId;
        private Rect playerBounds;
        public System.Windows.Point playerCenter;
        private double playerScale;
        private Boolean isHitSegment1;//בדיקה פגיעה עם הראש

        private Boolean isHitSegment2;//בדיקה פגיעה עם משהו אחר
        //$$$$$$$$$$$$4
        private int i = 0;
        int indexSegmentHit = 0;
        public bool isPlayerHit = false;



        //$$public bool isHit=false;

        public KeyValuePair<string, int> typeAndPlayerIdHit;
        public string str;
        //$$$$$$$$$$$$$$


        public Player(int skeletonSlot)
        {
            this.id = skeletonSlot;
        }

        public bool IsAlive { get; set; }

        public DateTime LastUpdated { get; set; }

        public Dictionary<Bone, BoneData> Segments
        {
            get
            {
                return this.segments;
            }
        }

        public int GetId()
        {
            return this.id;
        }


        public void SetJointsAndBonesBrushesOfPlayers(int numOfPlayers)  //$$
        {

            if (numOfPlayers == 0) //$$
            {

                this.jointsBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 128, 0)); //כתום- קודקודים
                this.bonesBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 128, 0));     //כתום-עצמות
                this.LastUpdated = DateTime.Now;//$$
            }
            //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
            //במקרה של שני שחקנים צבע השלד יוגרל

            else
            {
                // Generate one of 7 colors for player
                int[] mixR = { 1, 1, 1, 0, 1, 0, 0 };
                int[] mixG = { 1, 1, 0, 1, 0, 1, 0 };
                int[] mixB = { 1, 0, 1, 1, 0, 0, 1 };
                byte[] jointCols = { 245, 200 };
                byte[] boneCols = { 235, 160 };


                int i = colorId;
                colorId = (colorId + 1) % mixR.Count();

                this.bonesBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(jointCols[mixR[i]], jointCols[mixG[i]], jointCols[mixB[i]]));
                this.jointsBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(boneCols[mixR[i]], boneCols[mixG[i]], boneCols[mixB[i]]));
                this.LastUpdated = DateTime.Now;
            }
        }




        public void SetBounds(Rect r)
        {
            //מיקום השחקן
            this.playerBounds = r;
            this.playerCenter.X = (this.playerBounds.Left + this.playerBounds.Right) / 2;
            this.playerCenter.Y = (this.playerBounds.Top + this.playerBounds.Bottom) / 2 + 150;
            this.playerScale = Math.Min(this.playerBounds.Width, this.playerBounds.Height / 2) + 150;
        }

        public void UpdateBonePosition(Microsoft.Kinect.JointCollection joints, JointType j1, JointType j2)
        {
            var seg = new Segment(
                (joints[j1].Position.X * this.playerScale) + this.playerCenter.X,
                this.playerCenter.Y - (joints[j1].Position.Y * this.playerScale),
                (joints[j2].Position.X * this.playerScale) + this.playerCenter.X,
                this.playerCenter.Y - (joints[j2].Position.Y * this.playerScale))
            { Radius = Math.Max(3.0, this.playerBounds.Height * BoneSize) / 2 };
            this.UpdateSegmentPosition(j1, j2, seg);
        }

        public void UpdateJointPosition(Microsoft.Kinect.JointCollection joints, JointType j)
        {
            var seg = new Segment(
                (joints[j].Position.X * this.playerScale) + this.playerCenter.X,
                this.playerCenter.Y - (joints[j].Position.Y * this.playerScale))
            { Radius = this.playerBounds.Height * ((j == JointType.Head) ? HeadSize : HandSize) / 2 };
            this.UpdateSegmentPosition(j, j, seg);
        }

        public void Draw(UIElementCollection children)
        {

            if (!this.IsAlive)
            {
                return;
            }

            // Draw all bones first, then circles (head and hands).
            DateTime cur = DateTime.Now;
            foreach (var segment in this.segments)
            {
                Segment seg = segment.Value.GetEstimatedSegment(cur);

                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                //תפיסת הסגמנט שפגע ב"סינג" על מנת לצבוע אותו בצבע אחר
                var isHitSegment1 = this.id == typeAndPlayerIdHit.Value && segment.Key.Joint1.ToString().Equals(typeAndPlayerIdHit.Key);//$$
                

                if (!seg.IsCircle())
                {
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$// 
                    //לצורך בדיקה
                    var a1 = this.id;
                    var b1 = typeAndPlayerIdHit.Value;
                    var c1 = segment.Key.Joint1.ToString();
                    var d1 = typeAndPlayerIdHit.Key;
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$// 



                    var line = new Line
                    {
                        StrokeThickness = seg.Radius * 2,
                        X1 = seg.X1,
                        Y1 = seg.Y1,
                        X2 = seg.X2,
                        Y2 = seg.Y2,
                        Stroke = isHitSegment1 && isPlayerHit? jointsBrushesOFsegmentHit : this.bonesBrush,//$$
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };

                    children.Add(line);
                }


                
            }

            foreach (var segment in this.segments)
            {
               
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                //תפיסת הסגמנט שפגע ב"סינג" על מנת לצבוע אותו בצבע אחר
                var isHitSegment2 = this.id == typeAndPlayerIdHit.Value && segment.Key.Joint1.ToString().Equals(typeAndPlayerIdHit.Key);//$$
               
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$// 
                //לצורך בדיקה
                var a2 = this.id;
                var b2 = typeAndPlayerIdHit.Value;
                var c2 = segment.Key.Joint1.ToString();
                var d2 = typeAndPlayerIdHit.Key;
                //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//


                Segment seg = segment.Value.GetEstimatedSegment(cur);
                if (seg.IsCircle())
                {

                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//??
                    //תפיסת הסגמנט שפגע ב"סינג" על מנת לצבוע אותו בצבע אחר

                    var circle = new Ellipse { Width = seg.Radius * 2, Height = seg.Radius * 2 };
                    circle.SetValue(Canvas.LeftProperty, seg.X1 - seg.Radius);
                    circle.SetValue(Canvas.TopProperty, seg.Y1 - seg.Radius);
                    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//
                    //צבע המתאר של מקומות עגולים כמו יד שמסמן כמו בוקס (עגול) והראש יהיה בצבע של  המפרקים 

                    circle.Stroke = isHitSegment2 && isPlayerHit ? jointsBrushesOFsegmentHit : this.jointsBrush;//צבע המפרקים //$$
                    circle.StrokeThickness = 1;
                    circle.Fill = isHitSegment2 && isPlayerHit ? jointsBrushesOFsegmentHit : new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 189, 0));//צבע הראש //$$


                    children.Add(circle);
                }

            }

            // Remove unused players after 1/2 second.
            if (DateTime.Now.Subtract(this.LastUpdated).TotalMilliseconds > 500)
            {
                this.IsAlive = false;
            }
        }

        private void UpdateSegmentPosition(JointType j1, JointType j2, Segment seg)
        {
            var bone = new Bone(j1, j2);
            if (this.segments.ContainsKey(bone))
            {
                BoneData data = this.segments[bone];
                data.UpdateSegment(seg);
                this.segments[bone] = data;

            }
            else
            {
                this.segments.Add(bone, new BoneData(seg));
            }
        }
    }
}
