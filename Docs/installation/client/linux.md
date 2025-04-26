!!! warning
    EAC has to be turned off.

## Prerequisites

- **7 Days to Die** installed via **Steam** (latest supported version).
- **WalkerSim2** mod files (e.g., `WalkerSim2-0.9.3.zip`).
- **Required dependencies**
    - `libgdiplus`
    - `libc6-dev`

---

## Step 1: Locate Your 7 Days to Die Game Folder

1. Open **Steam**.
2. Go to your **Library** and find **7 Days to Die**.
3. Right-click the game and select **Manage > Browse local files**.
4. This will open the main 7 Days to Die installation directory.

Example path:

> ```~/.steam/steam/steamapps/common/7 Days To Die```

!!! note
    Folder paths may vary depending on your Linux distribution and installation method.

---

## Step 2: Download and Extract WalkerSim2

1. Download the latest WalkerSim2 release, e.g., `WalkerSim2-0.9.3.zip`.
2. Use an extraction tool like **Ark**, **File Roller**, or a terminal command (like `unzip`) to unpack the archive.
3. You should get a folder named:
   
    > ```WalkerSim2```

---

## Step 3: Install WalkerSim2

1. Navigate to the `Mods` folder inside your 7 Days to Die installation directory.
2. Move the **WalkerSim2** folder you extracted into the `Mods` folder.

Your folder structure should look like:

> ```7 Days To Die/Mods/WalkerSim2/```

Inside the `WalkerSim2` folder, you should see files like `ModInfo.xml` and other related files and folders.

---

## Step 4: Launch the Game

1. Start **7 Days to Die** through **Steam**.
2. WalkerSim2 should automatically load if installed correctly.
3. Start a new game or continue an existing one.

---

## Troubleshooting

- **Game won't launch or errors appear?**
    - Ensure that the mod is placed directly inside the `Mods` folder (no nested folders like `Mods/WalkerSim2/WalkerSim2/ModInfo.xml`).
    - Double-check that the WalkerSim2 version matches your installed game version.

- **Still having problems?**
    - Refer to the official WalkerSim2 documentation or community support channels.

---

## Important: Check for 0_TFP_Harmony

WalkerSim2 depends on a core mod called `0_TFP_Harmony`, which should already be present in your `Mods` folder.

- **If `0_TFP_Harmony` is missing:**
  1. Open **Steam**.
  2. Right-click on **7 Days to Die** and select **Properties**.
  3. Go to the **Installed Files** tab.
  4. Click **Verify integrity of game files**.

Steam will scan your installation and automatically restore any missing files, including `0_TFP_Harmony`.

---

## Additional: Dependencies for Linux Users

Both **7 Days to Die** game installations and **dedicated servers** on Linux require the following libraries to be installed:

- `libgdiplus`
- `libc6-dev`

### For Debian/Ubuntu-based systems:

Run the following command to install the required libraries:

```bash
sudo apt-get update && sudo apt-get install -y libgdiplus libc6-dev
```

### For Red Hat/Fedora-based systems:

Use `dnf` to install the dependencies:

```bash
sudo dnf install -y libgdiplus libc6-dev
```

### For Arch-based systems:

Use `pacman` to install the dependencies:

```bash
sudo pacman -S libgdiplus libc6-dev
```

These dependencies are required for **both game installations and dedicated servers** on Linux. They ensure that **WalkerSim2** works properly regardless of whether you're playing the game locally or hosting a server.

