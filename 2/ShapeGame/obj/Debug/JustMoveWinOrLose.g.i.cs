﻿#pragma checksum "..\..\JustMoveWinOrLose.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "E582C6E4A5A2BB4C626A07B9FF778B437458F0E1"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ShapeGame {
    
    
    /// <summary>
    /// JustMoveWinOrLose
    /// </summary>
    public partial class JustMoveWinOrLose : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 18 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.ImageBrush backgroundWindow;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas playfield;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image movementImg;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image shapeImg;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image basketImg;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Kinect.Toolkit.KinectSensorChooserUI SensorChooserUI;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Kinect.Toolkit.KinectSensorChooserUI sensorChooserUi;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\JustMoveWinOrLose.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox enableAec;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ShapeGame;component/justmovewinorlose.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\JustMoveWinOrLose.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 13 "..\..\JustMoveWinOrLose.xaml"
            ((ShapeGame.JustMoveWinOrLose)(target)).Loaded += new System.Windows.RoutedEventHandler(this.WindowLoaded);
            
            #line default
            #line hidden
            
            #line 13 "..\..\JustMoveWinOrLose.xaml"
            ((ShapeGame.JustMoveWinOrLose)(target)).Closed += new System.EventHandler(this.WindowClosed);
            
            #line default
            #line hidden
            
            #line 13 "..\..\JustMoveWinOrLose.xaml"
            ((ShapeGame.JustMoveWinOrLose)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.backgroundWindow = ((System.Windows.Media.ImageBrush)(target));
            return;
            case 3:
            this.playfield = ((System.Windows.Controls.Canvas)(target));
            
            #line 29 "..\..\JustMoveWinOrLose.xaml"
            this.playfield.SizeChanged += new System.Windows.SizeChangedEventHandler(this.PlayfieldSizeChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.movementImg = ((System.Windows.Controls.Image)(target));
            return;
            case 5:
            this.shapeImg = ((System.Windows.Controls.Image)(target));
            return;
            case 6:
            this.basketImg = ((System.Windows.Controls.Image)(target));
            return;
            case 7:
            this.SensorChooserUI = ((Microsoft.Kinect.Toolkit.KinectSensorChooserUI)(target));
            return;
            case 8:
            this.sensorChooserUi = ((Microsoft.Kinect.Toolkit.KinectSensorChooserUI)(target));
            return;
            case 9:
            this.enableAec = ((System.Windows.Controls.CheckBox)(target));
            
            #line 64 "..\..\JustMoveWinOrLose.xaml"
            this.enableAec.Checked += new System.Windows.RoutedEventHandler(this.EnableAecChecked);
            
            #line default
            #line hidden
            
            #line 64 "..\..\JustMoveWinOrLose.xaml"
            this.enableAec.Unchecked += new System.Windows.RoutedEventHandler(this.EnableAecChecked);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

