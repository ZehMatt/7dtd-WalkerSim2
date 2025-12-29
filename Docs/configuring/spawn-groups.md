# Spawn Group Masks (Advanced)

!!! warning "Advanced Feature for Modders"
    This is an advanced feature primarily intended for other mods and custom content creators who want to control exactly what types of zombies spawn in specific areas of their custom maps. Most users will not need this - the game's default spawn system based on biomes will be used automatically if no spawn group mask is configured.

## What Are Spawn Group Masks?

Spawn group masks allow you to define exactly which types of zombies (entity groups) spawn in specific areas of your map. Instead of using the game's default biome-based spawning, you can create a custom bitmap image that maps different colors to different spawn groups across your entire world.

This feature is useful for:

- **Modders creating custom worlds** with specific zombie distributions
- **Adventure map creators** who want controlled enemy placement
- **Overhaul mods** that introduce custom zombie types and want them in specific regions
- Testing specific entity groups in designated areas
- Creating thematic regions with unique zombie distributions

## How It Works

The system uses two files placed in your world folder:

1. **ws_spawngroups.xml** - Defines spawn groups with their colors and entity groups
2. **ws_spawngroupsmask.png** - An image where each pixel color corresponds to a spawn group

When a virtual zombie needs to spawn, WalkerSim:
1. Checks the pixel color at that location in the mask image
2. Looks up which spawn group matches that color
3. Selects an appropriate entity group (day or night)
4. Spawns a zombie from that entity group

## File Locations

Both files must be placed in your world folder:

- `<7 Days to Die>\Data\Worlds\<YourWorld>\ws_spawngroups.xml`
- `<7 Days to Die>\Data\Worlds\<YourWorld>\ws_spawngroupsmask.png`

Or for generated worlds:

- `%APPDATA%\7DaysToDie\GeneratedWorlds\<YourWorld>\ws_spawngroups.xml`
- `%APPDATA%\7DaysToDie\GeneratedWorlds\<YourWorld>\ws_spawngroupsmask.png`

## Creating the XML File (ws_spawngroups.xml)

The XML file defines your spawn groups. Each spawn group has:

- **Color**: A hex color code that matches pixels in the mask image
- **EntityGroupDay**: The entity group name to use during daytime
- **EntityGroupNight**: The entity group name to use during nighttime

### Example XML:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SpawnGroups>
    <SpawnGroup Color="#FF0000" EntityGroupDay="ZombiesWasteland" EntityGroupNight="ZombiesWastelandNight" />
    <SpawnGroup Color="#00FF00" EntityGroupDay="ZombiesForest" EntityGroupNight="ZombiesForestNight" />
    <SpawnGroup Color="#0000FF" EntityGroupDay="ZombiesBurntForest" EntityGroupNight="ZombiesBurntForestNight" />
    <SpawnGroup Color="#FFFF00" EntityGroupDay="ZombiesDesert" EntityGroupNight="ZombiesDesertNight" />
    <SpawnGroup Color="#FF00FF" EntityGroupDay="ZombiesSnow" EntityGroupNight="ZombiesSnowNight" />
</SpawnGroups>
```

### Available Entity Groups

You can use any entity group defined in the game's XML files. Common ones include:

**Biome Groups:**

- `ZombiesWasteland`, `ZombiesWastelandNight`
- `ZombiesForest`, `ZombiesForestNight`
- `ZombiesBurntForest`, `ZombiesBurntForestNight`
- `ZombiesDesert`, `ZombiesDesertNight`
- `ZombiesSnow`, `ZombiesSnowNight`

**Special Groups:**

- `FeralHorde` - Feral zombies
- `ZombiesCave` - Cave-dwelling zombies
- Custom groups from mods

You can find more entity groups in the game's `entitygroups.xml` file.

## Creating the Mask Image (ws_spawngroupsmask.png)

The mask image is a PNG file where each pixel's color determines which spawn group applies to that location.

### Image Specifications:

- **Format**: PNG (supports transparency)
- **Size**: Any size - the image will be automatically scaled to match your world size
- **Colors**: Must exactly match the hex colors defined in your XML file
- **Transparency**: Pixels with 0% opacity (fully transparent) mean no custom spawning - the game's default biome system will be used

### Creating the Mask:

1. **Get your world dimensions** - Use `walkersim stats` to see "World Size"
2. **Create an image** - Use any image editor (Photoshop, GIMP, Paint.NET, etc.)
   - The image can be any size, but using your actual world dimensions gives more precision
   - For large worlds, you can use a smaller image for easier editing
3. **Paint regions** - Use the exact hex colors from your XML to paint different areas
   - Use solid colors with no anti-aliasing or gradients
   - Each distinct color will map to a different spawn group
4. **Leave areas transparent** - Areas you want to use default biome spawning should be fully transparent
5. **Save as PNG** - Make sure to preserve the exact colors and transparency

### Tips:

- **Use a grid or overlay** - Overlay your world map to see where to place different spawn zones
- **Test with `maskinfo`** - Use the `walkersim maskinfo` console command to check what spawn group is at your current location
- **Start simple** - Begin with a few large zones before creating detailed regions
- **Use the map overlay** - The colors from your spawn groups will appear in the map overlay (singleplayer only)

## Example Workflow

1. **Define your spawn groups** in `ws_spawngroups.xml`:
   ```xml
   <SpawnGroups>
       <SpawnGroup Color="#FF0000" EntityGroupDay="ZombiesWasteland" EntityGroupNight="ZombiesWastelandNight" />
       <SpawnGroup Color="#00FF00" EntityGroupDay="ZombiesForest" EntityGroupNight="ZombiesForestNight" />
   </SpawnGroups>
   ```

2. **Create your mask image** with:

   - Red (#FF0000) areas for wasteland zombies
   - Green (#00FF00) areas for forest zombies
   - Transparent areas for default biome spawning

3. **Place both files** in your world folder

4. **Start or restart your game/server**

   - For existing games, use `walkersim restart` to reload the configuration

5. **Test your setup**:

   - Use `walkersim show` to see the spawn zones on your map (singleplayer only)
   - Use `walkersim maskinfo` to check the spawn group at your location
   - Observe which zombies spawn in different colored zones

## Troubleshooting

**Zombies aren't spawning in my custom zones:**

- Check that your image colors exactly match the XML hex codes
- Verify both files are in the correct world folder
- Use `walkersim maskinfo` to confirm the spawn group is being detected
- Make sure the entity group names exist in the game

**The mask image looks wrong on the map overlay:**

- The image is automatically scaled to your world size
- Colors will match what you defined in the XML file
- Transparent areas won't show any color overlay

**Colors are slightly different than expected:**

- Make sure your image editor isn't applying color profiles or adjustments
- Save as PNG-24 or PNG-32 to preserve exact colors
- Avoid anti-aliasing or gradients - use solid colors only

**Getting "No spawn group found" messages:**

- Some locations may have transparency in your mask
- Transparent areas fall back to the game's default biome spawning system
- This is normal and expected behavior

## Removing Spawn Group Masks

To go back to using the game's default biome-based spawning:

1. Delete or rename `ws_spawngroups.xml` and `ws_spawngroupsmask.png` from your world folder
2. Use `walkersim restart` to reload without the custom spawn groups

---

**Related Commands:**

- [`walkersim maskinfo`](../commands.md#maskinfo) - Check spawn group at your location
- [`walkersim show`](../commands.md#show) - View spawn zones on map (singleplayer only)
- [`walkersim restart`](../commands.md#restart) - Reload configuration after making changes
