using XFeatures.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace XFeatures.MiddleClickScroll
{
    internal sealed class MiddleClickScroll : MouseProcessorBase
    {
        private const double minMove = 10.0;
        private const double minTime = 25.0;
        private const double moveDivisor = 200.0;
        private IWpfTextView _view;
        private Point? _location;
        private Cursor _oldCursor;
        private DispatcherTimer _moveTimer;
        private DateTime _lastMoveTime;
        private IAdornmentLayer _layer;
        private Image _zeroPointImage;
        private bool _dismissOnMouseUp;

        private MiddleClickScroll(IWpfTextView view, MiddleClickScrollFactory factory)
        {
            this._view = view;
            this._layer = view.GetAdornmentLayer("MiddleClickScrollLayer");
            this._view.Closed += new EventHandler(this.OnClosed);
            this._view.VisualElement.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnIsVisibleChanged);
        }

        public static IMouseProcessor Create(IWpfTextView view, MiddleClickScrollFactory factory)
        {
            return (IMouseProcessor)view.Properties.GetOrCreateSingletonProperty<MiddleClickScroll>((Func<MiddleClickScroll>)(() => new MiddleClickScroll(view, factory)));
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this._view.VisualElement.IsVisible)
                return;
            this.StopScrolling();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            this.StopScrolling();
            this._view.VisualElement.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(this.OnIsVisibleChanged);
            this._view.Closed -= new EventHandler(this.OnClosed);
        }

        public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.PreprocessMouseDown(e);
        }

        public override void PreprocessMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.PreprocessMouseDown(e);
        }

        public override void PreprocessMouseDown(MouseButtonEventArgs e)
        {
            if (SettingsProvider.IsMiddleClickScrollEnabled() == false)
                return;
            if (this._location.HasValue)
            {
                this.StopScrolling();
                e.Handled = true;
            }
            else
            {
                if (e.ChangedButton != MouseButton.Middle || this._view.IsClosed || (!this._view.VisualElement.IsVisible || !this._view.VisualElement.CaptureMouse()))
                    return;
                this._oldCursor = this._view.VisualElement.Cursor;
                this._view.VisualElement.Cursor = Cursors.ScrollAll;
                Point position = e.GetPosition((IInputElement)this._view.VisualElement);
                this._location = new Point?(this._view.VisualElement.PointToScreen(position));
                if (this._zeroPointImage == null)
                {
                    BitmapSource bitmapSourceFromHicon = Imaging.CreateBitmapSourceFromHIcon(User32.LoadImage(IntPtr.Zero, new IntPtr(32654), 2U, 0, 0, 40960U), Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapSourceFromHicon.Freeze();
                    this._zeroPointImage = new Image();
                    this._zeroPointImage.Source = (ImageSource)bitmapSourceFromHicon;
                    this._zeroPointImage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    this._zeroPointImage.Opacity = 0.5;
                }
                Canvas.SetLeft((UIElement)this._zeroPointImage, this._view.ViewportLeft + position.X - this._zeroPointImage.DesiredSize.Width * 0.5);
                Canvas.SetTop((UIElement)this._zeroPointImage, this._view.ViewportTop + position.Y - this._zeroPointImage.DesiredSize.Height * 0.5);
                this._layer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, new SnapshotSpan?(), (object)null, (UIElement)this._zeroPointImage, (AdornmentRemovedCallback)null);
                this._lastMoveTime = DateTime.Now;
                this._moveTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 25), DispatcherPriority.Normal, new EventHandler(this.OnTimerElapsed), this._view.VisualElement.Dispatcher);
                this._dismissOnMouseUp = false;
                e.Handled = true;
            }
        }

        public override void PreprocessMouseUp(MouseButtonEventArgs e)
        {
            if (!this._dismissOnMouseUp || e.ChangedButton != MouseButton.Middle)
                return;
            this.StopScrolling();
            e.Handled = true;
        }

        private void StopScrolling()
        {
            if (!this._location.HasValue)
                return;
            this._location = new Point?();
            this._view.VisualElement.Cursor = this._oldCursor;
            this._oldCursor = (Cursor)null;
            this._view.VisualElement.ReleaseMouseCapture();
            this._moveTimer.Stop();
            this._moveTimer.Tick -= new EventHandler(this.OnTimerElapsed);
            this._moveTimer = (DispatcherTimer)null;
            this._layer.RemoveAllAdornments();
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            if (!this._view.IsClosed && this._view.VisualElement.IsVisible && this._location.HasValue)
            {
                DateTime now = DateTime.Now;
                Vector vector = this._view.VisualElement.PointToScreen(Mouse.GetPosition((IInputElement)this._view.VisualElement)) - this._location.Value;
                double val1 = Math.Abs(vector.X);
                double val2 = Math.Abs(vector.Y);
                double num1 = Math.Max(val1, val2);
                if (num1 > 10.0)
                {
                    this._dismissOnMouseUp = true;
                    double totalMilliseconds = (now - this._lastMoveTime).TotalMilliseconds;
                    double num2 = (num1 - 10.0) * totalMilliseconds / 200.0;
                    if (val1 > val2)
                    {
                        if (vector.X > 0.0)
                        {
                            IWpfTextView wpfTextView = this._view;
                            double num3 = wpfTextView.ViewportLeft + num2;
                            wpfTextView.ViewportLeft = num3;
                            this._view.VisualElement.Cursor = Cursors.ScrollE;
                        }
                        else
                        {
                            IWpfTextView wpfTextView = this._view;
                            double num3 = wpfTextView.ViewportLeft - num2;
                            wpfTextView.ViewportLeft = num3;
                            this._view.VisualElement.Cursor = Cursors.ScrollW;
                        }
                    }
                    else
                    {
                        ITextViewLine textViewLine = (ITextViewLine)this._view.TextViewLines[0];
                        double num3 = textViewLine.Top - this._view.ViewportTop;
                        double verticalDistance;
                        if (vector.Y > 0.0)
                        {
                            verticalDistance = num3 - num2;
                            this._view.VisualElement.Cursor = Cursors.ScrollS;
                        }
                        else
                        {
                            verticalDistance = num3 + num2;
                            this._view.VisualElement.Cursor = Cursors.ScrollN;
                        }
                        this._view.DisplayTextLineContainingBufferPosition(textViewLine.Start, verticalDistance, ViewRelativePosition.Top);
                    }
                }
                else
                    this._view.VisualElement.Cursor = Cursors.ScrollAll;
                this._lastMoveTime = now;
            }
            else
                this.StopScrolling();
        }
    }
}
