using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// Interaction logic for GoodLuck.xaml
    /// </summary>
    public partial class GoodLuck : Window
    {
        private ShapeGame.JustMoveMainWindow w1;
        public string typeOfGame;
        public GoodLuck()
        {
            InitializeComponent();
            w1 = new ShapeGame.JustMoveMainWindow();

            w1.TypeOfGame = "קל";
            w1.Show();


            this.Close(); //GoodLuckWindow



        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {

            w1 = new ShapeGame.JustMoveMainWindow();

            w1.TypeOfGame = "קל";
            w1.Show();


            this.Close(); //GoodLuckWindow

           



        }

        private void CheckMode(object sender, EventArgs e)
        {
            //$$$$$$$$$$$$$4
            //            switch (w1.playerMode)
            //            {
            //                



           // "sayBack":
    //                    w1.Close();// JustMoveMainWindow
    //                    JustMoveMenu w2 = new JustMoveMenu();
    //                    w2.Show();
    //                    Close();//JustMoveMenu
    //                    break;
    //                case "win":
    //                    w1.Close();// JustMoveMainWindow
    //                    JustMoveWinOrLose w3 = new JustMoveWinOrLose();
    //                    w3.winOrLoseMode = "win";
    //                    w3.Show();
    //                    Close();//JustMoveMenu
    //                    break;
    //                case "lose":
    //                    w1.Close();// JustMoveMainWindow
    //                    JustMoveWinOrLose w4 = new JustMoveWinOrLose();
    //                    w4.winOrLoseMode = "lose";
    //                    w4.Show();
    //                    Close();//JustMoveMenu
    //                    break;
    //                    //default:
    //                    //    w1.Close();// JustMoveMainWindow
    //                    //    breake;
    //            }
  

       }
    }
}
