using Nefarius.ViGEm.Client.Targets.DualShock4;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controllers
{
    public class ControllerConfiguration
    {
        // Specialize ControllerSection for Xbox and PlayStation
        public class XboxControllerSection : ControllerSection<GamepadButtonFlags> { }
        public class PlayStationControllerSection : ControllerSection<JoystickOffset> { }

        public XboxControllerSection Xbox { get; set; } = new();
        public PlayStationControllerSection PlayStation { get; set; } = new();

        public class ControllerSection<ButtonType>
        {
            public Dictionary<ButtonType, DualShock4Button> ButtonMappings { get; set; } = new();
            public Dictionary<string, string> ButtonStrings { get; set; } = new();
            public Dictionary<string, MacroDefinition> Macros { get; set; } = new();
        }

        public class MacroDefinition
        {
            public List<MacroStep> Steps { get; set; } = new();
        }

        public class MacroStep
        {
            public string ActionType { get; set; } = "";
            public Dictionary<string, object> Parameters { get; set; } = new();
        }

    }
}
