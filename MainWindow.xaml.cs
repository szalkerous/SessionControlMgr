using RMAwareWinFormApplication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
//using static SessionControlMgr.Session;

namespace SessionControlMgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Session Session { get; set; }
        private bool _loading = false;
        private Timer? timer_autostart;
        private Timer? timer_startminimized;

        public MainWindow()
        {
            if (AnotherInstanceExists())
            {
                App.Current.Shutdown();
            }

            Session = new SessionControlMgr.Session();
            _loading = true;
            InitializeComponent();
            Session.SetPokeCallback(this.UpdateStatus);
            Session.SetUpdateUICallback(this.UpdateStatus);
            SizeChanged += Window_SizeChanged;
            StateChanged += Window_StateChanged;
            LocationChanged += Window_LocationChanged;

            this.textStatus.Text = "Stopped";
            this.Title = "Session Lock Monitor - Stopped";
            LoadSettings();
            _loading = false;

            // Register application for restart
            if (RestartManagerWrapper.IsRestartManagerSupported)
            {
                string commandLine = String.Empty;
                uint result = RestartManagerWrapper.RegisterApplicationRestart(commandLine, ApplicationRestartFlags.RESTART_NO_HANG);

                if (result == HResult.E_FAIL || result == HResult.E_INVALIDARG)
                {
                    Debug.WriteLine("Failed to register application restart.");
                }
            }  

            if (Session.Config.AutoStart)
               timer_autostart = new Timer(autostart_callback, null, 3000, Timeout.Infinite);

            if (Session.Config.StartMinimized)
                timer_startminimized = new Timer(startminimized_callback, null, 1000, Timeout.Infinite);
        }

        ~MainWindow()
        {
            UpdateEnabled(false);
            Session.EnableDeviceSleep();
            Session.ClearCallbacks();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;

            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...

            // lParam: If this parameter is 0, the system is shutting down or restarting (it is not possible to determine which event is occurring)
            //         ENDSESSION_CLOSEAPP (0x00000001): The application is using a file that must be replaced, the system is being serviced, or system resources are exhausted.
            //         ENDSESSION_CRITICAL (0x40000000): The application is forced to shut down.
            //         ENDSESSION_LOGOFF (0x80000000): The user is logging off. 

            // wParam: (WM_ENDSESSION only) If the session is being ended, this parameter is TRUE; the session can end any time after all applications have returned
            //         from processing this message. Otherwise, it is FALSE.

            if (msg == WindowsMessages.WM_QUERYENDSESSION)
            {
                // When an application returns TRUE for this message, it receives the WM_ENDSESSION message, regardless of how the other
                // applications respond to the WM_QUERYENDSESSION message. Each application should return TRUE or FALSE immediately upon
                // receiving this message, and defer any cleanup operations until it receives the WM_ENDSESSION message.

                // If your application must block or postpone system shutdown, use the ShutdownBlockReasonCreate function.

                return (IntPtr)1;
            }
            else if (msg == WindowsMessages.WM_ENDSESSION)
            {
                int lpvalue = lParam.ToInt32();

                if (lpvalue == LParameter.ENDSESSION_CLOSEAPP || lpvalue == 0)
                {
                    //Save application state so it can be restored when its restarted.
                    SaveSettings();
                    UpdateEnabled(false);
                }

                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveSettings();
            UpdateEnabled(false);
            base.OnClosing(e);
        }

        private void SaveSettings()
        {
            Debug.WriteLine("SaveSettings Start");

            Session.Config.UseSTES = this.checkboxSTES.IsChecked is not null && (bool)(this.checkboxSTES.IsChecked);
            Session.Config.UseKeyCode = this.checkboxKeycode.IsChecked is not null && (bool)(this.checkboxKeycode.IsChecked);

            if (this.comboKeyCode.SelectedItem is not null)
            {
                EnumerationExtension.EnumerationMember item = (EnumerationExtension.EnumerationMember)this.comboKeyCode.SelectedItem;
                if (item.Value is not null)
                    Session.Config.KeyCode = (uint)item.Value;
                else
                    Session.Config.KeyCode = 0;
            }
            else
                Session.Config.KeyCode = 0;

            Session.Config.Hours = Int32.Parse(this.textboxHours.Text);
            Session.Config.Minutes = Int32.Parse(this.textboxMinutes.Text);
            Session.Config.TimeFrom = (this.pickerFrom.SelectedTime is not null) ? this.pickerFrom.SelectedTime.Value : DateTime.Parse("10:00:00");
            Session.Config.TimeTo = (this.pickerTo.SelectedTime is not null) ? this.pickerTo.SelectedTime.Value : DateTime.Parse("18:30:00");

            if (this.optionDuration.IsChecked is not null && (bool)(this.optionDuration.IsChecked))
                Session.Config.Action = 1;
            else if (this.optionRange.IsChecked is not null && (bool)(this.optionRange.IsChecked))
                Session.Config.Action = 2;
            else
                Session.Config.Action = 0;

            Session.Config.AutoStart = this.toggleAutoStart.IsChecked ?? false;
            Session.Config.StartMinimized = this.toggleStartMinimized.IsChecked ?? false;

            Debug.WriteLine($"SAVE: Session.Config.StartMinimized=({Session.Config.StartMinimized.GetType()}){Session.Config.StartMinimized}");

            Session.Config.Save();

            Debug.WriteLine("SaveSettings End");

        }

        private void LoadSettings()
        {
            Debug.WriteLine("LoadSettings Start");
            _loading = true;

            this.checkboxSTES.IsChecked = Session.Config.UseSTES;
            this.checkboxKeycode.IsChecked = Session.Config.UseKeyCode;
            if (Session.Config.KeyCode > 0)
            {
                foreach (EnumerationExtension.EnumerationMember curitem in comboKeyCode.Items.SourceCollection)
                {
                    if (curitem.ValueType == typeof(VK))
                    {
                        if (curitem.Value is not null)
                        {
                            if ((VK)curitem.Value == (VK)Session.Config.KeyCode)
                            {
                                this.comboKeyCode.SelectedItem = curitem;
                                break;
                            }
                        }
                    }
                }
            }

            this.textboxHours.Text = Session.Config.Hours.ToString();
            this.textboxMinutes.Text = Session.Config.Minutes.ToString();

            this.pickerFrom.Text = Session.Config.TimeFrom.ToShortTimeString();
            this.pickerTo.Text = Session.Config.TimeTo.ToShortTimeString();

            if (Session.Config.Action == 1)
                this.optionDuration.IsChecked = true;
            else if (Session.Config.Action == 2)
                this.optionRange.IsChecked = true;
            else
                this.optionAlways.IsChecked = true;

            this.Top = Session.Config.WindowTop;
            this.Left = Session.Config.WindowLeft;

            Thread.Yield();

            this.WindowState = Session.Config.WindowState;

            this.toggleAutoStart.IsChecked = Session.Config.AutoStart;

            Debug.WriteLine($"LOAD: Session.Config.StartMinimized=({Session.Config.StartMinimized.GetType()}){Session.Config.StartMinimized}");

            this.toggleStartMinimized.IsChecked = Session.Config.StartMinimized;

            Debug.WriteLine("LoadSettings End");
            _loading = false;
        }

        private void ToggleUIElements(bool state)
        {
            this.optionAlways.IsEnabled = state;
            this.optionDuration.IsEnabled = state;
            this.optionRange.IsEnabled = state;
            this.textboxHours.IsEnabled = state;
            this.textboxMinutes.IsEnabled = state;
            this.pickerFrom.IsEnabled = state;
            this.pickerTo.IsEnabled = state;
            this.checkboxSTES.IsChecked = state;
            this.checkboxKeycode.IsEnabled = state;
            this.comboKeyCode.IsEnabled = state;

            double animation_duration = 0.15;

            var settings_hide = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(animation_duration),
            };

            var settings_show = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(animation_duration),
            };

            var status_hide = new DoubleAnimation()
            {
                From = 200,
                To = 24,
                Duration = TimeSpan.FromSeconds(animation_duration),
            };

            var status_show = new DoubleAnimation()
            {
                From = 24,
                To = 200,
                Duration = TimeSpan.FromSeconds(animation_duration),

            };

            if (state)
            {
                cardSettings_Scale.BeginAnimation(ScaleTransform.ScaleYProperty, settings_show);
                cardStatus.BeginAnimation(HeightProperty, status_hide, HandoffBehavior.SnapshotAndReplace);
                textStatusBanner.Visibility = Visibility.Collapsed;
            }
            else
            {
                cardSettings_Scale.BeginAnimation(ScaleTransform.ScaleYProperty, settings_hide);
                cardStatus.BeginAnimation(HeightProperty, status_show);
                textStatusBanner.Visibility = Visibility.Visible;
            }
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (_loading) return;

            Session.Config.WindowState = WindowState;
            Session.Config.Save();

            if (WindowState == WindowState.Normal)
            {
                if (this.RestoreBounds.Top != Session.Config.WindowTop)
                    this.Top = Session.Config.WindowTop;

                if (this.RestoreBounds.Left != Session.Config.WindowLeft)
                    this.Left = Session.Config.WindowLeft;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            // Override behavior to "Hide when minimized"

            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_loading) return;

            if (WindowState == WindowState.Normal)
            {
                Session.Config.WindowWidth = (int)Width;
                Session.Config.WindowHeight = (int)Height;
            }
        }

        private void Window_LocationChanged(object? sender, EventArgs e)
        {
            if (_loading) return;

            if (WindowState == WindowState.Normal)
            {
                if (this.RestoreBounds.Top != Session.Config.WindowTop)
                    Session.Config.WindowTop = (int)RestoreBounds.Top;

                if (this.RestoreBounds.Left != Session.Config.WindowLeft)
                    Session.Config.WindowLeft = (int)RestoreBounds.Left;
            }
        }

        private void toggleEnabled_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            ToggleUIElements(false);

            Session.DisableDeviceSleep();

            this.textStatus.Text = "Running";
            this.Title = "Session Lock Monitor - Running";
        }

        private void toggleEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            Session.EnableDeviceSleep();

            ToggleUIElements(true);

            this.textStatus.Text = "Stopped";
            this.Title = "Session Lock Monitor - Stopped";
        }

        private void toggleAutoStart_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Session.Config.AutoStart = true;
            SaveSettings();
        }

        private void toggleAutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Session.Config.AutoStart = false;
            SaveSettings();
        }

        private void autostart_callback(object? state)
        {
            UpdateEnabled(true);
        }

        private void startminimized_callback(object? state)
        {
            ForceMinimize();
        }

        private void toggleStartMinimized_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Session.Config.StartMinimized = true;
            SaveSettings();
        }

        private void toggleStartMinimized_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Session.Config.StartMinimized = false;
            SaveSettings();
        }

        public void ForceMinimize()
        {
            Debug.WriteLine("ForceMinimize Start");

            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(ForceMinimize);
                return;
            }

            if (this.WindowState != WindowState.Minimized)
                this.WindowState = WindowState.Minimized;

            
            this.Hide();
        }

        public void UpdateEnabled(bool enabled)
        {
            Debug.WriteLine("UpdateEnabled Start");

            if (!this.toggleEnabled.Dispatcher.CheckAccess())
            {
                this.toggleEnabled.Dispatcher.Invoke(UpdateEnabled, enabled);
                return;
            }

            toggleEnabled.IsChecked = enabled;
        }

        public void UpdateStatus(string status, string elapsed, string pokes)
        {
            Debug.WriteLine("UpdateStatus Start");

            if (!this.textStatus.Dispatcher.CheckAccess())
            {
                this.textStatus.Dispatcher.Invoke(UpdateStatus, status, elapsed, pokes);
                return;
            }

            this.textStatus.Text = status;
            this.textStatus.InvalidateVisual();

            this.textElapsed.Text = elapsed;
            this.textElapsed.InvalidateVisual();

            this.textPokes.Text = pokes;
            this.textPokes.InvalidateVisual();

            Debug.WriteLine(format:"UpdateStatus End! Text = {0},{1}", status, elapsed);
        }

        private void MaskMinutesInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextIsNumeric(e.Text);
        }

        private void MaskNumericInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextIsNumeric(e.Text);
        }

        private void MaskNumericPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string input = (string)e.DataObject.GetData(typeof(string));
                if (!TextIsNumeric(input)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool TextIsNumeric(string input)
        {
            return input.All(c => Char.IsDigit(c) || Char.IsControl(c));
        }

        private void textboxHours_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine(e.Changes.ToString());


            TextBox eventcontrol;

            try
            {
                eventcontrol = (TextBox)e.Source;
            }
            catch
            {
                return;
            }

            if (eventcontrol.Text.Length == 0)
                return;

            if (int.TryParse(eventcontrol.Text, out int minutes))
            {
                if (minutes > 59)
                {
                    
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                e.Changes.Clear();
                e.Handled = true;
                return;
            }
        }


        public static bool AnotherInstanceExists()
        {
            Process currentRunningProcess = Process.GetCurrentProcess();
            Process[] listOfProcs = Process.GetProcessesByName(currentRunningProcess.ProcessName);
            ProcessModule? thisapp = currentRunningProcess.MainModule;

            if (thisapp == null)
                return false;

            foreach (Process proc in listOfProcs)
            {
                if (proc.MainModule == null)
                    continue;

                if ((proc.MainModule.FileName == thisapp.FileName) && (proc.Id != currentRunningProcess.Id))
                    return true;
            }
            return false;
        }

        private void NotifyIcon_LeftClick(Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            this.Show();

            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            this.Focus();
        }
    }
}
