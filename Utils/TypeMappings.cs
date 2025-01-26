using Nefarius.ViGEm.Client.Targets.DualShock4;
using SharpDX.XInput;
using SharpDX.DirectInput;

namespace Controllers
{
    public static class TypeMappings
    {
        public static readonly Dictionary<string, DualShock4Button> StringToDSButton = new()
        {
            { "Square", DualShock4Button.Square },
            { "X", DualShock4Button.Square },
            { "Cross", DualShock4Button.Cross },
            { "A", DualShock4Button.Cross },
            { "Circle", DualShock4Button.Circle },
            { "B", DualShock4Button.Circle },
            { "Triangle", DualShock4Button.Triangle },
            { "Y", DualShock4Button.Triangle },
            { "ShoulderLeft", DualShock4Button.ShoulderLeft },
            { "LeftShoulder", DualShock4Button.ShoulderLeft },
            { "ShoulderRight", DualShock4Button.ShoulderRight },
            { "RightShoulder", DualShock4Button.ShoulderRight },
            { "Options", DualShock4Button.Options },
            { "Start", DualShock4Button.Options },
            { "Share", DualShock4Button.Share },
            { "Back", DualShock4Button.Share },
            { "ThumbLeft", DualShock4Button.ThumbLeft },
            { "LeftThumb", DualShock4Button.ThumbLeft },
            { "ThumbRight", DualShock4Button.ThumbRight },
            { "RightThumb", DualShock4Button.ThumbRight }
        };

        public static readonly Dictionary<GamepadButtonFlags, string> XboxButtonToString = new()
        {
            { GamepadButtonFlags.A, "A" },
            { GamepadButtonFlags.B, "B" },
            { GamepadButtonFlags.X, "X" },
            { GamepadButtonFlags.Y, "Y" },
            { GamepadButtonFlags.LeftShoulder, "LeftShoulder" },
            { GamepadButtonFlags.RightShoulder, "RightShoulder" },
            { GamepadButtonFlags.Start, "Start" },
            { GamepadButtonFlags.Back, "Back" },
            { GamepadButtonFlags.LeftThumb, "LeftThumb" },
            { GamepadButtonFlags.RightThumb, "RightThumb" }
        };

        // TODO: Check if these are correct
        public static readonly Dictionary<JoystickOffset, string> PSButtonToString = new()
        {
            { JoystickOffset.Buttons0, "Square" },
            { JoystickOffset.Buttons1, "Cross" },
            { JoystickOffset.Buttons2, "Circle" },
            { JoystickOffset.Buttons3, "Triangle" },
            { JoystickOffset.Buttons4, "ShoulderLeft" },
            { JoystickOffset.Buttons5, "ShoulderRight" },
            { JoystickOffset.Buttons6, "Options" },
            { JoystickOffset.Buttons7, "Share" },
            { JoystickOffset.Buttons8, "ThumbLeft" },
            { JoystickOffset.Buttons9, "ThumbRight" }
        };

        public static Dictionary<string, string> ConvertXboxMappingToStrings(Dictionary<GamepadButtonFlags, DualShock4Button> buttonMappings)
        {
            var result = new Dictionary<string, string>();

            foreach (var mapping in buttonMappings)
            {
                if (TypeMappings.XboxButtonToString.TryGetValue(mapping.Key, out var xboxButtonString))
                {
                    if (TypeMappings.StringToDSButton.TryGetValue(mapping.Value.ToString(), out var ds4Button))
                    {
                        result[xboxButtonString] = ds4Button.ToString();
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, string> ConvertPSMappingToStrings(Dictionary<JoystickOffset, DualShock4Button> buttonMappings)
        {
            var result = new Dictionary<string, string>();

            foreach (var mapping in buttonMappings)
            {
                if (TypeMappings.PSButtonToString.TryGetValue(mapping.Key, out var xboxButtonString))
                {
                    if (TypeMappings.StringToDSButton.TryGetValue(mapping.Value.ToString(), out var ds4Button))
                    {
                        result[xboxButtonString] = ds4Button.ToString();
                    }
                }
            }

            return result;
        }

        public static Dictionary<GamepadButtonFlags, DualShock4Button> ConvertXboxStringsToMapping(Dictionary<string, string> buttonStrings)
        {
            var result = new Dictionary<GamepadButtonFlags, DualShock4Button>();
            var reverseXboxLookup = TypeMappings.XboxButtonToString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            foreach (var mapping in buttonStrings)
            {
                if (reverseXboxLookup.TryGetValue(mapping.Key, out var xboxButton))
                {
                    if (TypeMappings.StringToDSButton.TryGetValue(mapping.Value, out var ds4Button)) 
                        result[xboxButton] = ds4Button;
                }
            }

            return result;
        }

        public static Dictionary<JoystickOffset, DualShock4Button> ConvertPSStringsToMapping(Dictionary<string, string> buttonStrings)
        {
            var result = new Dictionary<JoystickOffset, DualShock4Button>();
            var reverseXboxLookup = TypeMappings.PSButtonToString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            foreach (var mapping in buttonStrings)
            {
                if (reverseXboxLookup.TryGetValue(mapping.Key, out var psButton))
                {
                    if (TypeMappings.StringToDSButton.TryGetValue(mapping.Value, out var ds4Button))
                        result[psButton] = ds4Button;
                }
            }

            return result;
        }
    }
}
