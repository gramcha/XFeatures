using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using XFeatures.Settings;
namespace XFeatures.HideMainMenu
{
    class HideMMenu
    {
        private FrameworkElement _menuContainer;
        private bool _isMenuVisible;
        Window mainWindow;
        bool mousemove=false;
        private bool IsMenuVisible
        {
            get
            {
                return this._isMenuVisible;
            }
            set
            {
                if (this._isMenuVisible != value)
                {
                    this._isMenuVisible = value;
                    if (this._menuContainer != null)
                    {
                        if (this._isMenuVisible)
                        {
                            this._menuContainer.ClearValue(FrameworkElement.HeightProperty);
                            //this._menuContainer.Height = 10.0;
                            //MessageBox.Show("after clear " + this._menuContainer.Height.ToString());
                            return;
                        }
                        if (SettingsProvider.IsHideMenuEnabled())
                            this._menuContainer.Height = 0.0;
                    }
                }
            }
        }
        private FrameworkElement MenuContainer
        {
            get
            {
                return this._menuContainer;
            }
            set
            {
                if (this._menuContainer != null)
                {
                    this._menuContainer.IsKeyboardFocusWithinChanged -= new DependencyPropertyChangedEventHandler(this.OnMenuContainerFocusChanged);
                    this._menuContainer.MouseEnter -= new MouseEventHandler(this.OnMouseEnter);
                    this._menuContainer.MouseLeave -= new MouseEventHandler(this.OnMouseLeave);
                }
                this._menuContainer = value;
                if (this._menuContainer != null)
                {
                    if (this._isMenuVisible)
                    {
                        this._menuContainer.ClearValue(FrameworkElement.HeightProperty);
                    }
                    else
                    {
                        if(SettingsProvider.IsHideMenuEnabled())
                            this._menuContainer.Height = 0.0;
                    }
                    this._menuContainer.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(this.OnMenuContainerFocusChanged);
                    this._menuContainer.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
                    this._menuContainer.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
                    //this._menuContainer.MouseMove += new MouseEventHandler(this.OnMouseMove);                    
                }
            }
        }
        public HideMMenu()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", new object[]
            {
                this.ToString()
            }));
        }
        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            mousemove = true;
            this.IsMenuVisible = this.IsAggregateFocusInMenuContainer(this.MenuContainer);
            mousemove = false;
        }
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            mousemove = false;
            this.IsMenuVisible = this.IsAggregateFocusInMenuContainer(this.MenuContainer);            
        }
        private void OnMenuContainerFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.IsMenuVisible = this.IsAggregateFocusInMenuContainer(this.MenuContainer);
        }
        private void LayoutUpdated(object sender, EventArgs e)
        {
            bool flag = false;
            foreach (Menu current in mainWindow.FindDescendants<Menu>())
            {
                if (AutomationProperties.GetAutomationId(current) == "MenuBar")
                {
                    FrameworkElement frameworkElement = current;
                    DependencyObject visualOrLogicalParent = current.GetVisualOrLogicalParent();
                    if (visualOrLogicalParent != null)
                    {
                        frameworkElement = ((visualOrLogicalParent.GetVisualOrLogicalParent() as DockPanel) ?? frameworkElement);
                    }
                    flag = true;
                    this.MenuContainer = frameworkElement;
                }
            }            
            if (flag)
            {
                mainWindow.LayoutUpdated -= this.LayoutUpdated;
            }
        }
        public void Initialize()
        {			
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this.PopupLostKeyboardFocus));
            mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                //EventHandler layoutUpdated = null;
                //layoutUpdated = delegate(object sender, EventArgs e)
                //{
                    
                //};
                mainWindow.LayoutUpdated += this.LayoutUpdated;
            }
        }
        private void PopupLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.IsMenuVisible && this.MenuContainer != null && !this.IsAggregateFocusInMenuContainer(this.MenuContainer))
            {
                this.IsMenuVisible = false;
            }
        }
        private bool IsAggregateFocusInMenuContainer(FrameworkElement menuContainer)
        {
            if (menuContainer.IsKeyboardFocusWithin || mousemove)
            {
                return true;
            }
            for (DependencyObject dependencyObject = (DependencyObject)Keyboard.FocusedElement; dependencyObject != null; dependencyObject = dependencyObject.GetVisualOrLogicalParent())
            {
                if (dependencyObject == menuContainer)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
