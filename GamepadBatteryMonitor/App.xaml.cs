using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GamepadBatteryMonitor.Gamepads;

namespace GamepadBatteryMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public GamepadMonitor Monitor { get; private set; }
        public SystemTray TrayIcon { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Monitor = new GamepadMonitor();
            TrayIcon =  new SystemTray(Monitor);

            Monitor.OnBatteryLevelChanged += gamepad => Console.Out.WriteLine($"{gamepad.Name}: {gamepad.BatteryLevel}");
            Monitor.BeginMonitoring();

            TrayIcon.Initialize();
        }
    }
}
