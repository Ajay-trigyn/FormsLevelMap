﻿using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using System.Threading;

namespace FormsLevelMap
{
    public partial class MainPage : ContentPage
    {
        //.........................
        // new variables

        int buttonInserted = 1; 
        int lastDeletedButton = 0;
        int lastDeletedFrame = 0;
        double gapY = 10;
        int buttonBuffer = 24;
        List<Frame> frames = new List<Frame>();

        //.........................

        int numButtonsInPattern = 6;
        int widgetWidth = 350;                          //Width of buttons screen                250----350
        int buttonWidth = 60;
        int buttonHeight = 60;

        int curlinessFactor = 60;                       //Change if changing widgetWidth          30----60
        float currentScore = 10000;
        float segmentScore = 1000;

        int heightScale;
        int scrollThreshold;

        int cbiforcurve = 0;
        List<Button> buttons = new List<Button>();
        List<SKPoint[]> skLineList = new List<SKPoint[]>();
        Random rnd = new Random();
        Size size = Device.Info.PixelScreenSize;
        
        float traversedScore = 0.0f;
        //int TotalIterations = 0;
        ImageSource BGImage = ImageSource.FromResource("FormsLevelMap.Images.bg.jpg", typeof(MainPage));
        ImageSource GapImage = ImageSource.FromFile("saturn");

        List<SKPoint> lastPoints = new List<SKPoint>();
        List<SKPoint> firstPoints = new List<SKPoint>();
        List<SKPoint> scorePoints = new List<SKPoint>();

        SKPath streetPath = new SKPath();
        SKPath scorePath = new SKPath();
        SKPath collectedPath = new SKPath();
        SKPath clipperPath = new SKPath();
        SKPaint streetStroke;
        SKPaint scoreStroke;
        SKPaint collectedStroke;
        SKPaint clipStroke;

        public MainPage()
        {

            InitializeComponent();

            heightScale = widgetWidth / numButtonsInPattern;
            scrollThreshold = heightScale * numButtonsInPattern / 4;

            SkCanvasView.WidthRequest = widgetWidth;
            SkCanvasView2.WidthRequest = widgetWidth;
            MainAbsoluteLayout.WidthRequest = widgetWidth;

            //double screenWidth = size.Width;
            //double screenHeight = size.Height;

            streetStroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Miter
            };

            scoreStroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 12,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Miter
            };

            collectedStroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 5,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Miter
            };

            clipStroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 9,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Miter
            };

            InitialIteration();

            SkCanvasView.PaintSurface += SkCanvasView_OnPaintSurface;
            SkCanvasView2.PaintSurface += SkCanvasView_OnPaintSurface;

            #region "down_Swipe_Gesture_Recognizer"
            //var downSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            //downSwipeGesture.Threshold = 50;
            //downSwipeGesture.Swiped += (sender, e) => {
            //    Console.WriteLine("heyhey" + e.Direction.ToString());
            //    MainScrollView.ScrollToAsync(buttons[30], 0, true);
            //};
            //OuterAbsoluteLayout.GestureRecognizers.Add(downSwipeGesture);
            //MainScrollView.GestureRecognizers.Add(downSwipeGesture);
            #endregion "down_Swipe_Gesture_Recognizer"

            MainScrollView.Scrolled += ScrollEvent;

        }

        void InitialIteration()
        {
            for (var i = 0; i < 3; i++)
            {
                buttonCreater();
                //DeleteButtons();
                //TotalIterations++;
                backgroundStack.Children.Add(new Image()
                {
                    Source = BGImage,
                    //Source = BGImage,
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 350,
                    Margin = new Thickness(0, 0, 0, 0),

                });
            }
        }

        void buttonCreater()
        {
            var xyPoint = new List<SKPoint>();
            double gapImageShiftScaleX;
            double gapImageShiftScaleY;

            for (var i = 0; i < numButtonsInPattern; i++)
            {

                Image image = new Image
                {
                    Source = GapImage,
                    BackgroundColor = Color.Transparent,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = buttonHeight * 2,
                    WidthRequest = buttonWidth * 2,
                    RotationX = 180,
                };

                gapImageShiftScaleY = (image.Height - buttonHeight) / 2;

                Frame frame = new Frame
                {
                    CornerRadius = buttonWidth,
                    Content = image,
                    Padding = 0

                };

                int indexForButtonText = buttons.Count - 1;

                if (i % 2 != 0)
                {
                    buttons.Add(new Button
                    {
                        Text = (indexForButtonText + 1).ToString(),
                        WidthRequest = buttonWidth,
                        HeightRequest = buttonHeight,
                        CornerRadius = buttonWidth / 2,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        BackgroundColor = Color.Orange,
                        RotationX = 180,
                    });
                }
                else
                {
                    buttons.Add(new Button
                    {
                        Text = (indexForButtonText + 1).ToString(),
                        WidthRequest = buttonWidth,
                        HeightRequest = buttonHeight,
                        CornerRadius = buttonWidth / 2,
                        BackgroundColor = Color.Green,
                        RotationX = 180,

                    });
                }

                int currentButtonIndex = buttons.Count - 1;
                int placementIndex = currentButtonIndex;


                while (placementIndex > numButtonsInPattern - 1)
                {
                    placementIndex = placementIndex - numButtonsInPattern;
                }

                if (i < numButtonsInPattern / 2.0f)
                {
                    var x = 0 + placementIndex * (2.0 / (numButtonsInPattern));
                    var y = (currentButtonIndex * heightScale + Math.Sin((i / (numButtonsInPattern / 8.0f)) * Math.PI / 2) * curlinessFactor + 10);
                    //var y = currentButtonIndex * heightScale + rnd.Next(-randomnessFactor, +randomnessFactor/2);
                    AbsoluteLayout.SetLayoutBounds(buttons[currentButtonIndex], new Rectangle(x, y, buttons[currentButtonIndex].Width, buttons[currentButtonIndex].Height));
                    AbsoluteLayout.SetLayoutFlags(buttons[currentButtonIndex], AbsoluteLayoutFlags.XProportional);

                    //gapbutton 
                    if (buttons[buttons.Count - 1].Text == buttonInserted.ToString())
                    {
                        Console.WriteLine($"FinalX------------{x}");
                        gapImageShiftScaleX = ((image.Width - buttonWidth) * x) / (2 * widgetWidth);
                        Console.WriteLine($"Xscale------{gapImageShiftScaleX}");
                        AbsoluteLayout.SetLayoutBounds(frame, new Rectangle(x + gapImageShiftScaleX, gapY + gapImageShiftScaleY, image.Width, image.Height));
                        AbsoluteLayout.SetLayoutFlags(frame, AbsoluteLayoutFlags.XProportional);
                        MainAbsoluteLayout.Children.Add(frame);
                        frames.Add(frame);
                        gapY += 174;
                        buttonInserted += 3;
                    }


                    Console.WriteLine($"button1X------------{x}");
                    Console.WriteLine($"button1Y------------{y}");
                }

                else if (i == numButtonsInPattern / 2.0f)
                {
                    var x = 2 - placementIndex * (2.0 / (numButtonsInPattern));
                    var y = (currentButtonIndex * heightScale + 10);
                    //var y = currentButtonIndex * heightScale + rnd.Next(-randomnessFactor/2, +randomnessFactor);
                    AbsoluteLayout.SetLayoutBounds(buttons[currentButtonIndex], new Rectangle(x, y, buttons[0].Width, buttons[0].Height));
                    AbsoluteLayout.SetLayoutFlags(buttons[currentButtonIndex], AbsoluteLayoutFlags.XProportional);



                    Console.WriteLine($"ButtonNumber-----------{buttons[2].Text}");
                    Console.WriteLine($"button2X------------{x}");
                    Console.WriteLine($"button2Y------------{y}");

                }

                else
                {
                    var x = 2 - placementIndex * (2.0 / (numButtonsInPattern));
                    var y = (currentButtonIndex * heightScale + Math.Sin((i / (numButtonsInPattern / 8.0f)) * Math.PI / 2) * curlinessFactor + 10);
                    //var y = currentButtonIndex * heightScale + rnd.Next(-randomnessFactor/2, +randomnessFactor);
                    AbsoluteLayout.SetLayoutBounds(buttons[currentButtonIndex], new Rectangle(x, y, buttons[currentButtonIndex].Width, buttons[currentButtonIndex].Height));
                    AbsoluteLayout.SetLayoutFlags(buttons[currentButtonIndex], AbsoluteLayoutFlags.XProportional);


                    //gapbutton
                    if (buttons[buttons.Count - 1].Text == buttonInserted.ToString())
                    {
                        Console.WriteLine($"FinalX------------{x}");
                        gapImageShiftScaleX = ((image.Width - buttonWidth) * x) / (2 * widgetWidth);
                        Console.WriteLine($"Xscale------{gapImageShiftScaleX}");
                        AbsoluteLayout.SetLayoutBounds(frame, new Rectangle(x - gapImageShiftScaleX, gapY + gapImageShiftScaleY, image.Width, image.Height));
                        AbsoluteLayout.SetLayoutFlags(frame, AbsoluteLayoutFlags.XProportional);
                        MainAbsoluteLayout.Children.Add(frame);
                        frames.Add(frame);
                        gapY += 174;
                        buttonInserted += 3;
                    }

                }
                MainAbsoluteLayout.Children.Add(buttons[currentButtonIndex]);
            }

            int curveHeightScale = widgetWidth / 6;
            for (var i = 0; i < 6; i++)
            {
                int placementIndex = cbiforcurve;
                while (placementIndex > 5)
                {
                    placementIndex = placementIndex - 6;
                }

                if (i < 3.0f)
                {
                    var xforcurve = 0 + placementIndex * (2.0 / (6));
                    var yforcurve = (cbiforcurve * curveHeightScale + Math.Sin((i / (6 / 8.0f)) * Math.PI / 2) * curlinessFactor);

                    if (i == 0) { xyPoint.Add(new SKPoint((float)(xforcurve * widgetWidth) + 30, (float)yforcurve + 30)); }
                    else { xyPoint.Add(new SKPoint((float)(xforcurve * widgetWidth), (float)yforcurve + 30)); }
                }
                else if (i == 3.0f)
                {
                    var xforcurve = 2 - placementIndex * (2.0 / (6));
                    var yforcurve = (cbiforcurve * curveHeightScale);

                    xyPoint.Add(new SKPoint(((float)xforcurve * widgetWidth) - 30, (float)yforcurve + 30));
                }
                else
                {
                    var xforcurve = 2 - placementIndex * (2.0 / (6));
                    var yforcurve = (cbiforcurve * curveHeightScale + Math.Sin((i / (6 / 8.0f)) * Math.PI / 2) * curlinessFactor);

                    xyPoint.Add(new SKPoint((float)(xforcurve * widgetWidth), (float)yforcurve + 30));
                }
                cbiforcurve++;
            }

            firstPoints.Add(xyPoint[0]);
            lastPoints.Add(xyPoint[5]);

            for (var j = 0; j < 5; j++)
            {
                skLineList.Add(new SKPoint[]
                {
                        xyPoint[j],xyPoint[j+1]
                });
            }
        }

        void DeleteButtons()
        {
            int currentButtonIndex = Int32.Parse(buttons[buttons.Count - 1].Text);
            int buttonsToDelete = buttons.Count - (buttonBuffer + lastDeletedButton);

            for (int i = lastDeletedButton; i < buttons.Count - buttonBuffer; i++)
            {
                MainAbsoluteLayout.Children.Remove(buttons[i]);
            }

            for (int i = lastDeletedFrame; i < (buttons.Count - buttonBuffer) / 3; i++)
            {
                MainAbsoluteLayout.Children.Remove(frames[i]);
            }

            lastDeletedButton += buttonsToDelete;
            lastDeletedFrame += buttonsToDelete / 3;
        }

        void Draw_RandomShape(SKCanvas skCanvas, object sender)
        {
            var donePoints = new List<SKPoint[]>();
            skLineList = skLineList.Distinct().ToList();
            bool firstDone = false;
            //bool secondDone = false;
            int senderIndex = 1;

            foreach (var elem in skLineList)
            {
                if (!donePoints.Contains(elem))
                {
                    //skCanvas.DrawLine(elem[0].X, elem[0].Y, elem[1].X, elem[1].Y, strokePaint);

                    if ((elem[1].Y - elem[0].Y) > 0 && (elem[1].X - elem[0].X) > 0 && !firstDone)
                    {  //0 -> 1
                        var firstLineElem = new SKPoint[] { new SKPoint(elem[0].X - 5, elem[0].Y), new SKPoint(elem[1].X, elem[1].Y + 5) };
                        var secondLineElem = new SKPoint[] { new SKPoint(elem[0].X + 5, elem[0].Y), new SKPoint(elem[1].X, elem[1].Y - 5) };
                        streetPath.MoveTo(firstLineElem[0]);
                        streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, firstLineElem[1]);
                        streetPath.MoveTo(secondLineElem[0]);
                        streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, secondLineElem[1]);

                        scorePath.MoveTo(elem[0]);
                        scorePath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, elem[1]);

                        firstDone = true;
                        traversedScore += segmentScore;

                        if (traversedScore <= currentScore)
                        {
                            collectedPath.MoveTo(elem[0]);
                            collectedPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, elem[1]);
                            Console.WriteLine(traversedScore + "0-1");
                        }

                    }
                    else if ((elem[1].Y - elem[0].Y) > 0 && (elem[1].X - elem[0].X) > 0 && firstDone)
                    {  //2 -> 3
                        var firstLineElem = new SKPoint[] { new SKPoint(elem[0].X, elem[0].Y + 5), new SKPoint(elem[1].X - 5, elem[1].Y) };
                        var secondLineElem = new SKPoint[] { new SKPoint(elem[0].X, elem[0].Y - 5), new SKPoint(elem[1].X + 5, elem[1].Y) };
                        streetPath.MoveTo(firstLineElem[0]);
                        streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, firstLineElem[1]);
                        streetPath.MoveTo(secondLineElem[0]);
                        streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, secondLineElem[1]);

                        scorePath.MoveTo(elem[0]);
                        scorePath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, elem[1]);

                        firstDone = false;
                        traversedScore += segmentScore;

                        if (traversedScore <= currentScore)
                        {
                            collectedPath.MoveTo(elem[0]);
                            collectedPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, elem[1]);
                            Console.WriteLine(traversedScore + "2-3");
                        }
                    }

                    if ((elem[1].Y - elem[0].Y) < 0 && (elem[1].X - elem[0].X) > 0)
                    {  //1 -> 2
                        var firstLineElem = new SKPoint[] { new SKPoint(elem[0].X, elem[0].Y - 5), new SKPoint(elem[1].X, elem[1].Y - 5) };
                        var secondLineElem = new SKPoint[] { new SKPoint(elem[0].X, elem[0].Y + 5), new SKPoint(elem[1].X, elem[1].Y + 5) };
                        var midpoint1 = new SKPoint((firstLineElem[0].X + firstLineElem[1].X) / 2, ((firstLineElem[0].Y + firstLineElem[1].Y) / 2));
                        var midpoint2 = new SKPoint((secondLineElem[0].X + secondLineElem[1].X) / 2, ((secondLineElem[0].Y + secondLineElem[1].Y) / 2));
                        var midlinemidpoint = new SKPoint((elem[0].X + elem[1].X) / 2, (elem[0].Y + elem[1].Y) / 2);

                        streetPath.MoveTo(firstLineElem[0]);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, midpoint1);
                        streetPath.MoveTo(secondLineElem[0]);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, midpoint2);

                        scorePath.MoveTo(elem[0]);
                        scorePath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, midlinemidpoint);

                        streetPath.MoveTo(midpoint1);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, firstLineElem[1]);
                        streetPath.MoveTo(midpoint2);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, secondLineElem[1]);

                        scorePath.MoveTo(midlinemidpoint);
                        scorePath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, elem[1]);
                        traversedScore += segmentScore;

                        if (traversedScore <= currentScore)
                        {
                            collectedPath.MoveTo(elem[0]);
                            collectedPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, midlinemidpoint);
                            collectedPath.MoveTo(midlinemidpoint);
                            collectedPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, elem[1]);
                            Console.WriteLine(traversedScore + "1-2 1");

                        }
                    }

                    if ((elem[1].Y - elem[0].Y) > 0 && (elem[1].X - elem[0].X) < 0)
                    {  //3 -> 4
                        var firstLineElem = new SKPoint[] { new SKPoint(elem[0].X - 5, elem[0].Y), new SKPoint(elem[1].X, elem[1].Y - 5) };
                        var secondLineElem = new SKPoint[] { new SKPoint(elem[0].X + 5, elem[0].Y), new SKPoint(elem[1].X, elem[1].Y + 5) };
                        streetPath.MoveTo(firstLineElem[0]);
                        streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, firstLineElem[1]);
                        streetPath.MoveTo(secondLineElem[0]);
                        streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, secondLineElem[1]);

                        scorePath.MoveTo(elem[0]);
                        scorePath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, elem[1]);

                        //secondDone = true;
                        traversedScore += segmentScore;

                        if (traversedScore <= currentScore)
                        {
                            collectedPath.MoveTo(elem[0]);
                            collectedPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.Clockwise, elem[1]);
                            Console.WriteLine(traversedScore + "3-4");
                        }
                    }

                    if ((elem[1].Y - elem[0].Y) < 0 && (elem[1].X - elem[0].X) < 0)
                    {  //4 -> 5

                        var firstLineElem = new SKPoint[] { new SKPoint(elem[0].X, elem[0].Y - 5), new SKPoint(elem[1].X, elem[1].Y - 5) };
                        var secondLineElem = new SKPoint[] { new SKPoint(elem[0].X, elem[0].Y + 5), new SKPoint(elem[1].X, elem[1].Y + 5) };
                        var midpoint1 = new SKPoint((firstLineElem[0].X + firstLineElem[1].X) / 2, ((firstLineElem[0].Y + firstLineElem[1].Y) / 2));
                        var midpoint2 = new SKPoint((secondLineElem[0].X + secondLineElem[1].X) / 2, ((secondLineElem[0].Y + secondLineElem[1].Y) / 2));
                        var midlinemidpoint = new SKPoint((elem[0].X + elem[1].X) / 2, (elem[0].Y + elem[1].Y) / 2);

                        streetPath.MoveTo(firstLineElem[0]);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, midpoint1);
                        streetPath.MoveTo(secondLineElem[0]);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, midpoint2);

                        scorePath.MoveTo(elem[0]);
                        scorePath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, midlinemidpoint);

                        streetPath.MoveTo(midpoint1);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, firstLineElem[1]);
                        streetPath.MoveTo(midpoint2);
                        streetPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, secondLineElem[1]);

                        scorePath.MoveTo(midlinemidpoint);
                        scorePath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, elem[1]);
                        traversedScore += segmentScore;

                        if (traversedScore <= currentScore)
                        {
                            collectedPath.MoveTo(elem[0]);
                            collectedPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.Clockwise, midlinemidpoint);
                            collectedPath.MoveTo(midlinemidpoint);
                            collectedPath.ArcTo(new SKPoint(70, 70), 55, SKPathArcSize.Small, SKPathDirection.CounterClockwise, elem[1]);
                            Console.WriteLine(traversedScore + "4-5 1");
                        }

                        try
                        {
                            fivetosix(senderIndex);
                            senderIndex++;
                        }
                        catch { }
                    }
                    donePoints.Add(elem);
                }
                donePoints.Add(elem);
            }

            void fivetosix(int index)
            {
                var thisLine = new SKPoint[] { new SKPoint(lastPoints[index - 1].X, lastPoints[index - 1].Y), new SKPoint(firstPoints[index].X, firstPoints[index].Y) };
                if (!donePoints.Contains(thisLine))
                {
                    var firstLineElem = new SKPoint[] { new SKPoint(lastPoints[index - 1].X, lastPoints[index - 1].Y - 5), new SKPoint(firstPoints[index].X - 5, firstPoints[index].Y) };
                    var secondLineElem = new SKPoint[] { new SKPoint(lastPoints[index - 1].X, lastPoints[index - 1].Y + 5), new SKPoint(firstPoints[index].X + 5, firstPoints[index].Y) };
                    //5->6
                    streetPath.MoveTo(firstLineElem[0]);
                    streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, firstLineElem[1]);
                    streetPath.MoveTo(secondLineElem[0]);
                    streetPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, secondLineElem[1]);

                    scorePath.MoveTo(thisLine[0]);
                    scorePath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, thisLine[1]);
                    traversedScore += segmentScore;

                    if (traversedScore <= currentScore)
                    {
                        collectedPath.MoveTo(thisLine[0]);
                        collectedPath.ArcTo(new SKPoint(100, 100), 45, SKPathArcSize.Small, SKPathDirection.CounterClockwise, thisLine[1]);
                        Console.WriteLine(traversedScore + "5-6");
                    }
                    donePoints.Add(thisLine);
                }
            }

            if (sender == SkCanvasView)
            {
                skCanvas.DrawPath(collectedPath, collectedStroke);
                skCanvas.DrawPath(clipperPath, clipStroke);
            }
            else
            {
                skCanvas.DrawPath(scorePath, scoreStroke);
                skCanvas.DrawPath(streetPath, streetStroke);
            }
        }

        void SkCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas skCanvas = surface.Canvas;

            skCanvas.Clear(SKColors.Transparent);
            var skCanvasWidth = info.Width;
            var skCanvasheight = info.Height;

            skCanvas.Scale(skCanvasWidth / (float)widgetWidth);
            Draw_RandomShape(skCanvas, sender);
        }

        void ScrollEvent(Object sender, ScrolledEventArgs e)
        {
            Console.WriteLine(e.ScrollY.ToString());
            if (e.ScrollY > MainScrollView.ContentSize.Height - MainScrollView.Height - 30)
            {
                //Thread.Sleep(100);
                backgroundStack.Children.Add(new Image()
                {
                    Source = BGImage,
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 350,
                    Margin = new Thickness(0, 0, 0, 0),

                });

                void CallButtonCreator()
                {
                    //Thread.Sleep(100);
                    buttonCreater();
                }
                //buttonCreater(TotalIterations);

                CallButtonCreator();

                SkCanvasView.InvalidateSurface();
                DeleteButtons();
                //SkCanvasView2.InvalidateSurface();
            }
        }
    }
}