using SharpDX.XInput;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using System.Diagnostics;

namespace Controllers
{
    public class XboxToPS4
    {
        private const int RefreshRate = 60; // 60Hz
        private const int PollingInterval = 1000 / RefreshRate; // ~16ms

        private Controller _xboxController;
        private CancellationTokenSource _cts;
        private ViGEmClient _viGEmClient;
        private IDualShock4Controller _virtualDS4;

        private Dictionary<GamepadButtonFlags, List<Action>> _macros = new();
        private Dictionary<GamepadButtonFlags, Task> _activeMacros = new(); // Track active macros to prevent overlapping
        private Dictionary<DualShock4Button, bool> _overriddenButtons = new();
        private Dictionary<GamepadButtonFlags, DualShock4Button> _buttonRemappings = new();


        public XboxToPS4()
        {
            _xboxController = new Controller(UserIndex.One);
            _cts = new CancellationTokenSource();
            _viGEmClient = new ViGEmClient();
            _virtualDS4 = _viGEmClient.CreateDualShock4Controller();
            _virtualDS4.Connect();                                      // Creates a virtual DS4 controller
            LoadConfiguration();
        }

        public void Start()
        {
            Console.WriteLine("Starting Xbox to PS4 Controller Bridge...");
            Task.Run(() => PollController(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _virtualDS4.Disconnect();
        }

        public void LoadConfiguration()
        {
            var config = ConfigurationManager.Load();
            _buttonRemappings = config.Xbox.ButtonMappings;
        }

        private async Task PollController(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_xboxController.IsConnected)
                {
                    Update();
                    await Task.Delay(PollingInterval, token);
                }
                else
                {
                    Console.WriteLine("Xbox Controller Disconnected!");
                    await Task.Delay(1000, token); // Slow polling
                }
            }
        }

        private Byte NormalizeThumb(short value)
        {
            return (Byte)((value + 32768) / 257);
        }

        private void Update()
        {
            if (!_xboxController.GetState(out var state)) return;

            // Check for macro triggers
            foreach (var macro in _macros)
            {
                var button = macro.Key;
                if (state.Gamepad.Buttons.HasFlag(button))
                {
                    // Only trigger the macro if it's not already running
                    if (!_activeMacros.TryGetValue(button, out var runningTask) || runningTask.IsCompleted)
                    {
                        // Start the macro and track it
                        var macroTask = RunMacroAsync(macro.Value, button);
                        _activeMacros[button] = macroTask;
                    }
                }
            }

            // Process other inputs (remapping, triggers, thumbsticks, etc.)
            ProcessInputs(state);

            // Update the virtual controller
            _virtualDS4.SubmitReport();
        }

        private async Task RunMacroAsync(List<Action> macroActions, GamepadButtonFlags button)
        {
            try
            {
                foreach (var action in macroActions)
                {
                    action.Invoke();
                    await Task.Delay(1); // Non-blocking delay
                }
            }
            finally
            {
                // Remove the macro from tracking when done (even if it errors)
                _activeMacros.Remove(button);
            }
        }

        private void ProcessInputs(State state)
        {
            foreach (var mapping in _buttonRemappings)
            {
                var dualShockButton = mapping.Value;

                // Button down only if its not being overridden by a macro
                if (!_overriddenButtons.ContainsKey(dualShockButton))
                {   
                    _virtualDS4.SetButtonState(dualShockButton, state.Gamepad.Buttons.HasFlag(mapping.Key));
                }
            }

            // Directional Input
            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp | GamepadButtonFlags.DPadLeft))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.Northwest);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp | GamepadButtonFlags.DPadRight))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.Northeast);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown | GamepadButtonFlags.DPadLeft))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.Southwest);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown | GamepadButtonFlags.DPadRight))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.Southeast);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.North);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.South);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.West);
            else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.East);
            else
                _virtualDS4.SetDPadDirection(DualShock4DPadDirection.None);

            // Map Triggers
            _virtualDS4.SetSliderValue(DualShock4Slider.LeftTrigger, state.Gamepad.LeftTrigger);
            _virtualDS4.SetSliderValue(DualShock4Slider.RightTrigger, state.Gamepad.RightTrigger);

            // Map Thumbsticks
            _virtualDS4.SetAxisValue(DualShock4Axis.RightThumbX, NormalizeThumb(state.Gamepad.RightThumbX));
            _virtualDS4.SetAxisValue(DualShock4Axis.RightThumbY, (byte)(255 - NormalizeThumb(state.Gamepad.RightThumbY)));
            _virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbX, NormalizeThumb(state.Gamepad.LeftThumbX));
            _virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbY, (byte)(255 - NormalizeThumb(state.Gamepad.LeftThumbY)));
        }
    }
}
