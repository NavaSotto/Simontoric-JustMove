   M S B A M L   `   `   `  ����^  ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers  5   	   S OMicrosoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=nulleHclr-namespace:Microsoft.Kinect.Toolkit;assembly=Microsoft.Kinect.ToolkitMicrosoft.Kinect.Toolkit 5   	    	ShapeGame"   ShapeGame.JustMoveMainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   0k(http://schemas.microsoft.com/kinect/2013  5      gkt^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers  5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   \toolkitHclr-namespace:Microsoft.Kinect.Toolkit;assembly=Microsoft.Kinect.Toolkit     5   	   -   5          � Title$  Kinect Shape Game��5   	   $��Italic$�6#   $��Segoe UI)�5   	    � WindowState$ 	Maximized=�5	   	    � WindowStyle$
 None=�6!    � ShowInTaskbar .   � 
ResizeMode$ 	CanResize=�5   	   ��5      �� 5   
   -   5        backgroundWindow�� �� ImageSource$ justMoveGameScreen.png��5   /    j� Stretch$
 Fill=�6T   6e    � 	Resources 5         fullBasketAnimation �� 
TargetName	 �� TargetProperty
 g� Duration	 �� To g� AutoReverse��%�   &	        �� 5   
   .��5      �� -   5        $ sb��5      $	 Opacity�65   .��5      �� $
 	00:00:0.5D�5      $0	 *(Panel.Background).(SolidColorBrush.Color)�5      $ WhiteX�5       . 5      5      5      5"      .��5%      � .+�5&   
   ��  �� Orientation$ Vertical=�6   
������ .+�5'      �  � ColumnDefinitions 5)      .+�5/      �� -   5        $��movementImg��5/   &   $	��166��6   $��D1R.PNG��69   $y�Hidden=�6J   $��85��6^   $��970,-369,-293.4,1735q�6j   $
��Fill=�6�   $��Bottom=�6�   �� 50      -   5        $��shapeImg��50      $��
shape1.png��6)   $y�Hidden=�6?   $
��Fill=�6T    J� RenderTransformOrigin$ 1.227,0.842&�6c   $��808.4,535,57.2,927.8q�6�   $��1��6�   �� 51      -   5        $��	basketImg��51      $��emptyBasket.png��6*   $y�Visible=�6D   $��451,314,143.6,1084q�6Z   $
��Fill=�6v   �� 52      -   5        $��giftImg��52      $��coin.png��6(   $y�Hidden=�6;   $
��Fill=�6O   $��356,359,355.6,1116.6q�6^   �� 53      -   5        $��fullBasketImg��53      $��69��6-   $��fullBasket.png��69   $
��Fill=�6Q   $��80��6`   $y�Hidden=�6k    �� Left$	 410��6   
 �� Top$	 380��6�   $ 0.438,0.041&�6�   �� 55      -	   5        $��	playfield��55   2   $��0,0,-6.4,1010.6q�6   ��58      �� 5:       �� EndPoint$ 0.5,1&�6.    �� 
StartPoint$ 0.5,0&�6?   5@      5D      4  .Microsoft.Kinect.Toolkit.KinectSensorChooserUI  5G      -
   5          Name$ SensorChooserUI��5G   0      IsListening . $��38��6\   $��46��6h   $��Stretch=�6s   $��Bottom=�6�   $ 0.575,1.867&�6�   $��315,-365,400.6,1859q�6�     5I      -   5        $��sensorChooserUi��5I   ^   $��Right=�6*   $	��Top=�6F   $��38��6v   $��46��6�   $ 0.564,-0.041&�6�   $��0,51,711.6,0q�6�   z�5J      V� 5K      .~�5L      �� �� 5M       �� AngleX$ 0.808Q�6,   �� 5N      T� 5O       T� X$ -0.795Q�61   5P      5R      5S      �� 5T      -   5         ��Name$ 	enableAec��5T      
���  ��$��20(�6;   $��	19,0,0,18q�6I   $
��Left=�6\   $��Bottom=�6w   $y�	Collapsed=�6�   {�.  .��5U   D   בהצלחה!!!5X      5\      �� 5]   
   -   5        $��sb��5]   "   $��Button��6   $
��Left=�6,   $��497,248,0,0q�6G   
�������$y�Hidden=�6q   $	��Top=�6�   $��75��6�   5^      5_      