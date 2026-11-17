using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;

using Config = SessionControlMgr.SettingsData;

namespace SessionControlMgr
{
    public class WindowSaveStateBehavior : Behavior<Window>
    {
        public Config Config
        {
            get 
            { 
                return (Config)GetValue(ConfigProperty); 
            }
            set 
            { 
                SetValue(ConfigProperty, value); 
            }
        }

        public static readonly DependencyProperty ConfigProperty =
            DependencyProperty.Register("Config", typeof(Config), typeof(WindowSaveStateBehavior), new PropertyMetadata(Config_Changed));

        private static void Config_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as WindowSaveStateBehavior;
            if (e.NewValue != null) b?.LoadSettings();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SizeChanged += Window_SizeChanged;
            AssociatedObject.StateChanged += Window_StateChanged;
            AssociatedObject.LocationChanged += Window_LocationChanged;
            LoadSettings();
        }

        bool _initialized = false;

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            SaveSettings();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveSettings();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (Config == null) return;

            AssociatedObject.Width = Config.WindowWidth;
            AssociatedObject.Height = Config.WindowHeight;
            AssociatedObject.Top = Config.WindowTop;
            AssociatedObject.Left = Config.WindowLeft;
            AssociatedObject.WindowState = Config.WindowState;

            _initialized = true;
        }

        private void SaveSettings()
        {
            if (Config == null || !_initialized) return;

            Config.WindowState = AssociatedObject.WindowState;
            if (AssociatedObject.WindowState == WindowState.Normal)
            {
                Config.WindowTop = (int)AssociatedObject.Top;
                Config.WindowLeft = (int)AssociatedObject.Left;

                Config.WindowWidth = (int)AssociatedObject.Width;
                Config.WindowHeight = (int)AssociatedObject.Height;
            }
        }
    }
}
