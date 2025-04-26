# Controller Bridge
*Note: This is still a work in progress.*

The Controller Bridge is a simple C# library that enables a programmer to input an Xbox or Playstation DS4/DS5 controller into their system and enable a programmer to perform the following:

- Remap Controller Buttons
    - Example: A->B or Left-Bumper -> Left-Trigger
- Create Custom Macros 
    - Example: Hold RT while moving the Right Stick down for 5 seconds
- Output Functionality
    - Example: Pressing the B button will call a custom function

## Usage
This library allows you to integrate gaming controllers into your custom software without needing to worry about the specifics of ViGemBus or SharpDX, enabling you to program at a higher level.

I originally developed this tool as part of another project, which used computer vision to enhance console gameplay with autonomous features powered by machine learning models. I realized that the controller bridge would be more valuable as a standalone tool rather than just a feature within that project.

## Requirements
To compile and use within your own project, you will need the following dependencies:

- [ViGemBus by nefarius](https://github.com/nefarius/ViGEmBus/releases) — *Required for driver-based controller emulation*
- [SharpDX by xoofx](https://github.com/sharpdx/SharpDX) — *Note: You can also install from the NuGet Package Manager*

None of this would be possible without their innovative hard work