using System;
using System.Windows;

namespace SessionControlMgr
{
    public class SettingsData
    {
        //public int Hours { get; set; }
        //public int Minutes { get; set; }
        //public bool UseSTES { get; set; }
        //public uint KeyCode { get; set; }
        //public bool UseKeyCode { get; set; }
        //public uint Action { get; set; }
        //public DateTime TimeFrom { get; set; }
        //public DateTime TimeTo { get; set; }
        
        //public int WindowLeft { get; set; }
        //public int WindowTop { get; set; }
        //public int WindowWidth { get; set; }
        //public int WindowHeight { get; set; }
        //public WindowState WindowState { get; set; }

        public int Hours { get { return Properties.SessionControlMgr.Default.Hours; } set { Properties.SessionControlMgr.Default.Hours = value; } }
        public int Minutes { get { return Properties.SessionControlMgr.Default.Minutes; } set { Properties.SessionControlMgr.Default.Minutes = value; } }
        public bool UseSTES { get { return Properties.SessionControlMgr.Default.UseSTES; } set { Properties.SessionControlMgr.Default.UseSTES = value; } }
        public uint KeyCode { get { return Properties.SessionControlMgr.Default.KeyCode; } set { Properties.SessionControlMgr.Default.KeyCode = value; } }
        public bool UseKeyCode { get { return Properties.SessionControlMgr.Default.UseKeyCode; } set { Properties.SessionControlMgr.Default.UseKeyCode = value; } }
        public uint Action { get { return Properties.SessionControlMgr.Default.Action; } set { Properties.SessionControlMgr.Default.Action = value; } }
        public DateTime TimeFrom { get { return Properties.SessionControlMgr.Default.TimeFrom; } set { Properties.SessionControlMgr.Default.TimeFrom = value; } }
        public DateTime TimeTo { get { return Properties.SessionControlMgr.Default.TimeTo; } set { Properties.SessionControlMgr.Default.TimeTo = value; } }
        public bool AutoStart { get { return Properties.SessionControlMgr.Default.AutoStart; } set { Properties.SessionControlMgr.Default.AutoStart = value; } }
        public bool StartMinimized { get { return Properties.SessionControlMgr.Default.StartMinimized; } set { Properties.SessionControlMgr.Default.StartMinimized = value; } }
        
        public int WindowLeft { get { return Properties.SessionControlMgr.Default.WindowLeft; } set { Properties.SessionControlMgr.Default.WindowLeft = value; } }
        public int WindowTop { get { return Properties.SessionControlMgr.Default.WindowTop; } set { Properties.SessionControlMgr.Default.WindowTop = value; } }
        public int WindowWidth { get { return Properties.SessionControlMgr.Default.WindowWidth; } set { Properties.SessionControlMgr.Default.WindowWidth = value; } }
        public int WindowHeight { get { return Properties.SessionControlMgr.Default.WindowHeight; } set { Properties.SessionControlMgr.Default.WindowHeight = value; } }
        public WindowState WindowState { get { return (WindowState)Properties.SessionControlMgr.Default.WindowState; } set { Properties.SessionControlMgr.Default.WindowState = (int)value; } }

        public SettingsData()
        {
            Load();
        }

        public void Load()
        {
            //Action = Properties.SessionControlMgr.Default.Action;
            //Hours = Properties.SessionControlMgr.Default.Hours;
            //Minutes = Properties.SessionControlMgr.Default.Minutes;
            //TimeFrom = Properties.SessionControlMgr.Default.TimeFrom;
            //TimeTo = Properties.SessionControlMgr.Default.TimeTo;
            //KeyCode = Properties.SessionControlMgr.Default.KeyCode;
            //UseKeyCode = Properties.SessionControlMgr.Default.UseKeyCode;
            //UseSTES = Properties.SessionControlMgr.Default.UseSTES;

            //WindowLeft = Properties.SessionControlMgr.Default.WindowLeft;
            //WindowTop = Properties.SessionControlMgr.Default.WindowTop;
            //WindowWidth = Properties.SessionControlMgr.Default.WindowWidth;
            //WindowHeight = Properties.SessionControlMgr.Default.WindowHeight;
            //WindowState = Properties.SessionControlMgr.Default.WindowState;

            Properties.SessionControlMgr.Default.Reload();


        }

        public void Save()
        {
            //Properties.SessionControlMgr.Default.Action = Action;
            //Properties.SessionControlMgr.Default.Hours = Hours;
            //Properties.SessionControlMgr.Default.Minutes = Minutes;
            //Properties.SessionControlMgr.Default.TimeFrom = TimeFrom;
            //Properties.SessionControlMgr.Default.TimeTo = TimeTo;

            //Properties.SessionControlMgr.Default.UseSTES = UseSTES;
            //Properties.SessionControlMgr.Default.UseKeyCode = UseKeyCode;
            //Properties.SessionControlMgr.Default.KeyCode = KeyCode;

            //Properties.SessionControlMgr.Default.WindowLeft = WindowLeft;
            //Properties.SessionControlMgr.Default.WindowTop = WindowTop;
            //Properties.SessionControlMgr.Default.WindowWidth = WindowWidth;
            //Properties.SessionControlMgr.Default.WindowHeight = WindowHeight;
            //Properties.SessionControlMgr.Default.WindowState = WindowState;

            Properties.SessionControlMgr.Default.Save();
        }
    }

    public static class StaticSettingsData
    {
        public static int Hours { get; set; }
        public static int Minutes { get; set; }
        public static bool UseSTES { get; set; }
        public static uint KeyCode { get; set; }
        public static bool UseKeyCode { get; set; }
        public static uint Action { get; set; }
        public static DateTime TimeFrom { get; set; }
        public static DateTime TimeTo { get; set; }

        public static int WindowLeft { get; set; }
        public static int WindowTop { get; set; }
        public static int WindowWidth { get; set; }
        public static int WindowHeight { get; set; }
        public static WindowState WindowState { get; set; }

        public static void Load()
        {
            Action = Properties.SessionControlMgr.Default.Action;
            Hours = Properties.SessionControlMgr.Default.Hours;
            Minutes = Properties.SessionControlMgr.Default.Minutes;
            TimeFrom = Properties.SessionControlMgr.Default.TimeFrom;
            TimeTo = Properties.SessionControlMgr.Default.TimeTo;
            KeyCode = Properties.SessionControlMgr.Default.KeyCode;
            UseKeyCode = Properties.SessionControlMgr.Default.UseKeyCode;
            UseSTES = Properties.SessionControlMgr.Default.UseSTES;

            WindowLeft = Properties.SessionControlMgr.Default.WindowLeft;
            WindowTop = Properties.SessionControlMgr.Default.WindowTop;
            WindowWidth = Properties.SessionControlMgr.Default.WindowWidth;
            WindowHeight = Properties.SessionControlMgr.Default.WindowHeight;
            WindowState = (WindowState)Properties.SessionControlMgr.Default.WindowState;
        }

        public static void Save()
        {
            Properties.SessionControlMgr.Default.Action = Action;
            Properties.SessionControlMgr.Default.Hours = Hours;
            Properties.SessionControlMgr.Default.Minutes =  Minutes;
            Properties.SessionControlMgr.Default.TimeFrom = TimeFrom;
            Properties.SessionControlMgr.Default.TimeTo = TimeTo;

            Properties.SessionControlMgr.Default.UseSTES = UseSTES;
            Properties.SessionControlMgr.Default.UseKeyCode = UseKeyCode;
            Properties.SessionControlMgr.Default.KeyCode = KeyCode;

            Properties.SessionControlMgr.Default.WindowLeft = WindowLeft;
            Properties.SessionControlMgr.Default.WindowTop = WindowTop;
            Properties.SessionControlMgr.Default.WindowWidth = WindowWidth;
            Properties.SessionControlMgr.Default.WindowHeight = WindowHeight;
            Properties.SessionControlMgr.Default.WindowState = (int)WindowState;

            Properties.SessionControlMgr.Default.Save();
        }
    }
}
