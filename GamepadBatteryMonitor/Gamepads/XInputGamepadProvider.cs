using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GamepadBatteryMonitor.Gamepads
{
    internal class XInputGamepadProvider : IGamepadProvider
    {
        private const string XINPUT_DLL = "xinput1_4.dll";

        private const int ERROR_SUCCESS = 0x0;
        private const int XUSER_MAX_COUNT = 4;
        private const int BATTERY_DEVTYPE_GAMEPAD = 0x00;

        private enum BATTERY_TYPE : byte
        {
            DISCONNECTED = 0x00,
            WIRED = 0x01,
            ALKALINE = 0x02,
            NIMH = 0x03,
            UNKNOWN = 0xFF
        }

        private enum BATTERY_LEVEL : byte
        {
            EMPTY = 0x00,
            LOW = 0x01,
            MEDIUM = 0x02,
            FULL = 0x03
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct XINPUT_BATTERY_INFORMATION
        {
            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(0)]
            internal BATTERY_TYPE BatteryType;

            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(1)]
            internal BATTERY_LEVEL BatteryLevel;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_VIBRATION
        {
            [MarshalAs(UnmanagedType.I2)]
            internal ushort LeftMotorSpeed;

            [MarshalAs(UnmanagedType.I2)]
            internal ushort RightMotorSpeed;
        }

        [DllImport(XINPUT_DLL)]
        private static extern int XInputGetBatteryInformation(int dwUserIndex, byte devType, ref XINPUT_BATTERY_INFORMATION pBatteryInformation);

        [DllImport(XINPUT_DLL)]
        private static extern int XInputSetState(int dwUserIndex, ref XINPUT_VIBRATION pVibration);

        
        public List<Gamepad> GetGamepadStates()
        {
            List<Gamepad> gamepads = new List<Gamepad>();
            XINPUT_BATTERY_INFORMATION batteryData = new XINPUT_BATTERY_INFORMATION();

            for (int i = 0; i < XUSER_MAX_COUNT; i++)
            {
                int result = XInputGetBatteryInformation(i, BATTERY_DEVTYPE_GAMEPAD, ref batteryData);
                if (result == ERROR_SUCCESS)
                {
                    gamepads.Add(new Gamepad(typeof(XInputGamepadProvider), String.Format(Properties.Resources.XinputGamepad, i + 1), ToBatteryLevel(batteryData), i));
                }
            }

            return gamepads;
        }

        public void NotifyLowBattery(Gamepad gamepad, NotificationConfiguration config)
        {
            if (config.Vibrate)
            {
                Task.Run(() => LowBatteryVibratePattern(gamepad));
            }
        }

        private async void LowBatteryVibratePattern(Gamepad gamepad)
        {
            Vibrate(gamepad, 1.0f);
            await Task.Delay(200);
            Vibrate(gamepad, 0.0f);
            await Task.Delay(200);
            Vibrate(gamepad, 1.0f);
            await Task.Delay(200);
            Vibrate(gamepad, 0.0f);
            await Task.Delay(200);
            Vibrate(gamepad, 1.0f);
            await Task.Delay(500);
            Vibrate(gamepad, 0.0f);

        }

        private void Vibrate(Gamepad gamepad, float level)
        {
            ushort motorLevel = (ushort) (ushort.MaxValue * level);
            XINPUT_VIBRATION vibrate = new XINPUT_VIBRATION
            {
                LeftMotorSpeed = motorLevel,
                RightMotorSpeed = motorLevel
            };

            XInputSetState((int) gamepad.Identifier, ref vibrate);
        }

        private BatteryLevel ToBatteryLevel(XINPUT_BATTERY_INFORMATION battery)
        {
            switch (battery.BatteryType)
            {
                case BATTERY_TYPE.WIRED:
                    return BatteryLevel.Wired;
                case BATTERY_TYPE.ALKALINE:
                case BATTERY_TYPE.NIMH:
                    switch (battery.BatteryLevel)
                    {
                        case BATTERY_LEVEL.EMPTY:
                            return BatteryLevel.Empty;
                        case BATTERY_LEVEL.LOW:
                            return BatteryLevel.Low;
                        case BATTERY_LEVEL.MEDIUM:
                            return BatteryLevel.Medium;
                        case BATTERY_LEVEL.FULL:
                            return BatteryLevel.Full;
                    }
                    break;

            }

            return BatteryLevel.Unknown;
        }
    }
}
