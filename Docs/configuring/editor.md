# Editor Tool

The Editor is a program that lets you test and set up the mod **outside of the game**. It simulates the zombie movement on your computer so you can see how your settings will work before using them in 7 Days to Die.

The Editor does NOT require the game to be running - it's a standalone preview tool.

![WalkerSim Editor](../img/editor-1.png)

---

## Working With Configuration Files

### Loading a Configuration

To load an existing configuration file:

1. Click `File` > `Load Configuration`
2. Browse to your config file (usually `WalkerSim.xml`)
3. The Editor will load all your settings

Common locations to load from:

- `Mods/WalkerSim2/WalkerSim.xml` (the default mod config)
- `<7 Days to Die>\Data\Worlds\<World>\WalkerSim.xml` (world-specific config)
- `%APPDATA%\GeneratedWorlds\<Your World>\WalkerSim.xml` (generated world config)

### Exporting Your Configuration

Once you're happy with your setup, you must export it to use in the game:

1. Click `File` > `Export Configuration`
2. Choose where to save it:
    - Save to `Mods/WalkerSim2/WalkerSim.xml` to use for all worlds
    - Save to your world folder to use for one specific world only
3. Click Save

**Important**: The Editor only previews your settings. You must export to use them in the game!

### Saving and Loading Simulation States

The simulation state (`.bin` files) contains all zombie positions, health, and current behaviors. These files are automatically created by the mod when the game is running.

**In the Game:**

- The mod automatically saves the simulation state every 60 seconds to `walkersim.bin` in your world's save folder
- This preserves all zombie data when you exit the game
- When you reload the world, the mod loads this file so zombies continue from where they were

**In the Editor:**

You can load these game-generated state files to test from a specific point:

- **Load State**: `File` > `Load State` - Loads a `walkersim.bin` file from a game save
- **Save State**: `File` > `Save State` - Saves the current editor simulation to a `.bin` file

This is useful for:

- Testing how your configuration changes affect an existing game save
- Debugging specific situations that happened in your game
- Continuing editor work from where you left off

!!! note
    State files include zombie positions and the configuration that was active when saved. The world size must match or the state cannot be loaded.

---

## Main Settings

### World

Pick a map to preview. This loads the map so you can see where zombies will go. The map choice is not saved - it's just for testing.

### Other Settings

Most settings here match the settings from the [Base Parameters](configuration/base.md) page. Click on any setting to see what it does.

## Movement Systems and Processors

This section lets you set up [Movement Systems](configuration/systems.md) and [Processors](configuration/processors.md).

You can:

- Create groups of zombies with different behaviors
- Set how fast each group moves
- Choose what zombies do when they spawn
- Add behaviors like "stick to roads" or "chase loud noises"

---

## Testing Your Setup

### Starting the Preview

1. Click `Simulation` > `Start` to begin
2. Watch zombies move around on the map
3. You can change settings while it's running (some settings need a restart)

### Speed Controls

You can make the preview run faster to test things quickly.

!!! note
    Very fast speeds might not be accurate. After speeding up, slow back down to normal speed and let it run for a bit to see the real behavior.

---

## Testing Tools

Click these tools and then click on the map to use them:

### Emit Sound

Creates a loud noise at the spot you click. This tests if zombies move toward loud sounds (needs the WorldEvents behavior).

- The circle shows how far the sound travels
- Zombies should start walking toward it

### Kill

Kills all zombies in a circle where you click. This tests if zombies come back (respawn) properly.

- The circle shows which zombies will die
- Watch to see if/where new zombies appear

---

## Summary

1. **Load** a configuration to start with (or use the default)
2. **Adjust** settings and test with the preview
3. **Export** your configuration when you're happy with it
4. Put the exported file in your mod or world folder
5. Launch 7 Days to Die to use your settings

The Editor is just for testing - always remember to export your configuration!
