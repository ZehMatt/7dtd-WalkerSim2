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

**Feature**: Map overlay (debug visualization).

**What it does**: Opens the map window and temporarily enables the WalkerSim overlay showing virtual zombie positions.

**Details**: The overlay only remains visible while the map is open. If you close and reopen the map, the overlay will be gone. To keep the overlay enabled permanently, use `walkersim map enable`.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode. The map overlay feature is not available on multiplayer servers.

---

### map

**Usage**: `walkersim map <enable|disable>`

**Feature**: Map overlay (debug visualization).

**What it does**: Enables or disables the WalkerSim simulation overlay in the in-game map window.

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

**Details**: When enabled, the map overlay shows the positions and movements of virtual zombies in the simulation. This is useful for understanding how zombies are distributed across the map. See also `walkersim biomes` and `walkersim roadgraph` for additional overlay layers.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode. The map overlay feature is not available on multiplayer servers.

---

### biomes

**Usage**: `walkersim biomes <enable|disable>`

**Feature**: Map overlay (debug visualization).

**What it does**: Toggles drawing the world's biome layer on top of the WalkerSim map overlay.

**Arguments**:

- `enable` (or `true` or `1`) - Draws biomes on the overlay
- `disable` (or `false` or `0`) - Hides the biome layer

**Details**: This is a sub-toggle of the map overlay and only takes visible effect while the overlay itself is enabled (see `walkersim map`/`walkersim show`). It is useful for correlating zombie distribution with biome boundaries.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode.

---

### roadgraph

**Usage**: `walkersim roadgraph <enable|disable>`

**Feature**: Map overlay (debug visualization).

**What it does**: Toggles drawing the road graph (the navigation graph used by road-following processors) on the map overlay.

**Arguments**:

- `enable` (or `true` or `1`) - Draws the road graph
- `disable` (or `false` or `0`) - Hides the road graph

**Details**: This is a sub-toggle of the map overlay and only takes visible effect while the overlay itself is enabled (see `walkersim map`/`walkersim show`). Useful for verifying the road graph processors such as `StickToRoads` and `CityVisitor` are seeing.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode.

---

### cities

**Usage**: `walkersim cities <enable|disable>`

**Feature**: Map overlay (debug visualization).

**What it does**: Toggles drawing the detected city regions on the map overlay. Each city is shown in a distinct hash-based color so adjacent or overlapping clusters can be told apart.

**Arguments**:

- `enable` (or `true` or `1`) - Draws city regions
- `disable` (or `false` or `0`) - Hides city regions

**Details**: This is a sub-toggle of the map overlay and only takes visible effect while the overlay itself is enabled (see `walkersim map`/`walkersim show`). Useful for verifying which areas the city-detection heuristic identified as cities and what the `PreferCities`, `AvoidCities`, and `CityVisitor` processors actually see. Cities draw underneath the road graph and agent markers.

!!! note "Singleplayer Only"
    This command only works when playing offline or in singleplayer mode.

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

**Feature**: Spawn group masks (advanced — see [Spawn Group Masks](configuring/spawn-groups.md)).

**What it does**: Resolves and prints the spawn group at the player's current world position by sampling the configured spawn group mask image (`ws_spawngroupsmask.png`).

**Output includes**:

- The player's position in both game-world and simulation coordinates
- The matched spawn group's day and night entity group names
- The hex color used by that spawn group (the color sampled from the mask image)

**Details**: This command is **only meaningful if you have set up the spawn group mask feature** by placing `ws_spawngroups.xml` and `ws_spawngroupsmask.png` in your world folder. Without those files, no custom spawn groups are defined and the command will report that none was found at your position. It is the primary debugging tool for verifying that the colors in your mask image align with the regions you intended.

---

## Tips

- Use `walkersim stats` regularly to monitor the health and performance of your simulation
- Use `walkersim config` to verify your configuration loaded correctly after making changes
- If you make changes to your XML configuration, use `walkersim restart` to apply them without restarting the server
- The `show` command is great for quickly checking zombie distribution without permanently enabling the overlay
- Use `timescale` to speed up testing when experimenting with different configuration settings
