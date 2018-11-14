// without shadows draw is much faster, but its fast enought anyway
#define USE_SHADOW

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace ColorDialogEx
{
    internal class ColorDialogExLogic
    {
        /// <summary>
        /// Color is changed by user
        /// </summary>
        public event EventHandler<ColorEventArgs> ColorChanged;

        protected virtual void OnColorChanged(ColorEventArgs e)
        {
            EventHandler<ColorEventArgs> handler = ColorChanged;
            handler?.Invoke(this, e);
        }

        private Canvas HostingCanvas;

        #region Color things
        // ------------------------------------------
        /// <summary>
        /// Get or set Hue value
        /// </summary>
        public double H
        {
            get
            {
                return Math.Round(_h, 2);
            }
            set
            {
                if (Math.Abs(_h - value) > 0.00001)
                {
                    _h = value;
                    Move_HexDirector(value, HexagonPointCenter);

                    ColorEventArgs e = new ColorEventArgs();
                    e.ColorRGB = ColorRGB;
                    e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                    OnColorChanged(e);
                }
            }
        }

        /// <summary>
        /// Get or set Saturation value
        /// </summary>
        public double S
        {
            get
            {
                return Math.Round(_s, 4);
            }
            set
            {
                if (Math.Abs(_s - value) > 0.00001)
                {
                    _s = value;
                    Move_SatDirector(value);

                    ColorEventArgs e = new ColorEventArgs();
                    e.ColorRGB = ColorRGB;
                    e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                    OnColorChanged(e);
                }
            }
        }

        /// <summary>
        /// Get or set Lightness value
        /// </summary>
        public double L
        {
            get
            {
                return Math.Round(_l, 4);
            }
            set
            {
                if (Math.Abs(_l - value) > 0.00001)
                {
                    _l = value;
                    Move_TriColorMark(value);
                    Redraw_TriangleMarkers();
                    Redraw_Alphator();

                    ColorEventArgs e = new ColorEventArgs();
                    e.ColorRGB = ColorRGB;
                    e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                    OnColorChanged(e);
                }
            }
        }

        /// <summary>
        /// Get or set Alpha value
        /// </summary>
        public byte A
        {
            get
            {
                return _a;
            }
            set
            {
                if (_a != value)
                {
                    _a = value;
                    Move_AlphDirector(value);

                    ColorEventArgs e = new ColorEventArgs();
                    e.ColorRGB = ColorRGB;
                    e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                    OnColorChanged(e);
                }
            }
        }

        /// <summary>
        /// Get or set color value w/o specific Alpha
        /// </summary>
        public Color ColorRGB
        {
            get
            {
                Color color = HSLConverter.HslToRgb(new HSL(H, S, L));
                return Color.FromArgb(255, color.R, color.G, color.B);
            }
            set
            {
                HSL hsl = HSLConverter.RgbToHsl(value);
                _h = hsl.H;
                _s = hsl.S;
                _l = hsl.L;
                _a = 255;

                Move_HexDirector(_h, HexagonPointCenter);
                Move_SatDirector(_s);
                Move_TriColorMark(_l);
                Redraw_TriangleMarkers();
                Redraw_Alphator();
                Move_AlphDirector(_a);

                ColorEventArgs e = new ColorEventArgs();
                e.ColorRGB = value;
                e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                OnColorChanged(e);
            }
        }

        /// <summary>
        /// Get or set color value with specific Alpha
        /// </summary>
        public Color ColorARGB
        {
            get
            {
                Color color = HSLConverter.HslToRgb(new HSL(H, S, L));
                return Color.FromArgb(A, color.R, color.G, color.B);
            }
            set
            {
                HSL hsl = HSLConverter.RgbToHsl(value);
                _h = hsl.H;
                _s = hsl.S;
                _l = hsl.L;
                _a = value.A;

                Move_HexDirector(_h, HexagonPointCenter);
                Move_SatDirector(_s);
                Move_TriColorMark(_l);
                Redraw_TriangleMarkers();
                Redraw_Alphator();
                Move_AlphDirector(_a);

                ColorEventArgs e = new ColorEventArgs();
                e.ColorRGB = value;
                e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                OnColorChanged(e);
            }
        }

        /// <summary>
        /// Hue
        /// </summary>
		private double _h = 0;

        /// <summary>
        /// Lightness
        /// </summary>
		private double _l = 0.5;

        /// <summary>
        /// Saturation
        /// </summary>
		private double _s = 1;

        /// <summary>
        /// Alpha
        /// </summary>
		private byte _a = 255;
        // ------------------------------------------
        #endregion


        #region Hexagon things
        // ------------------------------------------
        private PolyLineSegment HexaginDirectorLine; // director line inside hexagon
        private Point HexagonPointCenter; // center point
        private Point HexagonPoint1,
                        HexagonPoint2,
                        HexagonPoint3,
                        HexagonPoint4,
                        HexagonPoint5,
                        HexagonPoint6; // hexagon points
        private Path HexagonPath; // hexagon graphical object
        private double HexagonInnerRadius; // radius of inner circle

        private Path HexagonMarkPath; // small triangle markers inside hexagon
        private Point HexagonDirectorPoint = new Point(0, 0); // target direction of line
                                                              // ------------------------------------------
        #endregion


        #region Triangle things
        // ------------------------------------------
        private Canvas TriCanvas; // canvas for main triangle and markers
        private RotateTransform TriCanvasTrans; // rotate transformation for triangle canvas

        private Point TriPnt1, TriPnt2, TriPnt3; // main triangle points
        private PathFigure TriFigure; // cashed figure for markers creation

        private Path TriPath;  // main triangle graphicsl object
        private double TriSide;  // lenght of the side of main triangle
        private double TriHeight; // height of main triangle
        private ImageBrush TriIBrush; // background brush for main triangle
        private LinearGradientBrush TriBrushAlt; // alternative draw background brush for main triangle

        private Path TriMarkPath1; // object of triangle marker 1
        private Path TriMarkPath2; // object of triangle marker 2
        private Path TriMarkPath3; // object of triangle marker 3
        private Path TriMarkPath4; // object of triangle marker 4
        private Path TriMarkPath5; // object of triangle marker 5

        private Path TriColorMark; // marker of color inside triangle
        private TranslateTransform TriColorMarkTrans; // transformation for color markers

        private TranslateTransform TriMarkTrans1, TriMarkTrans2, TriMarkTrans3, TriMarkTrans4, TriMarkTrans5; // for small 3 markers of color triangle
                                                                                                              // ------------------------------------------
        #endregion


        #region Alpha and Saturation things
        // ------------------------------------------
        private Path SaturatorPath; // saturator rectangle
        private Point SaturatorPnt1; // saturation rect point 1
        private Point SaturatorPnt2; // saturation rect point 1
        private LinearGradientBrush SaturatorBrush; // saturator background

        private Path SaturatorDirectorPath; // line for saturator
        private TranslateTransform SaturatorDirectorTranslation; // translation for saturation director

        private Path AlphaPath; // alphator rectangle
        private Path AlphaPathBackground; // background alphator rectangle
        private Point AlphaPnt1; // alphator rect point 1
        private Point AlphaPnt2; // alphator rect point 1

        private Path AlphaDirectorPath; // line for alphator
        private TranslateTransform AlphaDirectorTranslation; // translation for alpha director

        private LinearGradientBrush AlphaBrush; // alphator background
        private ImageBrush AlphaBrushBackground; // for background alphator
                                                 // ------------------------------------------
        #endregion


        public ColorDialogExLogic(Canvas HostingCanvas)
        {
            this.HostingCanvas = HostingCanvas;
            Recreate_All();
        }

        public ColorDialogExLogic(Canvas HostingCanvas, Color ColorRGB)
        {
            this.HostingCanvas = HostingCanvas;
            Recreate_All();
            ColorARGB = ColorRGB;
        }

        public void RefreshResize()
        {
            Recreate_All();
        }

        private void Recreate_All()
        {
            if (HostingCanvas.ActualWidth < 16 || HostingCanvas.ActualHeight < 16)
                return;

            HostingCanvas.SnapsToDevicePixels = false;

            //---------------------------------begin new ---------------------
            HostingCanvas.Children.Clear();

            //---------------------------------base points for hexagon -------
            Recreate_BasePoints(HostingCanvas);

            //---------------------------------hexagon + director line --------
            Recreate_Hexagon(HostingCanvas);

            //---------------------------------triangle -----------------------
            Recreate_Triangle(HostingCanvas);

            //---------------------------------hexagon markers ----------------
            Recreate_HexMarkers();

            //---------------------------------color marker -------------------
            Recreate_ColorMarker();

            //---------------------------------triangle markers ---------------
            Recreate_TriangleMarkers();

            //---------------------------------saturation edit ----------------
            Recreate_Saturator(HostingCanvas);

            //---------------------------------alpha channel edit -------------
            Recreate_Alphator(HostingCanvas);

            //---------------------------------initialize positions -----------
            Move_HexDirector(_h, HexagonPointCenter);
            Move_TriColorMark(_l);
            Move_SatDirector(_s);
            Move_AlphDirector(_a);

            //---------------------------------hexagon background redraw ------
            Redraw_Hexagon(HostingCanvas);

            //---------------------------------triangle background redraw -----
            Redraw_TriangleAlt();

            //---------------------------------markers redraw -----------------
            Redraw_TriangleMarkers();

            //---------------------------------saturation edit redraw ---------
            Redraw_Saturator();

            //---------------------------------alpha edit redraw --------------
            Redraw_AlphatorBack();
            Redraw_Alphator();

            //---------------------------------hexagon events -----------------
            #region Hexagon events

            bool mouseGearCaptured = false;

            // we don't care about memory leaks via event subscriptions
            // HexagonPath is new()
            HexagonPath.MouseDown += (o, args) =>
                        {
                            HexagonPath.CaptureMouse();
                            mouseGearCaptured = true;
                            args.Handled = true;

                            Point mousePoint = args.GetPosition(HexagonPath);
                            _h = GetAngle(mousePoint, HexagonPointCenter);
                            Move_HexDirector(mousePoint, HexagonInnerRadius / 3);

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };
            HexagonPath.MouseUp += (o, args) =>
                        {
                            HexagonPath.ReleaseMouseCapture();
                            mouseGearCaptured = false;
                            args.Handled = true;
                        };
            HexagonPath.MouseMove += (o, args) =>
                        {
                            if (!mouseGearCaptured) return;
                            args.Handled = true;

                            Point mousePoint = args.GetPosition(HexagonPath);
                            _h = GetAngle(mousePoint, HexagonPointCenter);
                            Move_HexDirector(mousePoint, HexagonInnerRadius / 3);

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };

            #endregion

            //---------------------------------hexagon markers events ---------
            #region Hexagon markers events

            HexagonMarkPath.MouseDown += (o, args) =>
                        {
                            Point pos = args.GetPosition(TriCanvas);

                            double a = GetAngle(pos, new Point(TriCanvas.Width / 2d, TriCanvas.Height / 2d)) - 90;
                            double anew = Math.Round(a / 30d) * 30;
                            double canvasAngle = H;

                            anew = canvasAngle + anew;
                            H = anew;
                        };

            #endregion

            //---------------------------------triangle markers events --------
            #region Triangle markers events

            TriMarkPath1.MouseDown += (o, args) =>
                        {
                            double x = TriMarkTrans1.X;
                            double p = (x + TriSide / 2d) / TriSide;
                            L = p;
                        };
            TriMarkPath2.MouseDown += (o, args) =>
                        {
                            double x = TriMarkTrans2.X;
                            double p = (x + TriSide / 2d) / TriSide;
                            L = p;
                        };
            TriMarkPath3.MouseDown += (o, args) =>
                        {
                            double x = TriMarkTrans3.X;
                            double p = (x + TriSide / 2d) / TriSide;
                            L = p;
                        };
            TriMarkPath4.MouseDown += (o, args) =>
                        {
                            double x = TriMarkTrans4.X;
                            double p = (x + TriSide / 2d) / TriSide;
                            L = p;
                        };
            TriMarkPath5.MouseDown += (o, args) =>
                        {
                            double x = TriMarkTrans5.X;
                            double p = (x + TriSide / 2d) / TriSide;
                            L = p;
                        };

            #endregion


            //---------------------------------triangle events ----------------
            #region Triangle events

            bool mouseTriangleCaptured = false;

            TriPath.MouseDown += (o, args) =>
                        {
                            TriPath.CaptureMouse();
                            mouseTriangleCaptured = true;
                            args.Handled = true;


                            Point mousePoint = args.GetPosition(TriCanvas);
                            Move_TriColorMark(mousePoint);

                            Redraw_TriangleMarkers();
                            Redraw_Alphator();

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };

            TriPath.MouseUp += (o, args) =>
                        {
                            TriPath.ReleaseMouseCapture();
                            mouseTriangleCaptured = false;
                            args.Handled = true;
                        };

            TriPath.MouseMove += (o, args) =>
                        {
                            if (!mouseTriangleCaptured)
                                return;

                            Point mousePoint = args.GetPosition(TriCanvas);
                            Move_TriColorMark(mousePoint);

                            Redraw_TriangleMarkers();
                            Redraw_Alphator();

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };

            TriPath.MouseWheel += (o, args) =>
                        {

                            double a = GetAngle(HexaginDirectorLine.Points[0], HexagonPointCenter);
                            double anew = args.Delta > 0 ? a + 30 : a - 30;
                            H = anew;
                        };

            #endregion


            //---------------------------------saturator events ---------------
            #region Saturator events

            bool mouseSatCaptured = false;

            SaturatorPath.MouseDown += (o, args) =>
                        {
                            SaturatorPath.CaptureMouse();
                            mouseSatCaptured = true;
                            args.Handled = true;

                            Point mousePoint = args.GetPosition(HostingCanvas);
                            Move_SatDirector(mousePoint);

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };
            SaturatorPath.MouseUp += (o, args) =>
                        {
                            SaturatorPath.ReleaseMouseCapture();
                            mouseSatCaptured = false;
                            args.Handled = true;
                        };
            SaturatorPath.MouseMove += (o, args) =>
                        {
                            if (!mouseSatCaptured) return;
                            args.Handled = true;

                            Point mousePoint = args.GetPosition(HostingCanvas);
                            Move_SatDirector(mousePoint);

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };

            #endregion


            //---------------------------------alphator events ---------------
            #region Alphator events

            bool mouseAlphCaptured = false;

            AlphaPath.MouseDown += (o, args) =>
                        {
                            AlphaPath.CaptureMouse();
                            mouseAlphCaptured = true;
                            args.Handled = true;

                            Point mousePoint = args.GetPosition(HostingCanvas);
                            Move_AlphDirector(mousePoint);

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };
            AlphaPath.MouseUp += (o, args) =>
                        {
                            AlphaPath.ReleaseMouseCapture();
                            mouseAlphCaptured = false;
                            args.Handled = true;
                        };
            AlphaPath.MouseMove += (o, args) =>
                        {
                            if (!mouseAlphCaptured) return;
                            args.Handled = true;

                            Point mousePoint = args.GetPosition(HostingCanvas);
                            Move_AlphDirector(mousePoint);

                            ColorEventArgs e = new ColorEventArgs();
                            e.ColorRGB = ColorRGB;
                            e.ColorARGB = Color.FromArgb(A, e.ColorRGB.R, e.ColorRGB.G, e.ColorRGB.B);
                            OnColorChanged(e);
                        };

            #endregion


            //---------------------------------shadow FX ---------------------
#if USE_SHADOW
            DropShadowEffect shadowEffect = new DropShadowEffect();
            shadowEffect.ShadowDepth = 2;
            shadowEffect.BlurRadius = 4;
            shadowEffect.Opacity = 0.4;

            HexagonPath.Effect = shadowEffect;
            TriPath.Effect = shadowEffect;
            SaturatorPath.Effect = shadowEffect;
            AlphaPath.Effect = shadowEffect;
#endif
        }

        /// <summary>
        /// Calculate base points for every future calculations and figures creation
        /// </summary>
        /// <param name="hoster">UIElement where to draw</param>
        private void Recreate_BasePoints(Canvas hoster)
        {
            double radius = (int)Math.Min(hoster.ActualWidth / 2d, hoster.ActualHeight / 2d - hoster.ActualHeight * 0.04);
            double h1 = (int)((Math.Sqrt(3) / 2d) * radius);
            HexagonInnerRadius = (int)(h1 - (h1 * 0.06));

            HexagonPointCenter = new Point((int)(hoster.ActualWidth / 2d), (int)(hoster.ActualHeight / 2d));

            HexagonPoint1 = new Point(HexagonPointCenter.X + radius, HexagonPointCenter.Y);
            HexagonPoint2 = new Point((int)(HexagonPointCenter.X + radius / 2d), HexagonPointCenter.Y - h1);
            HexagonPoint3 = new Point((int)(HexagonPointCenter.X - radius / 2d), HexagonPointCenter.Y - h1);
            HexagonPoint4 = new Point(HexagonPointCenter.X - radius, HexagonPointCenter.Y);
            HexagonPoint5 = new Point((int)(HexagonPointCenter.X - radius / 2d), HexagonPointCenter.Y + h1);
            HexagonPoint6 = new Point((int)(HexagonPointCenter.X + radius / 2d), HexagonPointCenter.Y + h1);
        }

        /// <summary>
        /// Recreate main hexagon as Hue-wheel color selection
        /// </summary>
        private void Recreate_Hexagon(Canvas hoster)
        {
            //--------- HEX  ----------------------------------------------------
            PathFigure hexF = new PathFigure();
            hexF.StartPoint = HexagonPoint1;
            hexF.Segments.Add(new PolyLineSegment(new[] { HexagonPoint2, HexagonPoint3, HexagonPoint4, HexagonPoint5, HexagonPoint6, HexagonPoint1 }, true));
            hexF.IsClosed = true;

            PathGeometry hexG = new PathGeometry();
            hexG.Figures.Add(hexF);

            Path hexPath = new Path();
            hexPath.Data = hexG;
            hexPath.StrokeThickness = 2;
            hexPath.Stroke = new SolidColorBrush(Colors.Black);
            hexPath.Fill = new SolidColorBrush(Colors.Chocolate);
            //hoster.Children.Add(hexPath); /* Uncomment for visual test */


            //--------- Ellipse ----------------------------------------------------
            EllipseGeometry elG = new EllipseGeometry(new Point(HexagonPointCenter.X, HexagonPointCenter.Y), HexagonInnerRadius, HexagonInnerRadius);
            Path elPath = new Path();
            elPath.Data = elG;
            elPath.StrokeThickness = 2;
            elPath.Stroke = new SolidColorBrush(Colors.Black);
            elPath.Fill = new SolidColorBrush(Colors.Chocolate);
            //hoster.Children.Add(elPath); /* Uncomment for visual test */


            //--------- Combined Hex ----------------------------------------------------
            HexagonPath = new Path();
            HexagonPath.Data = new CombinedGeometry(GeometryCombineMode.Exclude, hexPath.Data, elPath.Data);
            HexagonPath.StrokeThickness = 0.5;
            HexagonPath.Stroke = new SolidColorBrush(Colors.Gray);
            //GearPath.Fill = new SolidColorBrush(Colors.Chocolate); /* Uncomment for visual test */

            hoster.Children.Add(HexagonPath);
            HexagonPath.SnapsToDevicePixels = true;


            //--------- Hex director Line  -----------------------------------------------
            Path originalLinePath = new Path();
            if (Math.Abs(HexagonDirectorPoint.X) < 0.00001 && Math.Abs(HexagonDirectorPoint.Y) < 0.00001)
                HexagonDirectorPoint = new Point(HexagonPointCenter.X, 0);
            HexaginDirectorLine = new PolyLineSegment(new[] { HexagonDirectorPoint, new Point(HexagonPointCenter.X + 0.1, HexagonPointCenter.Y + 0.1) }, true);
            PathGeometry lineG = new PathGeometry(new[]
            {
                new PathFigure(HexagonPointCenter, new[]
                {
                    HexaginDirectorLine
                },
                true)
            });
            originalLinePath.Data = lineG;
            originalLinePath.StrokeThickness = 1;
            originalLinePath.Stroke = new SolidColorBrush(Colors.Black);
            //hoster.Children.Add(originalLinePath); /* Uncomment for visual test */


            //--------- Combined Hex director Line  ---------------------------------------
            Path gearLinePath = new Path();
            gearLinePath.Data = new CombinedGeometry(GeometryCombineMode.Exclude, originalLinePath.Data, elPath.Data);
            gearLinePath.Data = new CombinedGeometry(GeometryCombineMode.Intersect, gearLinePath.Data, hexPath.Data);
            gearLinePath.StrokeThickness = 0.5;
            gearLinePath.Stroke = new SolidColorBrush(Colors.Black);
            hoster.Children.Add(gearLinePath);
            gearLinePath.SnapsToDevicePixels = false;
        }

        /// <summary>
        /// Recreate main triangle inside hexagon
        /// </summary>
        private void Recreate_Triangle(Canvas hoster)
        {
            TriSide = HexagonInnerRadius / (Math.Sqrt(3d) / 3d);
            TriHeight = (Math.Sqrt(3d) / 2d) * TriSide;

            TriCanvas = new Canvas(); // background canvas for main triangle
            TriCanvas.Width = HexagonInnerRadius * 2;
            TriCanvas.Height = HexagonInnerRadius * 2;

            // 3 points of main triangle
            TriPnt1 = new Point(HexagonInnerRadius, 0);
            TriPnt2 = new Point(TriPnt1.X - TriSide / 2d, TriHeight);
            TriPnt3 = new Point(TriPnt1.X + TriSide / 2d, TriHeight);

            TriFigure = new PathFigure();
            TriFigure.StartPoint = TriPnt1;
            TriFigure.Segments.Add(new PolyLineSegment(new[] { TriPnt2, TriPnt3, TriPnt1 }, true));
            TriFigure.IsClosed = true;

            PathGeometry triGeometry = new PathGeometry();
            triGeometry.Figures.Add(TriFigure);

            TriPath = new Path();
            TriPath.Data = triGeometry;
            TriPath.StrokeThickness = 0.5;
            TriPath.Stroke = new SolidColorBrush(Colors.Gray);

            TriBrushAlt = new LinearGradientBrush(); // alternative- Redraw_TriangleAlt()

            TriIBrush = new ImageBrush();
            TriIBrush.Stretch = Stretch.None;
            TriIBrush.TileMode = TileMode.Tile;
            TriPath.Fill = TriIBrush;
            //hoster.Background = triIB;  /* Uncomment for visual test */

            TriCanvas.Children.Add(TriPath);
            hoster.Children.Add(TriCanvas);
            //TriCanvas.Background = new SolidColorBrush(Colors.Red);  /* Uncomment for visual test */

            TriPath.SnapsToDevicePixels = false;

            TranslateTransform center = new TranslateTransform(HexagonPointCenter.X - TriCanvas.Width / 2d, HexagonPointCenter.Y - TriCanvas.Height / 2d);
            TriCanvas.LayoutTransform = center;

            TriCanvasTrans = new RotateTransform();
            TriCanvasTrans.Angle = GetAngle(HexagonDirectorPoint, HexagonPointCenter) * -1 + 90;
            TriCanvasTrans.CenterX = TriCanvas.Width / 2d;
            TriCanvasTrans.CenterY = TriCanvas.Height / 2d;

            TransformGroup group = new TransformGroup();
            group.Children.Add(TriCanvasTrans);
            group.Children.Add(center);

            TriCanvas.RenderTransform = group;
        }

        /// <summary>
        /// Recreate markers inside hexagon
        /// </summary>
        private void Recreate_HexMarkers()
        {
            PathGeometry markG1 = new PathGeometry();
            markG1.Figures.Add(TriFigure);
            RotateTransform t1 = new RotateTransform(30, TriCanvas.Width / 2d, TriCanvas.Height / 2d);
            markG1.Transform = t1;

            PathGeometry markG2 = new PathGeometry();
            markG2.Figures.Add(TriFigure);
            RotateTransform t2 = new RotateTransform(60, TriCanvas.Width / 2d, TriCanvas.Height / 2d);
            markG2.Transform = t2;

            PathGeometry markG3 = new PathGeometry();
            markG3.Figures.Add(TriFigure);
            RotateTransform t3 = new RotateTransform(90, TriCanvas.Width / 2d, TriCanvas.Height / 2d);
            markG3.Transform = t3;

            Path markPath1 = new Path();
            markPath1.Data = markG1;
            markPath1.StrokeThickness = 1;
            markPath1.Stroke = Brushes.Black;
            //hoster.Children.Add(markPath1); /* Uncomment for visual test */

            Path markPath2 = new Path();
            markPath2.Data = markG2;
            markPath2.StrokeThickness = 1;
            markPath2.Stroke = Brushes.Black;
            //hoster.Children.Add(markPath2); /* Uncomment for visual test */

            Path markPath3 = new Path();
            markPath3.Data = markG3;
            markPath3.StrokeThickness = 1;
            markPath3.Stroke = Brushes.Black;
            //hoster.Children.Add(markPath3); /* Uncomment for visual test */

            EllipseGeometry triElG = new EllipseGeometry(new Point(TriCanvas.Width / 2d, TriCanvas.Height / 2d),
                                                         HexagonInnerRadius - (HexagonInnerRadius * 0.08),
                                                         HexagonInnerRadius - (HexagonInnerRadius * 0.08));
            Path triElPath = new Path();
            triElPath.Data = triElG;
            triElPath.StrokeThickness = 1;

            HexagonMarkPath = new Path();
            HexagonMarkPath.Data = new CombinedGeometry(GeometryCombineMode.Union, markPath1.Data, markPath2.Data);
            HexagonMarkPath.Data = new CombinedGeometry(GeometryCombineMode.Union, HexagonMarkPath.Data, markPath3.Data);
            HexagonMarkPath.Data = new CombinedGeometry(GeometryCombineMode.Exclude, HexagonMarkPath.Data, triElPath.Data);
            HexagonMarkPath.StrokeThickness = 0.5;
            HexagonMarkPath.Stroke = Brushes.DarkGray;
            HexagonMarkPath.Fill = Brushes.LightGray;

            TriCanvas.Children.Add(HexagonMarkPath);
            HexagonMarkPath.SnapsToDevicePixels = true;
        }

        /// <summary>
        /// Recreate 3 markers at bottom of main triangle
        /// </summary>
        private void Recreate_TriangleMarkers()
        {
            double triMarkHeight = (HexagonInnerRadius * 0.08);
            double triMarkSide = triMarkHeight / (Math.Sqrt(3d) / 2d);

            Point triMarkPnt1 = new Point(TriPnt1.X, TriHeight);
            Point triMarkPnt2 = new Point(TriPnt1.X - triMarkSide / 2d, triMarkPnt1.Y + triMarkHeight);
            Point triMarkPnt3 = new Point(TriPnt1.X + triMarkSide / 2d, triMarkPnt1.Y + triMarkHeight);

            PathGeometry triMark1G = new PathGeometry();
            triMark1G.Figures.Add(new PathFigure(triMarkPnt1, 
                                                 new PathSegment[]
                                                 {
                                                     new PolyLineSegment(new[] {triMarkPnt2, triMarkPnt3, triMarkPnt1}, true)
                                                 }, 
                                                 true));

            PathGeometry triMark2G = new PathGeometry();
            triMark2G.Figures.Add(new PathFigure(triMarkPnt1,
                                                 new PathSegment[] 
                                                 {
                                                     new PolyLineSegment(new[] { triMarkPnt2, triMarkPnt3, triMarkPnt1 }, true)
                                                 }, 
                                                 true));

            PathGeometry triMark3G = new PathGeometry();
            triMark3G.Figures.Add(new PathFigure(triMarkPnt1,
                                                 new PathSegment[] 
                                                 {
                                                     new PolyLineSegment(new[] { triMarkPnt2, triMarkPnt3, triMarkPnt1 }, true)
                                                 }, 
                                                 true));

            PathGeometry triMark4G = new PathGeometry();
            triMark4G.Figures.Add(new PathFigure(triMarkPnt1,
                                                 new PathSegment[] 
                                                 {
                                                     new PolyLineSegment(new[] { triMarkPnt2, triMarkPnt3, triMarkPnt1 }, true)
                                                 }, 
                                                 true));

            PathGeometry triMark5G = new PathGeometry();
            triMark5G.Figures.Add(new PathFigure(triMarkPnt1,
                                                 new PathSegment[] 
                                                 {
                                                     new PolyLineSegment(new[] { triMarkPnt2, triMarkPnt3, triMarkPnt1 }, true)
                                                 }, 
                                                 true));

            TriMarkPath1 = new Path();
            TriMarkPath1.Data = triMark1G;
            TriMarkPath1.StrokeThickness = 0.5;
            TriMarkPath1.Stroke = Brushes.DarkGray;
            TriMarkPath1.Fill = Brushes.LightGray;

            TriMarkPath2 = new Path();
            TriMarkPath2.Data = triMark1G;
            TriMarkPath2.StrokeThickness = 0.5;
            TriMarkPath2.Stroke = Brushes.DarkGray;
            TriMarkPath2.Fill = Brushes.LightGray;

            TriMarkPath3 = new Path();
            TriMarkPath3.Data = triMark1G;
            TriMarkPath3.StrokeThickness = 0.5;
            TriMarkPath3.Stroke = Brushes.DarkGray;
            TriMarkPath3.Fill = Brushes.LightGray;

            TriMarkPath4 = new Path();
            TriMarkPath4.Data = triMark1G;
            TriMarkPath4.StrokeThickness = 0.5;
            TriMarkPath4.Stroke = Brushes.DarkGray;
            TriMarkPath4.Fill = Brushes.LightGray;

            TriMarkPath5 = new Path();
            TriMarkPath5.Data = triMark1G;
            TriMarkPath5.StrokeThickness = 0.5;
            TriMarkPath5.Stroke = Brushes.DarkGray;
            TriMarkPath5.Fill = Brushes.LightGray;

            // --------- 1/5 -----------
            TriMarkTrans1 = new TranslateTransform(0, 0);
            TriMarkPath1.RenderTransform = TriMarkTrans1;

            TriMarkTrans2 = new TranslateTransform(0, 0);
            TriMarkPath2.RenderTransform = TriMarkTrans2;

            TriMarkTrans3 = new TranslateTransform(0, 0);
            TriMarkPath3.RenderTransform = TriMarkTrans3;

            TriMarkTrans4 = new TranslateTransform(0, 0);
            TriMarkPath4.RenderTransform = TriMarkTrans4;

            TriMarkTrans5 = new TranslateTransform(0, 0);
            TriMarkPath5.RenderTransform = TriMarkTrans5;

            TriCanvas.Children.Add(TriMarkPath1);
            TriCanvas.Children.Add(TriMarkPath2);
            TriCanvas.Children.Add(TriMarkPath3);
            TriCanvas.Children.Add(TriMarkPath4);
            TriCanvas.Children.Add(TriMarkPath5);

            TriMarkPath1.SnapsToDevicePixels = true;
            TriMarkPath2.SnapsToDevicePixels = true;
            TriMarkPath3.SnapsToDevicePixels = true;
            TriMarkPath4.SnapsToDevicePixels = true;
            TriMarkPath5.SnapsToDevicePixels = true;
        }

        /// <summary>
        /// Recreate hexagon markers inside hexagon
        /// </summary>
        private void Recreate_ColorMarker()
        {
            EllipseGeometry geometry1 = new EllipseGeometry(new Point(TriCanvas.Width / 2d, TriCanvas.Height / 2d), 4, 4);
            EllipseGeometry geometry2 = new EllipseGeometry(new Point(TriCanvas.Width / 2d, TriCanvas.Height / 2d), 2, 2);
            CombinedGeometry combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, geometry1, geometry2);
            TriColorMark = new Path();
            TriColorMark.Data = combinedGeometry;
            TriColorMark.Fill = new SolidColorBrush(Colors.Black);
            TriCanvas.Children.Add(TriColorMark);

            TriColorMarkTrans = new TranslateTransform();
            TriColorMark.RenderTransform = TriColorMarkTrans;
        }

        /// <summary>
        /// Recreate saturation rectangle
        /// </summary>
        private void Recreate_Saturator(Canvas hoster)
        {
            // possible bounds
            double height = (HexagonPoint1.X - HexagonPointCenter.X) - (HexagonPoint6.Y - HexagonPointCenter.Y);
            SaturatorPnt1 = new Point((int)(HexagonPointCenter.X - HexagonInnerRadius), (int)(HexagonPoint6.Y + height * 0.55));
            SaturatorPnt2 = new Point((int)(HexagonPointCenter.X + HexagonInnerRadius), (int)(HexagonPoint6.Y + height * 1.25));

            // geometry
            RectangleGeometry saturatorGeometry = new RectangleGeometry(new Rect(SaturatorPnt1, SaturatorPnt2));
            SaturatorPath = new Path();
            SaturatorPath.Data = saturatorGeometry;
            SaturatorPath.StrokeThickness = 0.5;
            SaturatorPath.Stroke = new SolidColorBrush(Colors.Gray);

            SaturatorBrush = new LinearGradientBrush();
            SaturatorPath.Fill = SaturatorBrush;

            hoster.Children.Add(SaturatorPath);
            SaturatorPath.SnapsToDevicePixels = true;

            // line
            LineGeometry saturatorLineGeometry = new LineGeometry(new Point(SaturatorPnt1.X, SaturatorPnt1.Y - 2), new Point(SaturatorPnt1.X, SaturatorPnt2.Y + 2));
            SaturatorDirectorPath = new Path();
            SaturatorDirectorPath.Data = saturatorLineGeometry;
            SaturatorDirectorPath.Stroke = new SolidColorBrush(Colors.Black);
            SaturatorDirectorPath.StrokeThickness = 1;
            hoster.Children.Add(SaturatorDirectorPath);
            SaturatorDirectorPath.SnapsToDevicePixels = true;

            SaturatorDirectorTranslation = new TranslateTransform(SaturatorPnt2.X - SaturatorPnt1.X - 1, 0);
            SaturatorDirectorPath.RenderTransform = SaturatorDirectorTranslation;
        }

        /// <summary>
        /// Recreate alphator rectangle
        /// </summary>
        private void Recreate_Alphator(Canvas hoster)
        {
            // possible bounds
            double height = (HexagonPoint1.X - HexagonPointCenter.X) - (HexagonPointCenter.Y - HexagonPoint3.Y);
            AlphaPnt1 = new Point((int)(HexagonPointCenter.X - HexagonInnerRadius), (int)(HexagonPoint3.Y - height * 1.2));
            AlphaPnt2 = new Point((int)(HexagonPointCenter.X + HexagonInnerRadius), (int)(HexagonPoint3.Y - height * 0.6));

            // geometry
            RectangleGeometry alphaPathData = new RectangleGeometry(new Rect(AlphaPnt1, AlphaPnt2));
            AlphaPath = new Path();
            AlphaPath.Data = alphaPathData;
            AlphaPath.StrokeThickness = 0.5;
            AlphaPath.Stroke = new SolidColorBrush(Colors.Gray);

            AlphaBrush = new LinearGradientBrush();
            AlphaPath.Fill = AlphaBrush;

            // background with checkmate
            RectangleGeometry alphaBackground = new RectangleGeometry(new Rect(AlphaPnt1, AlphaPnt2));
            AlphaPathBackground = new Path();
            AlphaBrushBackground = new ImageBrush();
            AlphaPathBackground.Data = alphaBackground;
            AlphaPathBackground.Fill = AlphaBrushBackground;

            // add background object first, then alpha-filled on top
            hoster.Children.Add(AlphaPathBackground);
            hoster.Children.Add(AlphaPath);
            AlphaPathBackground.SnapsToDevicePixels = true;
            AlphaPath.SnapsToDevicePixels = true;

            // line
            LineGeometry alphLineGeometry = new LineGeometry(new Point(AlphaPnt1.X, AlphaPnt1.Y - 2), new Point(AlphaPnt1.X, AlphaPnt2.Y + 2));
            AlphaDirectorPath = new Path();
            AlphaDirectorPath.Data = alphLineGeometry;
            AlphaDirectorPath.Stroke = new SolidColorBrush(Colors.Black);
            AlphaDirectorPath.StrokeThickness = 1;
            hoster.Children.Add(AlphaDirectorPath);
            AlphaDirectorPath.SnapsToDevicePixels = true;

            AlphaDirectorTranslation = new TranslateTransform(AlphaPnt2.X - AlphaPnt1.X - 1, 0);
            AlphaDirectorPath.RenderTransform = AlphaDirectorTranslation;
        }

        /// <summary>
        /// Redraw main element - color wheel hexagon
        /// </summary>
        /// <param name="hoster"></param>
        private void Redraw_Hexagon(Canvas hoster)
        {
            if (Math.Abs(hoster.ActualWidth) < 0.00001)
                return;

            WriteableBitmap writeableBitmap = new WriteableBitmap((int)hoster.ActualWidth, (int)hoster.ActualHeight, 96, 96,
                                                     PixelFormats.Pbgra32, null);

            uint[] pixels = new uint[(writeableBitmap.PixelWidth * writeableBitmap.PixelHeight)];

            for (int x = 0; x < writeableBitmap.PixelWidth; ++x)
            {
                for (int y = 0; y < writeableBitmap.PixelHeight; ++y)
                {
                    int red, green, blue;
                    const int alpha = 255;

                    int i = (writeableBitmap.PixelWidth * y + x);

                    // general method, but its a wery-wery slow!!
                    //if (GearPath.Data.FillContains(new Point(x, y)))

                    // this custom method is much faster!!
                    if (!PointInCircle(new Point(x, y), HexagonInnerRadius - 1) && PointInHexagon(new Point(x, y)))
                    {
                        double hexagonAngle = GetAngle(new Point(x, y), HexagonPointCenter);

                        Color rgb = HSLConverter.HslToRgb(new HSL(hexagonAngle, 1, 0.5));
                        red = rgb.R;
                        green = rgb.G;
                        blue = rgb.B;
                    }
                    else
                    {
                        red = green = blue = 0;
                    }

                    pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
                }
            }

            writeableBitmap.WritePixels(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight), pixels, writeableBitmap.PixelWidth * writeableBitmap.Format.BitsPerPixel / 8, 0);
            ImageBrush imageBrush = new ImageBrush(writeableBitmap);
            imageBrush.Stretch = Stretch.None;
            imageBrush.TileMode = TileMode.None;
            HexagonPath.Fill = imageBrush;
        }

        /// <summary>
        /// Redraw 3 markers at bottom of main triangle
        /// </summary>
        private void Redraw_TriangleMarkers()
        {
            double xPosition = TriSide / 5d;
            TriMarkTrans3.X = TriColorMarkTrans.X;

            double tmpPosition;

            tmpPosition = (TriMarkTrans3.X - xPosition * 2);
            if (tmpPosition < -TriSide / 2d)
                tmpPosition = TriSide / 2d + (tmpPosition + TriSide / 2d);
            TriMarkTrans1.X = tmpPosition;

            tmpPosition = (TriMarkTrans3.X - xPosition);
            if (tmpPosition < -TriSide / 2d)
                tmpPosition = TriSide / 2d + (tmpPosition + TriSide / 2d);
            TriMarkTrans2.X = tmpPosition;

            tmpPosition = (TriMarkTrans3.X + xPosition);
            if (tmpPosition > TriSide / 2d)
                tmpPosition = -TriSide / 2d + (tmpPosition - TriSide / 2d);
            TriMarkTrans4.X = tmpPosition;

            tmpPosition = (TriMarkTrans3.X + xPosition * 2);
            if (tmpPosition > TriSide / 2d)
                tmpPosition = -TriSide / 2d + (tmpPosition - TriSide / 2d);
            TriMarkTrans5.X = tmpPosition;
        }

        /// <summary>
        /// Redraw inner main triangle inside hexagon
        /// </summary>
        private void Redraw_Triangle()
        {
            double angle = GetAngle(HexagonDirectorPoint, HexagonPointCenter);

            WriteableBitmap writeableBitmap = new WriteableBitmap((int)TriSide, (int)TriHeight, 96, 96, PixelFormats.Pbgra32, null);
            uint[] pixels = new uint[(writeableBitmap.PixelWidth * writeableBitmap.PixelHeight)];
            int red = 255;
            int green = 255;
            int blue = 255;
            int alpha = 255;

            Point trianglePoint1 = new Point(TriSide / 2d, 0);
            Point trianglePoint2 = new Point(0, TriHeight);
            Point trianglePoint3 = new Point(TriSide, TriHeight);

            for (int x = 0; x < writeableBitmap.PixelWidth; ++x)
            {
                for (int y = 0; y < writeableBitmap.PixelHeight; ++y)
                {
                    int i = (writeableBitmap.PixelWidth * y + x);

                    if (PointInTriangle(new Point(x, y), trianglePoint1, trianglePoint2, trianglePoint3))
                    {
                        Color rgb = HSLConverter.HslToRgb(new HSL(angle, 1, x / (double)writeableBitmap.PixelWidth));
                        red = rgb.R;
                        green = rgb.G;
                        blue = rgb.B;
                    }

                    pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
                }
            }
            writeableBitmap.WritePixels(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight), pixels, writeableBitmap.PixelWidth * writeableBitmap.Format.BitsPerPixel / 8, 0);
            TriIBrush.ImageSource = writeableBitmap;

        }

        /// <summary>
        /// Alternative fill reiangle background via GradientBrush (much faser but less accurate)
        /// </summary>
        private void Redraw_TriangleAlt()
        {
            Color color1 = HSLConverter.HslToRgb(new HSL(H, S, 0));
            Color color11 = HSLConverter.HslToRgb(new HSL(H, S, 0.05));
            Color color2 = HSLConverter.HslToRgb(new HSL(H, S, 0.5));
            Color color22 = HSLConverter.HslToRgb(new HSL(H, S, 0.95));
            Color color3 = HSLConverter.HslToRgb(new HSL(H, S, 1));

            TriBrushAlt.GradientStops.Clear();
            TriBrushAlt.GradientStops.Add(new GradientStop(color1, 0.0));
            TriBrushAlt.GradientStops.Add(new GradientStop(color11, 0.05));
            TriBrushAlt.GradientStops.Add(new GradientStop(color2, 0.5));
            TriBrushAlt.GradientStops.Add(new GradientStop(color22, 0.95));
            TriBrushAlt.GradientStops.Add(new GradientStop(color3, 1.0));
            TriBrushAlt.StartPoint = new Point(0, 0);
            TriBrushAlt.EndPoint = new Point(1, 0);

            TriPath.Fill = TriBrushAlt;
        }

        /// <summary>
        /// Redraw saturator background
        /// </summary>
        private void Redraw_Saturator()
        {
            Color rgb1 = HSLConverter.HslToRgb(new HSL(H, 0.0, 0.5));
            Color rgb2 = HSLConverter.HslToRgb(new HSL(H, 0.1, 0.5));
            Color rgb3 = HSLConverter.HslToRgb(new HSL(H, 0.5, 0.5));
            Color rgb4 = HSLConverter.HslToRgb(new HSL(H, 0.9, 0.5));
            Color rgb5 = HSLConverter.HslToRgb(new HSL(H, 1.0, 0.5));

            SaturatorBrush.GradientStops.Clear();
            SaturatorBrush.GradientStops.Add(new GradientStop(rgb1, 0.0));
            SaturatorBrush.GradientStops.Add(new GradientStop(rgb2, 0.1));
            SaturatorBrush.GradientStops.Add(new GradientStop(rgb3, 0.5));
            SaturatorBrush.GradientStops.Add(new GradientStop(rgb4, 0.9));
            SaturatorBrush.GradientStops.Add(new GradientStop(rgb5, 1.0));
            SaturatorPath.Fill = SaturatorBrush;
        }

        /// <summary>
        /// Redraw background alphator background
        /// </summary>
        private void Redraw_AlphatorBack()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap((int)(AlphaPnt2.Y - AlphaPnt1.Y), (int)(AlphaPnt2.Y - AlphaPnt1.Y), 96, 96, PixelFormats.Pbgra32, null);
            int half = (int)((writeableBitmap.Height - 1) / 2d);
            uint[] pixels = new uint[(writeableBitmap.PixelWidth * writeableBitmap.PixelHeight)];

            int red, green, blue;
            const int alpha = 255;

            for (int x = 0; x < writeableBitmap.PixelWidth; ++x)
            {
                for (int y = 0; y < writeableBitmap.PixelHeight; ++y)
                {
                    int i = (writeableBitmap.PixelWidth * y + x);

                    if (y <= half)
                    {
                        red = Colors.Gray.R;
                        green = Colors.Gray.G;
                        blue = Colors.Gray.B;
                    }
                    else
                    {
                        red = Colors.WhiteSmoke.R;
                        green = Colors.WhiteSmoke.G;
                        blue = Colors.WhiteSmoke.B;
                    }

                    pixels[i] = (uint)((alpha << 24) + (red << 16) + (green << 8) + blue);
                }
            }
            writeableBitmap.WritePixels(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight), pixels, writeableBitmap.PixelWidth * writeableBitmap.Format.BitsPerPixel / 8, 0);
            AlphaBrushBackground.ImageSource = writeableBitmap;
            AlphaBrushBackground.TileMode = TileMode.Tile;
            AlphaBrushBackground.Viewport = new Rect(0, 0, (AlphaPnt2.Y - AlphaPnt1.Y) / (AlphaPnt2.X - AlphaPnt1.X), 1);
        }

        /// <summary>
        /// Redraw alphator background
        /// </summary>
        private void Redraw_Alphator()
        {
            Color color1 = HSLConverter.HslToRgb(new HSL(H, 1, L));
            Color color2 = Color.FromArgb(0, color1.R, color1.G, color1.B);

            AlphaBrush.GradientStops.Clear();
            AlphaBrush.GradientStops.Add(new GradientStop(color2, 0));
            AlphaBrush.GradientStops.Add(new GradientStop(color1, 1));
            AlphaPath.Fill = AlphaBrush;
        }

        /// <summary>
        /// Call when hexagon director line moving (via mouse etc.) to refresh position of triangle and markers
        /// </summary>
        /// <param name="mousePoint">target point</param>
        /// <param name="sensitiveRadius">ignorable radius near centre to avoid epileptic moves</param>
        private void Move_HexDirector(Point mousePoint, double sensitiveRadius)
        {
            if (!PointInCircle(mousePoint, sensitiveRadius))
            {
                HexaginDirectorLine.Points[0] = new Point(mousePoint.X + (mousePoint.X - HexagonPointCenter.X) * 3,
                                                          mousePoint.Y + (mousePoint.Y - HexagonPointCenter.Y) * 3);
                HexagonDirectorPoint = HexaginDirectorLine.Points[0];

                Redraw_TriangleAlt();
                Redraw_Saturator();
                Redraw_Alphator();

                TriCanvasTrans.Angle = GetAngle(HexagonDirectorPoint, HexagonPointCenter) * -1 + 90;
                TriCanvasTrans.CenterX = TriCanvas.Width / 2d;
                TriCanvasTrans.CenterY = TriCanvas.Height / 2d;

                _h = GetAngle(HexaginDirectorLine.Points[0], HexagonPointCenter);
            }
        }

        /// <summary>
        /// Set angle on hexagon for hexagon director line
        /// </summary>
        /// <param name="degreeHue">Angle in grad</param>
        /// <param name="origin">Custom origin point</param>
        /// <returns>Target triangle point to set hex director line</returns>
        private void Move_HexDirector(double degreeHue, Point origin)
        {
            if (degreeHue < 0)
                degreeHue = -degreeHue;

            if (degreeHue > 360)
                degreeHue = degreeHue % 360;

            if (Math.Abs(degreeHue) < 0.00001 || Math.Abs(degreeHue - 360) < 0.00001) // (degreeHue == 0 || degreeHue == 360)
            {
                Move_HexDirector(new Point(origin.X + origin.X, origin.Y), HexagonInnerRadius);
                return;
            }
            if (Math.Abs(degreeHue - 90) < 0.00001) // (degreeHue == 90)
            {
                Move_HexDirector(new Point(origin.X, origin.Y - origin.Y), HexagonInnerRadius);
                return;
            }
            if (Math.Abs(degreeHue - 180) < 0.00001) // (degreeHue == 180)
            {
                Move_HexDirector(new Point(origin.X - origin.X, origin.Y), HexagonInnerRadius);
                return;
            }
            if (Math.Abs(degreeHue - 270) < 0.00001) // (degreeHue == 270)
            {
                Move_HexDirector(new Point(origin.X, origin.Y + origin.Y), HexagonInnerRadius);
                return;
            }

            if (Math.Abs(degreeHue - 45) < 0.00001) // (degreeHue == 45)
            {
                Move_HexDirector(new Point(origin.X + origin.X, origin.Y - origin.Y), HexagonInnerRadius);
                return;
            }
            if (Math.Abs(degreeHue - 135) < 0.00001) // (degreeHue == 135)
            {
                Move_HexDirector(new Point(origin.X - origin.X, origin.Y - origin.Y), HexagonInnerRadius);
                return;
            }
            if (Math.Abs(degreeHue - 225) < 0.00001) // (degreeHue == 225)
            {
                Move_HexDirector(new Point(origin.X - origin.X, origin.Y + origin.Y), HexagonInnerRadius);
                return;
            }
            if (Math.Abs(degreeHue - 315) < 0.00001) // (degreeHue == 315)
            {
                Move_HexDirector(new Point(origin.X + origin.X, origin.Y + origin.Y), HexagonInnerRadius);
                return;
            }

            double sideB = origin.X;
            double radiance = degreeHue * (Math.PI / 180d);
            double sideA = sideB * Math.Tan(radiance);

            if (degreeHue > 0 && degreeHue < 45)
            {
                Move_HexDirector(new Point(origin.X + sideB, origin.Y - sideA), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 45 && degreeHue < 90)
            {
                degreeHue = 90 - degreeHue;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X + sideA, origin.Y - sideB), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 90 && degreeHue < 135)
            {
                degreeHue = degreeHue - 90;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X - sideA, origin.Y - sideB), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 135 && degreeHue < 180)
            {
                degreeHue = 180 - degreeHue;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X - sideB, origin.Y - sideA), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 180 && degreeHue < 225)
            {
                degreeHue = degreeHue - 180;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X - sideB, origin.Y + sideA), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 225 && degreeHue < 270)
            {
                degreeHue = 270 - degreeHue;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X - sideA, origin.Y + sideB), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 270 && degreeHue < 315)
            {
                degreeHue = degreeHue - 270;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X + sideA, origin.Y + sideB), HexagonInnerRadius);
                return;
            }
            if (degreeHue > 315 && degreeHue < 360)
            {
                degreeHue = 360 - degreeHue;
                radiance = degreeHue * (Math.PI / 180d);
                sideA = sideB * Math.Tan(radiance);
                Move_HexDirector(new Point(origin.X + sideB, origin.Y + sideA), HexagonInnerRadius);
                return;
            }

            throw new Exception("Impossiburu angle! Angle must be [0..360]");
        }

        /// <summary>
        /// Lightness (triangle) mouse move
        /// </summary>
        /// <param name="mousePoint">Point of interest</param>
        private void Move_TriColorMark(Point mousePoint)
        {
            if (PointInTriangle(mousePoint, TriPnt1, TriPnt2, TriPnt3)) // mouse inside
            {
                TriColorMarkTrans.X = mousePoint.X - TriCanvas.Width / 2d;
                TriColorMarkTrans.Y = mousePoint.Y - TriCanvas.Height / 2d;
            }
            else if (mousePoint.Y > TriPnt2.Y) // mouse bottom
            {
                if (mousePoint.X < TriPnt2.X)
                    TriColorMarkTrans.X = TriPnt2.X - TriCanvas.Width / 2d;
                else if (mousePoint.X > TriPnt3.X)
                    TriColorMarkTrans.X = TriPnt3.X - TriCanvas.Width / 2d;
                else
                    TriColorMarkTrans.X = mousePoint.X - TriCanvas.Width / 2d;

                TriColorMarkTrans.Y = TriPnt2.Y - TriCanvas.Height / 2d;
            }
            else if (mousePoint.X > TriPnt1.X) // mouse righter
            {
                if (mousePoint.Y < TriPnt1.Y)
                {
                    TriColorMarkTrans.X = TriPnt1.X - TriCanvas.Width / 2d;
                    TriColorMarkTrans.Y = TriPnt1.Y - TriCanvas.Height / 2d;
                }
                else
                {
                    double tmpside = (mousePoint.Y - TriPnt1.Y) / (Math.Sqrt(3d) / 2d);
                    TriColorMarkTrans.X = (TriPnt1.X + (tmpside / 2d)) - TriCanvas.Width / 2d;
                    TriColorMarkTrans.Y = mousePoint.Y - TriCanvas.Height / 2d;
                }
            }
            else if (mousePoint.X < TriPnt1.X) // mouse lefter
            {
                if (mousePoint.Y < TriPnt1.Y)
                {
                    TriColorMarkTrans.X = TriPnt1.X - TriCanvas.Width / 2d;
                    TriColorMarkTrans.Y = TriPnt1.Y - TriCanvas.Height / 2d;
                }
                else
                {
                    double tmpside = (mousePoint.Y - TriPnt1.Y) / (Math.Sqrt(3d) / 2d);
                    TriColorMarkTrans.X = (TriPnt1.X - (tmpside / 2d)) - TriCanvas.Width / 2d;
                    TriColorMarkTrans.Y = mousePoint.Y - TriCanvas.Height / 2d;
                }
            }

            double point = TriColorMarkTrans.X + TriSide / 2d;
            _l = (point / (TriSide / 100d)) / 100d;
        }

        /// <summary>
        /// Lightness (triangle) editor set by value
        /// </summary>
        /// <param name="value">Value of lightness [0..1]</param>
        private void Move_TriColorMark(double value)
        {
            double point = (TriSide * value) - TriSide / 2d;
            double tmp = value > 0.5 ? 1 - value : value;
            TriColorMarkTrans.X = point;
            TriColorMarkTrans.Y = (TriCanvas.Height - TriHeight) - TriHeight * tmp;
        }

        /// <summary>
        /// Satiration editor mouse move
        /// </summary>
        /// <param name="mousePoint">Point of interest</param>
        private void Move_SatDirector(Point mousePoint)
        {
            // do -1 pixel to avoid WPF antialiasing mistakes
            double targetX = mousePoint.X;
            if (targetX < SaturatorPnt1.X)
                targetX = SaturatorPnt1.X;
            if (targetX > SaturatorPnt2.X - 1)
                targetX = SaturatorPnt2.X - 1;
            SaturatorDirectorTranslation.X = targetX - SaturatorPnt1.X;

            _s = (targetX - SaturatorPnt1.X) / (SaturatorPnt2.X - 1 - SaturatorPnt1.X);
            Redraw_TriangleAlt();
        }

        /// <summary>
        /// Satiration editor set by value
        /// </summary>
        /// <param name="value">Value of saturation [0..1]</param>
        private void Move_SatDirector(double value)
        {
            double targetX = SaturatorPnt1.X + ((SaturatorPnt2.X - SaturatorPnt1.X) * value);

            // do -1 pixel to avoid WPF antialiasing mistakes
            if (targetX < SaturatorPnt1.X)
                targetX = SaturatorPnt1.X;
            if (targetX > SaturatorPnt2.X - 1)
                targetX = SaturatorPnt2.X - 1;
            SaturatorDirectorTranslation.X = targetX - SaturatorPnt1.X;

            Redraw_TriangleAlt();
        }

        /// <summary>
        /// Alpha editor mouse move
        /// </summary>
        /// <param name="mousePoint">Point of interest</param>
        private void Move_AlphDirector(Point mousePoint)
        {
            // do -1 pixel to avoid WPF antialiasing mistakes
            double targetX = mousePoint.X;
            if (targetX < AlphaPnt1.X)
                targetX = AlphaPnt1.X;
            if (targetX > AlphaPnt2.X - 1)
                targetX = AlphaPnt2.X - 1;
            AlphaDirectorTranslation.X = targetX - AlphaPnt1.X;

            if (mousePoint.X - AlphaPnt1.X > 0)
                _a = (byte)(255d / ((AlphaPnt2.X - AlphaPnt1.X - 1) / (targetX - AlphaPnt1.X)));
            else
                _a = 0;
        }

        /// <summary>
        /// Alpha editor set by value
        /// </summary>
        /// <param name="value">Alpha value [0..255]</param>
        private void Move_AlphDirector(byte value)
        {
            double targetX = AlphaPnt1.X + ((AlphaPnt2.X - AlphaPnt1.X) * (value / 255d));

            // do -1 pixel to avoid WPF antialiasing mistakes
            if (targetX < AlphaPnt1.X)
                targetX = AlphaPnt1.X;
            if (targetX > AlphaPnt2.X - 1)
                targetX = AlphaPnt2.X - 1;
            AlphaDirectorTranslation.X = targetX - AlphaPnt1.X;
        }

        #region Local helpers

        /// <summary>
        /// Get an angle of the point for hexagon color wheel in range 0-1
        /// </summary>
        /// <param name="sourcePoint">Point of interest</param>
        /// <param name="origin">Origin point</param>
        /// <returns></returns>
        private static double GetAngle(Point sourcePoint, Point origin)
        {
            double x = (sourcePoint.X - origin.X);
            double y = -(sourcePoint.Y - origin.Y);
            double angle = Math.Atan2(y, x) * (180 / Math.PI);

            if (angle >= 360)
                angle = 360 - angle;

            if (angle < 0)
                angle = 360 + angle;

            //angle /= 360;
            return angle;
        }

        /// <summary>
        /// Target point is inside a triangle?
        /// </summary>
        /// <param name="sourcePoint">Point of interest</param>
        /// <param name="p1">Triangle point 1</param>
        /// <param name="p2">Triangle point 2</param>
        /// <param name="p3">Triangle point 3</param>
        /// <returns></returns>
        private static bool PointInTriangle(Point sourcePoint, Point p1, Point p2, Point p3)
        {
            double t1 = ((p1.X - sourcePoint.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p1.Y - sourcePoint.Y));
            double t2 = ((p2.X - sourcePoint.X) * (p3.Y - p2.Y) - (p3.X - p2.X) * (p2.Y - sourcePoint.Y));
            double t3 = ((p3.X - sourcePoint.X) * (p1.Y - p3.Y) - (p1.X - p3.X) * (p3.Y - sourcePoint.Y));
            return ((t1 >= 0 && t2 >= 0 && t3 >= 0) || (t1 <= 0 && t2 <= 0 && t3 <= 0));
        }

        /// <summary>
        /// Target point is inside a circle?
        /// </summary>
        /// <param name="srcP">Point of interest</param>
        /// <param name="radius">Radius of circle</param>
        /// <returns></returns>
        private bool PointInCircle(Point srcP, double radius)
        {
            return (srcP.X - HexagonPointCenter.X) * (srcP.X - HexagonPointCenter.X) + (srcP.Y - HexagonPointCenter.Y) * (srcP.Y - HexagonPointCenter.Y) <= radius * radius;
        }

        /// <summary>
        /// Target point is inside a hexagon?
        /// </summary>
        /// <param name="sourcePoint">Point of interest</param>
        /// <returns></returns>
        private bool PointInHexagon(Point sourcePoint)
        {
            // extend hexagon by 1 pixel to avoid antialiasing problems
            Point hexPnt1 = new Point(HexagonPoint1.X + 1, HexagonPoint1.Y);
            Point hexPnt2 = new Point(HexagonPoint2.X + 1, HexagonPoint2.Y - 1);
            Point hexPnt3 = new Point(HexagonPoint3.X - 1, HexagonPoint3.Y - 1);
            Point hexPnt4 = new Point(HexagonPoint4.X - 1, HexagonPoint4.Y);
            Point hexPnt5 = new Point(HexagonPoint5.X - 1, HexagonPoint5.Y + 1);
            Point hexPnt6 = new Point(HexagonPoint6.X + 1, HexagonPoint6.Y + 1);

            if (sourcePoint.Y < hexPnt2.Y || sourcePoint.Y > hexPnt5.Y)
                return false;

            if (sourcePoint.X >= hexPnt3.X && sourcePoint.X <= hexPnt2.X && sourcePoint.Y >= hexPnt3.Y && sourcePoint.Y < hexPnt5.Y)
                return true;

            if (sourcePoint.Y <= HexagonPointCenter.Y)
            {
                if (sourcePoint.X <= hexPnt3.X)
                {
                    if (PointInTriangle(sourcePoint, HexagonPointCenter, hexPnt3, hexPnt4)) return true;

                }
                else if (sourcePoint.X >= hexPnt2.X)
                {
                    if (PointInTriangle(sourcePoint, HexagonPointCenter, hexPnt1, hexPnt2)) return true;
                }
            }
            else
            {
                if (sourcePoint.X <= hexPnt5.X)
                {
                    if (PointInTriangle(sourcePoint, HexagonPointCenter, hexPnt4, hexPnt5)) return true;
                }
                else if (sourcePoint.X >= hexPnt6.X)
                {
                    if (PointInTriangle(sourcePoint, HexagonPointCenter, hexPnt6, hexPnt1)) return true;
                }
            }
            return false;
        }

        #endregion

    }
}
