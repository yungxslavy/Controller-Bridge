using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using SharpDX.DirectInput;
using SharpDX.XInput;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace Controllers
{
    public static class ConfigurationManager
    {
        private const string DefaultFileName = "controller_config.json";

        private class SaveData
        {
            public Dictionary<string, string>? XboxMappings { get; set; }
            public Dictionary<string, string>? PSMappings { get; set; }

            public SaveData() { }

            public SaveData(ControllerConfiguration config)
            {
                XboxMappings = config.Xbox.ButtonStrings;
                PSMappings = config.PlayStation.ButtonStrings;
            }
        }

        public static void SaveConfig(ControllerConfiguration config, string path)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            SaveData data = new(config);
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }

        public static ControllerConfiguration Load(string filePath = DefaultFileName)
        {
            try
            {
                ControllerConfiguration controllerConfiguration = new();

                // Create default if file does not exist
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Configuration file not found, creating default configuration...");
                    controllerConfiguration = CreateDefaultConfiguration();
                    SaveConfig(controllerConfiguration, filePath);
                    return controllerConfiguration;
                }

                Console.WriteLine("Loading configuration file...");
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                
                if (data == null || data.XboxMappings == null || data.PSMappings == null)
                {
                    Console.WriteLine("Failed to load configuration file: Data is null");
                    Console.WriteLine("Creating default configuration...");
                    return CreateDefaultConfiguration();
                }

                // Load configuration data to our controller configuration object
                controllerConfiguration.Xbox.ButtonStrings = data.XboxMappings;
                controllerConfiguration.Xbox.ButtonMappings = TypeMappings.ConvertXboxStringsToMapping(data.XboxMappings);
                controllerConfiguration.PlayStation.ButtonStrings = data.PSMappings;
                controllerConfiguration.PlayStation.ButtonMappings = TypeMappings.ConvertPSStringsToMapping(data.PSMappings);

                return controllerConfiguration;
            }
            catch (Exception e)
            {
                // Log error and return default configuration
                Console.WriteLine($"Failed to load configuration file: {e.Message}");
                return CreateDefaultConfiguration();
            }
        }

        public static ControllerConfiguration CreateDefaultConfiguration()
        {
            var config = new ControllerConfiguration();

            // Default Xbox button mappings
            config.Xbox.ButtonMappings = new Dictionary<GamepadButtonFlags, DualShock4Button>
            {
                { GamepadButtonFlags.A, DualShock4Button.Cross },
                { GamepadButtonFlags.B, DualShock4Button.Circle },
                { GamepadButtonFlags.X, DualShock4Button.Square },
                { GamepadButtonFlags.Y, DualShock4Button.Triangle },
                { GamepadButtonFlags.LeftShoulder, DualShock4Button.ShoulderLeft },
                { GamepadButtonFlags.RightShoulder, DualShock4Button.ShoulderRight },
                { GamepadButtonFlags.Start, DualShock4Button.Options },
                { GamepadButtonFlags.Back, DualShock4Button.Share },
                { GamepadButtonFlags.LeftThumb, DualShock4Button.ThumbLeft },
                { GamepadButtonFlags.RightThumb, DualShock4Button.ThumbRight }
            };

            config.Xbox.ButtonStrings = TypeMappings.ConvertXboxMappingToStrings(config.Xbox.ButtonMappings);

            // Default PlayStation button mappings TODO: ENSURE THESE ARE CORRECT
            config.PlayStation.ButtonMappings = new Dictionary<JoystickOffset, DualShock4Button>
            {
                { JoystickOffset.Buttons0, DualShock4Button.Square },
                { JoystickOffset.Buttons1, DualShock4Button.Cross },
                { JoystickOffset.Buttons2, DualShock4Button.Circle },
                { JoystickOffset.Buttons3, DualShock4Button.Triangle },
                { JoystickOffset.Buttons4, DualShock4Button.ShoulderLeft },
                { JoystickOffset.Buttons5, DualShock4Button.ShoulderRight },
                { JoystickOffset.Buttons8, DualShock4Button.Options },
                { JoystickOffset.Buttons9, DualShock4Button.Share },
                { JoystickOffset.Buttons10, DualShock4Button.ThumbLeft },
                { JoystickOffset.Buttons11, DualShock4Button.ThumbRight }
            };

            config.PlayStation.ButtonStrings = TypeMappings.ConvertPSMappingToStrings(config.PlayStation.ButtonMappings);

            // Default Xbox macros
            config.Xbox.Macros.Add("X", new ControllerConfiguration.MacroDefinition
            {
                Steps = new List<ControllerConfiguration.MacroStep>
                {
                    new()
                    {
                        ActionType = "PressButton",
                        Parameters = new Dictionary<string, object> { { "Button", "Square" } }
                    },
                    new()
                    {
                        ActionType = "Delay",
                        Parameters = new Dictionary<string, object> { { "Duration", 4700 } }
                    },
                    new()
                    {
                        ActionType = "ReleaseButton",
                        Parameters = new Dictionary<string, object> { { "Button", "Square" } }
                    }
                }
            });

            return config;
        }
    }
}