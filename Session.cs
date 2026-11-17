using System;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;

namespace SessionControlMgr
{
    public class Session
    {
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_CONTINUOUS = 0x80000000
        }
        public SYSTEMTIMEOUTS TimeOuts
        {
            get { return sysTimeouts; }
        }
        public struct SYSTEMTIMEOUTS
        {
            public int BATTERYIDLETIMEOUT;
            public int EXTERNALIDLETIMEOUT;
            public int WAKEUPIDLETIMEOUT;
            public int SCREENSAVETIMEOUT;
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfo")]
        internal static extern int SystemParametersInfo(int uiAction, int uiParam, ref int pvParam, int fWinIni);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public delegate void PokeCallback(string status, string elapsed, string pokes);
        public delegate void UpdateUICallback(string status, string elapsed, string pokes);

        private static System.Threading.Timer? preventSleepTimer = null;
        private static System.Threading.Timer? updateUITimer = null;
        public const int SPI_GETBATTERYIDLETIMEOUT = 252;
        public const int SPI_GETEXTERNALIDLETIMEOUT = 254;
        public const int SPI_GETWAKEUPIDLETIMEOUT = 256;
        public const int SPI_GETSCREENSAVETIMEOUT = 14;
        public static int Counter = 0;
        public static int timeOutinSec = 0;
        public static int batteryIdleTimer;
        public static int externalIdleTimer;
        public static int wakeupIdleTimer;
        public static int screensaveIdleTimer;
        public static SYSTEMTIMEOUTS sysTimeouts;
        public static SettingsData Config = new();
        private static UInt32 startticks = 0;
        public static UInt32 Elapsed = 0;
        private static PokeCallback? pcb;
        private static UpdateUICallback? uicb;

        public Session()
        {
            GetSystemTimeOuts();
        }

        public static void SetPokeCallback(PokeCallback pokeCallback)
        {
            pcb = pokeCallback;

        }

        public static void SetUpdateUICallback(UpdateUICallback updateuiCallback)
        {
            uicb = updateuiCallback;
        }

        public static void ClearCallbacks()
        {
            pcb = null;
            uicb = null;
        }

        public static void DisableDeviceSleep()
        {
            GetSystemTimeOuts();

            try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to call Win32API SetThreadExecutionState(): " + e.ToString());
            }
            preventSleepTimer = new System.Threading.Timer(new TimerCallback(PokeDeviceToKeepAwake), null, 0, timeOutinSec * 1000);
            updateUITimer = new System.Threading.Timer(new TimerCallback(UpdateTick), null, 0, 15000);
            startticks = unchecked((UInt32)Environment.TickCount);
            Debug.WriteLine(format: "DisableDeviceSleep: Timeout in sec is: {0}", timeOutinSec);
        }

        public static void EnableDeviceSleep()
        {
            preventSleepTimer?.Dispose();
            preventSleepTimer = null;
            updateUITimer?.Dispose();
            updateUITimer = null;

            startticks = 0;

            try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to call Win32API SetThreadExecutionState(): " + e.ToString());
            }

            Debug.WriteLine("EnableDeviceSleep");
        }

        private static void UpdateTick(object? state)
        {
            if (startticks > 0)
                Elapsed = unchecked((UInt32)Environment.TickCount) - startticks;
            else
                Elapsed = 0;

            string statustext = string.Empty;
            string countertext = String.Format("{0} pokes", Counter);

            if (Session.Elapsed > 0)
            {
                if (Session.Elapsed < 60000)
                    statustext = String.Format("{0} second(s)", Math.Round((decimal)(Session.Elapsed / 1000)));
                else
                    statustext = String.Format("{0} minute(s)", Math.Round((decimal)(Session.Elapsed / 60000)));

                if (Session.uicb != null)
                    uicb("Running", statustext, countertext);
            }
            else
            {
                if (Session.uicb != null)
                    uicb("Running", "Waiting...", "0 pokes");
            }
        }

        private static void PokeDeviceToKeepAwake(object? state)
        {

            Counter++;

            if (Session.Config.UseSTES)
            {
                try
                {
                    SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            if (Session.Config.UseKeyCode)
            {
                try
                {
                    SendKeyPress(Session.Config.KeyCode);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            Debug.WriteLine("PokeDeviceToKeepAwake");

            if (startticks > 0)
                Elapsed = unchecked((UInt32)Environment.TickCount) - startticks;
            else
                Elapsed = 0;

            string statustext = string.Empty;
            string countertext = String.Format("{0} pokes", Counter);

            if (Session.Elapsed > 0)
            {
                if (Session.Elapsed < 60000)
                    statustext = String.Format("{0} second(s)", Math.Round((decimal)(Session.Elapsed / 1000)));
                else
                    statustext = String.Format("{0} minute(s)", Math.Round((decimal)(Session.Elapsed / 60000)));

                if (Session.pcb != null)
                    pcb("Running", statustext, countertext);

            }
            else
            {
                if (Session.pcb != null)
                    pcb("Running", "Waiting...", "1 poke");
            }

            //}
            //catch { Debug.WriteLine("PokeDeviceToKeepAwake ERROR 2"); }
        }

        /// <summary>
        ///   Sends the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void SendKeyPress(uint key)
        {
            const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

            try
            {
                keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public static void GetSystemTimeOuts()
        {
            sysTimeouts.BATTERYIDLETIMEOUT = -2;
            sysTimeouts.EXTERNALIDLETIMEOUT = -2;
            sysTimeouts.WAKEUPIDLETIMEOUT = -2;
            sysTimeouts.SCREENSAVETIMEOUT = -2;

            try
            {
                if (SystemParametersInfo(SPI_GETBATTERYIDLETIMEOUT, 0, ref batteryIdleTimer, 0) == 1)
                    sysTimeouts.BATTERYIDLETIMEOUT = batteryIdleTimer;
                else
                    sysTimeouts.BATTERYIDLETIMEOUT = -1;
            }
            catch
            {
                sysTimeouts.BATTERYIDLETIMEOUT = -1;
            }

            try
            {
                if (SystemParametersInfo(SPI_GETEXTERNALIDLETIMEOUT, 0, ref externalIdleTimer, 0) == 1)
                    sysTimeouts.EXTERNALIDLETIMEOUT = externalIdleTimer;
                else
                    sysTimeouts.EXTERNALIDLETIMEOUT = -1;
            }
            catch
            {
                sysTimeouts.EXTERNALIDLETIMEOUT = -1;
            }

            try
            {
                if (SystemParametersInfo(SPI_GETWAKEUPIDLETIMEOUT, 0, ref wakeupIdleTimer, 0) == 1)
                    sysTimeouts.WAKEUPIDLETIMEOUT = wakeupIdleTimer;
                else
                    sysTimeouts.WAKEUPIDLETIMEOUT = -1;
            }
            catch
            {
                sysTimeouts.BATTERYIDLETIMEOUT = -1;
            }

            try
            {
                if (SystemParametersInfo(SPI_GETSCREENSAVETIMEOUT, 0, ref screensaveIdleTimer, 0) == 1)
                    sysTimeouts.SCREENSAVETIMEOUT = screensaveIdleTimer;
                else
                    sysTimeouts.SCREENSAVETIMEOUT = -1;
            }
            catch
            {
                sysTimeouts.SCREENSAVETIMEOUT = -1;
            }

            timeOutinSec = 61;

            if (sysTimeouts.BATTERYIDLETIMEOUT > 0 && timeOutinSec > sysTimeouts.BATTERYIDLETIMEOUT)
                timeOutinSec = sysTimeouts.BATTERYIDLETIMEOUT;
            if (sysTimeouts.EXTERNALIDLETIMEOUT > 0 && timeOutinSec > sysTimeouts.EXTERNALIDLETIMEOUT)
                timeOutinSec = sysTimeouts.EXTERNALIDLETIMEOUT;
            if (sysTimeouts.WAKEUPIDLETIMEOUT > 0 && timeOutinSec > sysTimeouts.WAKEUPIDLETIMEOUT)
                timeOutinSec = sysTimeouts.WAKEUPIDLETIMEOUT;
            if (sysTimeouts.SCREENSAVETIMEOUT > 0 && timeOutinSec > sysTimeouts.SCREENSAVETIMEOUT)
                timeOutinSec = sysTimeouts.SCREENSAVETIMEOUT;

            if (timeOutinSec < 2)
                timeOutinSec = 2;

            timeOutinSec -= 1;

            Debug.WriteLine("Lowest common timeout (sec): {0}", timeOutinSec);
        }
    }
}
