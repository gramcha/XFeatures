using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.Utilities;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;
using XFeatures.Settings;

namespace XFeatures.SyncMouseWheelZoom
{
    [TextViewRole("DOCUMENT")]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    internal sealed class SyncMouseWheelZoomListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            SyncMouseWheelZoom syncMouseWheel = new SyncMouseWheelZoom(textView);
        }
    }
    
    public class SyncMouseWheelZoom
    {
        private IWpfTextView _view;
        private bool _bFirst;
        private const double MinZoomLevel = 40, MaxZoomLevel = 400, DefaultZoomLevel = 100;
        private static double LastZoomLevel;


        public SyncMouseWheelZoom(IWpfTextView view)
        {
            this._view = view;            
            this._bFirst = true;            
            this._view.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(this.LayoutChanged);
            this._view.ZoomLevelChanged += new EventHandler<ZoomLevelChangedEventArgs>(this.ZoomLevelChanged);
            this._view.Closed += new EventHandler(this.ViewClosed);
            this._view.GotAggregateFocus += FirstGotFocus;
            view.VisualElement.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs args)
            { this.SetZoomLevel(_view); };
        }
        void FirstGotFocus(object sender, EventArgs e)
        {
            ((ITextView)sender).GotAggregateFocus -= FirstGotFocus;
            
            this.SetZoomLevel(_view); 
        }
        private void ViewClosed(object sender, EventArgs e)
        {
            this._view.LayoutChanged -= new EventHandler<TextViewLayoutChangedEventArgs>(this.LayoutChanged);
            this._view.ZoomLevelChanged -= new EventHandler<ZoomLevelChangedEventArgs>(this.ZoomLevelChanged);
            this._view.Closed -= new EventHandler(this.ViewClosed);
            //Settings.Default.PropertyChanged -= new PropertyChangedEventHandler(this.Default_PropertyChanged);
            this._view = (IWpfTextView)null;
        }

        private void LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (!this._bFirst)
                return;
            this._bFirst = false;
            this.SetZoomLevel(this._view);
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.SetZoomLevel(this._view);
        }

        private bool SetZoomLevel(IWpfTextView _view)
        {
            try
            {
                if (!SettingsProvider.IsSyncMouseWheelZoomEnabled())
                    return false;
                if (!_view.IsClosed)
                {
                    if (!this._bFirst)
                    {
                        if (_view.ZoomLevel != LastZoomLevel)
                        {
                            if (LastZoomLevel >= MinZoomLevel)
                            {
                                if (LastZoomLevel <= MaxZoomLevel)
                                {
                                    _view.ZoomLevel = LastZoomLevel;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException ex)
            {
            }
            return false;
        }

        private void ZoomLevelChanged(object sender, ZoomLevelChangedEventArgs e)
        {
            if (sender == this || LastZoomLevel == e.NewZoomLevel)
                return;
            LastZoomLevel = e.NewZoomLevel;
            this.SetZoomLevel(this._view);
            
        }

        private static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }
    }
}
