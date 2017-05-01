using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using GamepadBatteryMonitor.Gamepads;
using GamepadBatteryMonitor.Properties;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;

namespace GamepadBatteryMonitor
{
    public class SystemTray
    {
        private const int NotificationTimeout = 3;

        private NotifyIcon _notifyIcon;
        private readonly GamepadMonitor _monitor;

        public SystemTray(GamepadMonitor monitor)
        {
            _monitor = monitor;
        }

        public void Initialize()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += OpenSettings;
            _notifyIcon.Icon = Resources.TrayIcon;
            _notifyIcon.Text = Resources.Title;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = BuildContextMenu(null);

            _monitor.OnBatteryLevelChanged += UpdateTooltip;
            _monitor.OnBatteryLevelChangedToLow += ShowLowNotification;

            UpdateTooltip(null);
        }

        private void ShowLowNotification(Gamepad gamepad)
        {
            _notifyIcon.ShowBalloonTip(NotificationTimeout * 1000, Resources.Title, $"{gamepad.Name} is {gamepad.BatteryLevel}", ToolTipIcon.None);
            _monitor.SendBatteryLowNotification(gamepad);
        }

        private void UpdateTooltip(Gamepad changedGamepad)
        {
            var gamepads = _monitor.ConnectedGamepads;

            if (gamepads != null && gamepads.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (Gamepad gamepad in gamepads)
                {
                    stringBuilder.AppendLine($"{gamepad.Name}: {gamepad.BatteryLevel}");
                }
                _notifyIcon.Text = stringBuilder.ToString();
                _notifyIcon.ContextMenu = BuildContextMenu(gamepads);
            }
            else
            {
                _notifyIcon.Text = Resources.Title;
            }
        }

        private void Exit(object sender, EventArgs eventArgs)
        {
            _monitor.EndMonitoring();
            Application.Current.Shutdown();
        }

        private void OpenSettings(object sender, EventArgs eventArgs)
        {
           ConfigWindow config = new ConfigWindow();
            config.Show();
        }

        private ContextMenu BuildContextMenu(List<Gamepad> gamepads)
        {
            ContextMenu menu = new ContextMenu();

            if (gamepads != null && gamepads.Count > 0)
            {
                foreach (Gamepad gamepad in gamepads)
                {
                    menu.MenuItems.Add($"{gamepad.Name}: {gamepad.BatteryLevel}");
                }

                menu.MenuItems.Add("-");
            }

            menu.MenuItems.Add("Settings", OpenSettings);
            menu.MenuItems.Add("Exit", Exit);

            return menu;


        }

        
    }
}
