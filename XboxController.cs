using SharpDX.XInput;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace Controllers
{
    public class XboxController
    {
        private const int RefreshRate = 60; // 60 calls/second
        private const int PollingInterval = 1000 / RefreshRate; // ~16ms
        private const int ReconnectInterval = 1000; // Check every second if disconnected

        private Controller _controller;
        private CancellationTokenSource _cts;
        private bool _isConnected;
        private int _pollCounter = 0;

        public XboxController()
        {
            _controller = new Controller(UserIndex.One);
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Task.Run(() => PollController(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task PollController(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_controller.IsConnected)
                {
                    if (!_isConnected)
                    {
                        Console.WriteLine("Controller Connected!");
                        _isConnected = true;
                    }

                    Update();
                    await Task.Delay(PollingInterval, token); // Efficient sleep
                }
                else
                {
                    if (_isConnected)
                    {
                        Console.WriteLine("Controller Disconnected!");
                        _isConnected = false;
                    }
                    Console.WriteLine("Waiting for controller...");
                    await Task.Delay(ReconnectInterval, token); // Slow polling if disconnected
                }
            }
        }

        private void Update()
        {
            _controller.GetState(out var state);
            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
            {
                Console.WriteLine("A button pressed");
            }
            if (NormalizeThumb(state.Gamepad.LeftThumbY) > .2 || NormalizeThumb(state.Gamepad.LeftThumbY) < -.2)
            {
                Console.WriteLine(NormalizeThumb(state.Gamepad.LeftThumbY));
            }
        }

        private float NormalizeThumb(short value)
        {
            return Math.Clamp(value / 32767.0f, -1, 1);
        }
    }

    public class XboxToPS4
    {
        private const int RefreshRate = 60; // 60Hz
        private const int PollingInterval = 1000 / RefreshRate; // ~16ms

        private Controller _xboxController;
        private CancellationTokenSource _cts;
        private ViGEmClient _viGEmClient;
        private IDualShock4Controller _virtualDS4;

        public XboxToPS4()
        {
            _xboxController = new Controller(UserIndex.One);
            _cts = new CancellationTokenSource();
            _viGEmClient = new ViGEmClient();
            _virtualDS4 = _viGEmClient.CreateDualShock4Controller();
            _virtualDS4.Connect(); // Creates a virtual DS4 controller
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

        private void Update()
        {
            _virtualDS4.SetButtonState(DualShock4Button.Cross, true);
            _virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbX, 140);

            if (!_xboxController.GetState(out var state)) return;

            // Map Xbox Buttons to PlayStation
            _virtualDS4.SetButtonState(DualShock4Button.Cross, state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A));
            _virtualDS4.SetButtonState(DualShock4Button.Circle, state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B));
            _virtualDS4.SetButtonState(DualShock4Button.Square, state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X));
            _virtualDS4.SetButtonState(DualShock4Button.Triangle, state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y));

            // Map Triggers
            _virtualDS4.SetSliderValue(DualShock4Slider.LeftTrigger, state.Gamepad.LeftTrigger);
            _virtualDS4.SetSliderValue(DualShock4Slider.RightTrigger, state.Gamepad.RightTrigger);

            // Map Thumbsticks
            //_virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbX, Convert.ToByte(state.Gamepad.LeftThumbX / 255));
            //_virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbY, Convert.ToByte(state.Gamepad.LeftThumbY / 255));
            //_virtualDS4.SetAxisValue(DualShock4Axis.RightThumbX, Convert.ToByte(state.Gamepad.RightThumbX / 255));
            //_virtualDS4.SetAxisValue(DualShock4Axis.RightThumbY, Convert.ToByte(state.Gamepad.RightThumbY / 255));

            // Update the virtual controller
            _virtualDS4.SubmitReport();
        }
    }
}
