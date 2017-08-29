using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GamepadBatteryMonitor.Gamepads
{
    public enum BatteryLevel
    {
        Empty, Low, Medium, Full, Wired, Unknown
    }

    public class NotificationConfiguration
    {
        public bool Popup;
        public bool Vibrate;
        public bool Sound;
    }

    public class Gamepad
    {
        public Gamepad(Type dataProvider, string name, BatteryLevel batteryLevel, object identifier)
        {
            DataProvider = dataProvider;
            Name = name;
            BatteryLevel = batteryLevel;
            Identifier = identifier;
        }

        public Type DataProvider { get; }
        public string Name { get; }
        public BatteryLevel BatteryLevel { get; }

        public object Identifier { get; }

        protected bool Equals(Gamepad other)
        {
            return DataProvider == other.DataProvider && Equals(Identifier, other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Gamepad) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DataProvider != null ? DataProvider.GetHashCode() : 0) * 397) ^ (Identifier != null ? Identifier.GetHashCode() : 0);
            }
        }
    }

    internal interface IGamepadProvider
    {
        List<Gamepad> GetGamepadStates();
        void NotifyLowBattery(Gamepad gamepad, NotificationConfiguration config);
    }

    public delegate void GamepadEvent(Gamepad gamepad);

    public class GamepadMonitor
    {
        private const int BatteryMonitorInterval = 5;
        private readonly IList<IGamepadProvider> _gamepadProviders = new ReadOnlyCollection<IGamepadProvider>(new List<IGamepadProvider>()
        {
            new XInputGamepadProvider()
        });
        
        private Timer _gamepadMonitorTimer;

        public event GamepadEvent OnBatteryLevelChanged;
        public event GamepadEvent OnGamepadRemoved;
        public event GamepadEvent OnBatteryLevelChangedToLow;

        public GamepadMonitor()
        {
            ConnectedGamepads = new List<Gamepad>();
        }

        public List<Gamepad> ConnectedGamepads { get; private set; }

        public bool IsRunning => _gamepadMonitorTimer != null;

        public void BeginMonitoring()
        {
            if (_gamepadMonitorTimer != null)
            {
                EndMonitoring();
            }

            _gamepadMonitorTimer = new Timer();
            _gamepadMonitorTimer.Elapsed += (sender, args) => MonitorBatteryLevels();
            _gamepadMonitorTimer.Interval = BatteryMonitorInterval * 1000;
            _gamepadMonitorTimer.AutoReset = true;
            _gamepadMonitorTimer.Enabled = true;

            MonitorBatteryLevels();
        }

        public void EndMonitoring()
        {
            if (_gamepadMonitorTimer != null)
            {
                _gamepadMonitorTimer.Stop();
                _gamepadMonitorTimer = null;
            }
        }

        private void MonitorBatteryLevels()
        {
            BatteryLevel triggerLevel = BatteryLevel.Empty;
            Enum.TryParse(Properties.Settings.Default.notifyLevel, out triggerLevel);

            List<Gamepad> oldGamepadStates = ConnectedGamepads;
            ConnectedGamepads = new List<Gamepad>();

            foreach(var provider in _gamepadProviders)
            {
                ConnectedGamepads.AddRange(provider.GetGamepadStates());
            }

            foreach (Gamepad gamepad in ConnectedGamepads)
            {
                Gamepad oldGamepad = oldGamepadStates.Find(g => g.Equals(gamepad));

                if (oldGamepad != null)
                {
                    if (gamepad.BatteryLevel != oldGamepad.BatteryLevel)
                    {
                        OnBatteryLevelChanged?.Invoke(gamepad);

                        if (gamepad.BatteryLevel == triggerLevel && oldGamepad.BatteryLevel != BatteryLevel.Unknown && oldGamepad.BatteryLevel > triggerLevel)
                        {
                            OnBatteryLevelChangedToLow?.Invoke(gamepad);
                        }
                    }
                }
                else
                {
                    OnBatteryLevelChanged?.Invoke(gamepad);
                }
            }

            foreach (var gamepad in oldGamepadStates.Except(ConnectedGamepads))
            {
                OnGamepadRemoved?.Invoke(gamepad);
            }
        }

        public void SendBatteryLowNotification(Gamepad gamepad, NotificationConfiguration config)
        {
            GetOwningProvider(gamepad).NotifyLowBattery(gamepad, config);
        }

        internal IGamepadProvider GetOwningProvider(Gamepad gamepad)
        {
            foreach (IGamepadProvider provider in _gamepadProviders)
            {
                if (provider.GetType() == gamepad.DataProvider)
                {
                    return provider;
                }
            }
            return null;
        }

    }
}
