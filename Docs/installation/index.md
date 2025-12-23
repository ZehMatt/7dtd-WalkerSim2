# Installation

WalkerSim 2 is a server-side mod for 7 Days to Die. This means:

- **Dedicated Servers**: Install WalkerSim 2 on the server
- **Single Player / Local Hosting**: Install WalkerSim 2 as you would a client mod (since the client acts as the server in offline play)
- **Clients Connecting to Servers**: No installation required - clients can join servers running WalkerSim 2 without having the mod installed themselves

## Installation Options

Choose your installation type:

### For Players (Client/Single Player)
If you're playing single-player or hosting a local game:

- [Windows Installation](client/windows.md)
- [Linux Installation](client/linux.md)
- [MacOS Installation](client/macos.md)

### For Server Administrators
If you're running a dedicated server:

- [Windows Server Installation](server/windows.md)
- [Linux Server Installation](server/linux.md)

## After Installation

Once installed, WalkerSim 2 is ready to use with its preconfigured settings. The mod includes a default `WalkerSim.xml` configuration file that provides balanced simulation parameters suitable for most gameplay scenarios.

You can start playing immediately with these defaults, or customize the simulation using:

- **The Editor Tool**: A visual configuration tool located in the mod's installation directory
- **Direct XML Editing**: Modify the `WalkerSim.xml` file with any text editor

For detailed configuration instructions, see the [Configuring the Simulation](../configuring/index.md) section.

## Important Notes

- **EAC (Easy Anti-Cheat) must be disabled** when using mods in 7 Days to Die
- WalkerSim 2 requires the `0_TFP_Harmony` mod, which is included with the game by default
- The mod creates a persistent simulation that runs continuously - agents exist in the virtual world even when no players are nearby
