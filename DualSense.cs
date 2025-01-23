using System;
using System.Threading;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using SharpDX.DirectInput;

namespace Controllers
{
    public class PSToPS4
    {
        private const int RefreshRate = 60; // 60Hz
        private const int PollingInterval = 1000 / RefreshRate; // ~16ms

        private IDualShock4Controller _virtualDS4;
        private ViGEmClient _viGEmClient;
        private CancellationTokenSource _cts;
        private DirectInput _directInput;
        private Joystick? _controller;

        public PSToPS4()
        {
            _viGEmClient = new ViGEmClient();
            _virtualDS4 = _viGEmClient.CreateDualShock4Controller();
            _virtualDS4.Connect(); // Creates a virtual DS4 controller
            _cts = new CancellationTokenSource();
            _directInput = new DirectInput();
            _controller = null;
        }

        public void Start()
        {
            Console.WriteLine("Starting PS4 to PS4 Controller Bridge...");
            Task.Run(() => PollController(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _virtualDS4.Disconnect();
            _controller?.Unacquire();
        }

        private async Task PollController(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_controller != null)
                {
                    Update();
                    await Task.Delay(PollingInterval, token);
                }
                else
                {
                    Console.WriteLine("PS Controller Disconnected, Searching...");
                    SearchForController();
                    await Task.Delay(1000, token); // Slow polling when disconnected
                }
            }
        }
        private void Update()
        {
            // Console.WriteLine("Updating PS4 Controller...");
            try
            {
                // do nothing if controller is null
                if (_controller == null)
                    return;

                // Poll the controller for data
                _controller!.Poll();

                // Get the buffered data
                var datas = _controller.GetBufferedData();
                foreach (var state in datas)
                {
                    Console.WriteLine(state);
                    MapControllerData(state);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");

                // Shit hit the fan, disconnect the controller and set it to null
                if (_controller != null)
                {
                    _controller.Unacquire();
                    _controller.Dispose();
                    _controller = null;
                }
            }
        }

        private void SearchForController()
        {   
            // Look through all connected devices for a controller connected
            foreach (var deviceInstance in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                _controller = new Joystick(_directInput, deviceInstance.InstanceGuid);

                // Enable buffered mode to retrieve buffered data
                _controller.Properties.BufferSize = 32;  // Set buffer size (adjust as needed)

                // Acquire the device
                _controller.Acquire();

                Console.WriteLine($"Found PS Controller: {_controller.Information.InstanceName}");
                Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", _controller.Information.InstanceGuid);
                break;
            }
               
            // If no controller is found, return
            if (_controller == null)
            {
                Console.WriteLine("No PS Controller found!");
                return;
            }

            // Set the data format to the controller
            _controller.Acquire();
        }

        // Reads the controller data and maps it to the virtual controller 1:1 
        void MapControllerData(JoystickUpdate state)
        {
            // Map PS4 Buttons to Virtual PS4
            // Buttons: 0 (released) 128 (pressed)
            switch (state.Offset)
            {
                // Square Button 
                case JoystickOffset.Buttons0:
                    _virtualDS4.SetButtonState(DualShock4Button.Square, state.Value == 128);
                    break;
                // Cross Button
                case JoystickOffset.Buttons1:
                    _virtualDS4.SetButtonState(DualShock4Button.Cross, state.Value == 128);
                    break;
                // Circle Button
                case JoystickOffset.Buttons2:
                    _virtualDS4.SetButtonState(DualShock4Button.Circle, state.Value == 128);
                    break;
                // Triangle Button
                case JoystickOffset.Buttons3:
                    _virtualDS4.SetButtonState(DualShock4Button.Triangle, state.Value == 128);
                    break;
                // L1 Button
                case JoystickOffset.Buttons4:
                    _virtualDS4.SetButtonState(DualShock4Button.ShoulderLeft, state.Value == 128);
                    break;
                // R1 Button
                case JoystickOffset.Buttons5:
                    _virtualDS4.SetButtonState(DualShock4Button.ShoulderRight, state.Value == 128);
                    break;
                // L2 Button
                case JoystickOffset.RotationX:
                    _virtualDS4.SetSliderValue(DualShock4Slider.LeftTrigger, Convert.ToByte((state.Value / (double)ushort.MaxValue) * 255));
                    break;
                // R2 Button
                case JoystickOffset.RotationY:
                    _virtualDS4.SetSliderValue(DualShock4Slider.RightTrigger, Convert.ToByte((state.Value / (double)ushort.MaxValue) * 255));
                    break;
                // LS X-Axis 
                case JoystickOffset.X:
                    _virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbX, Convert.ToByte((state.Value / (double)ushort.MaxValue) * 255));
                    break;
                // LS Y-Axis
                case JoystickOffset.Y:
                    _virtualDS4.SetAxisValue(DualShock4Axis.LeftThumbY, Convert.ToByte((state.Value / (double)ushort.MaxValue) * 255));
                    break;
                // RS X-Axis
                case JoystickOffset.RotationZ:
                    _virtualDS4.SetAxisValue(DualShock4Axis.RightThumbY, Convert.ToByte((state.Value / (double)ushort.MaxValue) * 255));
                    break;
                // RS Y-Axis
                case JoystickOffset.Z:
                    _virtualDS4.SetAxisValue(DualShock4Axis.RightThumbX, Convert.ToByte((state.Value / (double)ushort.MaxValue) * 255));
                    break;
            }

            // Update the virtual controller
            _virtualDS4.SubmitReport();
        }
    }

}