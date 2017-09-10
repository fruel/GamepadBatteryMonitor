using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadBatteryMonitor.Gamepads
{
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
            return Equals((Gamepad)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DataProvider != null ? DataProvider.GetHashCode() : 0) * 397) ^ (Identifier != null ? Identifier.GetHashCode() : 0);
            }
        }
    }
}
