# Setting Up the Mod

The mod comes with a default setup file called `WalkerSim.xml` that works well for most people. You can use it as-is or change it to your liking.

## Where Settings Are Stored

The mod looks for settings in this order:

1. **In your world folder**
    - The mod first looks inside your world's folder for a `WalkerSim.xml` file
    - Location examples:
        - `<7 Days to Die>\Data\Worlds\<World>\WalkerSim.xml`
        - `%APPDATA%\GeneratedWorlds\<Your World>\WalkerSim.xml`
    - This lets you have different settings for different worlds

2. **In the mod folder**
    - If it doesn't find a world-specific file, it uses the default:
     ```
     Mods/WalkerSim2/WalkerSim.xml
     ```

This means you can have one set of settings for all worlds, or customize settings for each world separately.

## Single Player Games

When you play offline by yourself, your game acts as both the player and the server. This mod runs on the server side, so it works the same whether you're playing alone or with friends.

## How to Change Settings

You have two options:

### Use the Editor Tool
- A program with buttons and sliders that makes changing settings easy
- You can see a preview of how zombies will move
- Good for people who don't like editing files

### Edit the XML File Directly
- Open `WalkerSim.xml` in Notepad or any text editor
- Change the settings manually
- Good for people comfortable with files and code

---

**Next Steps:**

- Learn about [Base Settings](configuration/base.md) - the main options
- Learn about [Movement Systems](configuration/systems.md) - how to organize zombies
- Learn about [Processors](configuration/processors.md) - how zombies move
- Learn about [The Editor Tool](editor.md) - the visual setup program
