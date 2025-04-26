# Configuring the Simulation

WalkerSim 2 comes with a default configuration file named `WalkerSim.xml`.  
Whenever a game is started, the mod attempts to load the configuration using the following priority:

1. **World-Specific Configuration**
    - First, WalkerSim 2 looks for a `WalkerSim.xml` inside the world’s save directory.
    - This allows you to create custom WalkerSim 2 configurations tied specifically to individual worlds.
    - For example, pre-generated worlds from 7 Days to Die (typically found under `Data/Worlds`) can include a custom `WalkerSim.xml`.

2. **Default Mod Configuration**
    - If no world-specific configuration is found, WalkerSim 2 will fall back to the default configuration located at:
     ```
     Mods/WalkerSim2/WalkerSim.xml
     ```

This structure allows full flexibility: you can have global defaults, or fine-tune the simulation behavior per world if desired.
