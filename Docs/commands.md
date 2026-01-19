# Console Commands

WalkerSim 2 provides several console commands to control and monitor the simulation. All commands start with `walkersim` followed by a subcommand.

!!! warning "Admin Privileges Required"
    All WalkerSim console commands require administrator privileges. On multiplayer servers, only players with admin rights can execute these commands.

## How to Use Commands

1. Open the console in-game by pressing **F1**
2. Type `walkersim` followed by one of the commands below
3. Press Enter to execute

**Example**: `walkersim pause`

---

## Available Commands

### show

**Usage**: `walkersim show`

**What it does**: Opens the map window and temporarily enables the overlay showing virtual zombie positions.

**Details**: The overlay will only remain visible while the map is open. If you close and reopen the map, the overlay will be gone. To keep the overlay enabled permanently, use `walkersim map enable`.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode. The map overlay feature is not available on multiplayer servers.

---

### map

**Usage**: `walkersim map <enable|disable>`

**What it does**: Enables or disables the simulation overlay in the map window.

**Arguments**:

- `enable` (or `true` or `1`) - Turns on the overlay permanently
- `disable` (or `false` or `0`) - Turns off the overlay

**Examples**:
```
walkersim map enable
walkersim map disable
walkersim map true
walkersim map 0
```

**Details**: When enabled, the map overlay shows the positions and movements of virtual zombies in the simulation. This is useful for understanding how zombies are distributed across the map.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode. The map overlay feature is not available on multiplayer servers.

---

### pause

**Usage**: `walkersim pause`

**What it does**: Pauses the simulation completely.

**Details**: This stops the simulation from running, which also halts all spawning and despawning of zombies. Existing spawned zombies will remain in the game world, but no new zombies will spawn and virtual zombies will stop moving in the simulation.

---

### resume

**Usage**: `walkersim resume`

**What it does**: Resumes the simulation after it has been paused.

**Details**: This restarts the simulation, allowing virtual zombies to continue moving and spawning/despawning to function normally again.

---

### restart

**Usage**: `walkersim restart`

**What it does**: Reloads the configuration file and restarts the simulation from scratch.

**Details**: Use this command after making changes to the `WalkerSim.xml` configuration file. The simulation will reload all settings and reinitialize with the new configuration. This is useful for testing different settings without restarting the entire server or game.

---

### stats

**Usage**: `walkersim stats`

**What it does**: Displays detailed statistics about the current simulation.

**Output includes**:

- **World Size**: The dimensions of the simulated world
- **Ticks**: How many simulation updates have occurred
- **Active**: Whether the simulation is running
- **Paused**: Whether the simulation is paused
- **Players**: Number of players in the game
- **Total Agents**: Total number of virtual zombies in the simulation
- **Total Groups**: Number of zombie groups
- **Successful Spawns**: How many zombies have successfully spawned
- **Failed Spawns**: How many spawn attempts failed (e.g., due to obstructions)
- **Total Despawns**: How many zombies have been despawned
- **Active Agents**: Current number of spawned zombies in the game world
- **Bloodmoon**: Whether a Blood Moon is currently active
- **DayTime**: Whether it's currently day or night
- **Time Scale**: The speed multiplier of the simulation
- **Average Tick Time**: Performance metric showing how long each simulation update takes
- **Wind Direction**: Current wind direction affecting zombie movement
- **Wind Target**: The target direction wind is changing toward
- **Next Wind Change**: When the wind direction will change next

**Details**: This command is useful for debugging, monitoring performance, and understanding the current state of the simulation.

---

### timescale

**Usage**: `walkersim timescale <value>`

**What it does**: Changes the speed of the simulation.

**Arguments**:

- `value` - A floating-point number representing the speed multiplier

**Examples**:
```
walkersim timescale 1.0    # Normal speed
walkersim timescale 2.0    # Double speed
walkersim timescale 0.5    # Half speed
walkersim timescale 10.0   # 10x speed
```

**Details**: Values greater than 1.0 speed up the simulation, while values less than 1.0 slow it down. This can be useful for testing how zombies behave over longer periods, or for slowing down the simulation to observe specific behaviors in detail.

---

### config

**Usage**: `walkersim config`

**What it does**: Displays the currently loaded WalkerSim configuration.

**Output includes**:

- **Random Seed**: The seed used for random number generation
- **Population Density**: Number of virtual zombies per square kilometer
- **Group Size**: Number of agents per group
- **Fast Forward At Start**: Whether the simulation fast-forwards on startup
- **Start Agents Grouped**: Whether agents start in groups or scattered
- **Start Position**: Where agents initially spawn (RandomCity, RandomBorderLocation, etc.)
- **Respawn Position**: Where agents respawn after death
- **Pause During Bloodmoon**: Whether simulation pauses during Blood Moon events
- **Sound Distance Scale**: Multiplier for sound event distances
- **Processor Groups**: Complete list of all movement processor groups including:
  - Group number (or "Any" for generic groups)
  - Speed scale
  - Post-spawn behavior settings
  - All processors with their type, distance, and power values

**Details**: This command is useful for debugging configuration issues, verifying that your XML settings loaded correctly, and understanding the current simulation parameters. It's especially helpful when sharing configurations with others or troubleshooting behavior.

---

### maskinfo

**Usage**: `walkersim maskinfo`

**What it does**: Shows information about the spawn group at your current location.

**Output includes**:

- Your position coordinates (both game world and simulation coordinates)
- The spawn group assigned to your location
- Day and night entity groups for that spawn area
- Color coding used in the map overlay

**Details**: This command helps you understand what types of zombies can spawn in your current location. Different biomes and areas have different spawn groups, which determine what zombie types appear during day and night.

---

## Tips

- Use `walkersim stats` regularly to monitor the health and performance of your simulation
- Use `walkersim config` to verify your configuration loaded correctly after making changes
- If you make changes to your XML configuration, use `walkersim restart` to apply them without restarting the server
- The `show` command is great for quickly checking zombie distribution without permanently enabling the overlay
- Use `timescale` to speed up testing when experimenting with different configuration settings
