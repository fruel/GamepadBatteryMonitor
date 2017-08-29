using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GamepadBatteryMonitor.Gamepads;
using GamepadBatteryMonitor.WPF;
using Prism.Commands;

namespace GamepadBatteryMonitor
{
    class ConfigWindowViewModel : BaseViewModel
    {
        private readonly SystemTray _trayIcon;
        private BatteryLevel _selectedBatteryLevel;
        private bool _showNotification;
        private bool _vibrateGamepad;
        private bool _playSound;

        public ConfigWindowViewModel(SystemTray trayIcon)
        {
            _trayIcon = trayIcon;

            BatteryLevel savedLevel;
            if (Enum.TryParse(Properties.Settings.Default.notifyLevel, out savedLevel))
            {
                _selectedBatteryLevel = savedLevel;
            }

            _showNotification = Properties.Settings.Default.notifyPopup;
            _vibrateGamepad = Properties.Settings.Default.notifyVibrate;
            _playSound = Properties.Settings.Default.notifySound;
        }

        public bool IsShowNotificationEnabled
        {
            get { return _showNotification; }
            set
            {
                _showNotification = value;
                OnPropertyChanged();
            }
        }

        public bool IsVibrateGamepadEnabled
        {
            get { return _vibrateGamepad; }
            set
            {
                _vibrateGamepad = value;
                OnPropertyChanged();
            }
        }
        public bool IsPlaySoundEnabled
        {
            get { return _playSound; }
            set
            {
                _playSound = value;
                OnPropertyChanged();
            }
        }

        public BatteryLevel SelectedBatteryLevel
        {
            get { return _selectedBatteryLevel; }
            set
            {
                _selectedBatteryLevel = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<BatteryLevel> AvailableBatteryLevels
        {
            get
            {
                return new BatteryLevel[]{BatteryLevel.Full, BatteryLevel.Medium, BatteryLevel.Low, BatteryLevel.Empty};
            }
        }

        public ICommand SaveCommand
        {
            get { return new DelegateCommand<UserControl>(Save); }
        }

        public ICommand TestCommand
        {
            get { return new DelegateCommand(Test); }
        }

        private void Save(UserControl window)
        {
            Properties.Settings.Default.notifyLevel = _selectedBatteryLevel.ToString();
            Properties.Settings.Default.notifyPopup = _showNotification;
            Properties.Settings.Default.notifyVibrate = _vibrateGamepad;
            Properties.Settings.Default.notifySound = _playSound;
            Properties.Settings.Default.Save();

            if (window != null)
            {
                Window.GetWindow(window)?.Close();
            }
        }

        private void Test()
        {
            if (_trayIcon?.Monitor != null && _trayIcon.Monitor.ConnectedGamepads.Count > 0)
            {
                Gamepad testGamepad = _trayIcon.Monitor.ConnectedGamepads[0];

                NotificationConfiguration config = new NotificationConfiguration
                {
                    Popup = _showNotification,
                    Sound = _playSound,
                    Vibrate = _vibrateGamepad
                };

                _trayIcon.ShowLowNotification(testGamepad, config);
            }
        }
    }
}
