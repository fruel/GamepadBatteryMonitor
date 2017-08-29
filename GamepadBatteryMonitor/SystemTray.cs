using System;
using System.Collections.Generic;
using System.Configuration;
using System.Speech.Synthesis;
using System.Text;
using System.Windows;
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

        public GamepadMonitor Monitor
        {
            get { return _monitor; }
        }

        public void Initialize()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += OpenSettings;
            _notifyIcon.Icon = Resources.AppIcon;
            _notifyIcon.Text = Resources.Title;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = BuildContextMenu(null);

            _monitor.OnBatteryLevelChanged += UpdateTooltip;
            _monitor.OnGamepadRemoved += UpdateTooltip;
            _monitor.OnBatteryLevelChangedToLow += BatteryLevelChangedToLow;

            UpdateTooltip(null);
        }

        private void BatteryLevelChangedToLow(Gamepad gamepad)
        {
            ShowLowNotification(gamepad, GetNotificationConfiguration());
        }

        public void ShowLowNotification(Gamepad gamepad, NotificationConfiguration config)
        {
            if (config.Popup)
            {
                _notifyIcon.ShowBalloonTip(NotificationTimeout * 1000, Resources.Title, $"{gamepad.Name} is {gamepad.BatteryLevel}", ToolTipIcon.None);
            }

            if (config.Sound)
            {
                SpeechSynthesizer synthesizer = new SpeechSynthesizer
                {
                    Volume = 100,
                    Rate = 0
                };

                synthesizer.SpeakAsync($"Battery level of {gamepad.Name} is {gamepad.BatteryLevel}");
            }

            _monitor.SendBatteryLowNotification(gamepad, config);
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
                _notifyIcon.ContextMenu = BuildContextMenu(null);
                _notifyIcon.Text = Resources.Title;
            }
        }

        private void Exit(object sender, EventArgs eventArgs)
        {
            _monitor.EndMonitoring();
            _notifyIcon.Visible = false;
            Application.Current.Shutdown();
        }

        private void OpenSettings(object sender, EventArgs eventArgs)
        {
            Window config = new Window
            {
                Content = new ConfigWindowViewModel(this),
                ResizeMode = ResizeMode.NoResize,
                Title = Resources.Title,
                Width = 525,
                Height = 400
            };

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

        private NotificationConfiguration GetNotificationConfiguration()
        {
            return new NotificationConfiguration
            {
                Popup = Settings.Default.notifyPopup,
                Vibrate = Settings.Default.notifyVibrate,
                Sound = Settings.Default.notifySound
            };
        }
    }
}
