## Prerequisites

- **7 Days to Die Dedicated Server** installed via **Steam** or **SteamCMD** (latest supported version).
- **WalkerSim2** mod files (e.g., WalkerSim2-0.9.3.zip).

---

## Step 1: Locate Your 7 Days to Die Dedicated Server Folder

1. If installed via **Steam**:
   - Open **Steam** on your Windows PC.
   - Navigate to your **Library**, select **Tools**, and find **7 Days to Die Dedicated Server**.
   - Right-click, select **Manage**, then click **Browse local files**.
2. If installed via **SteamCMD**:
   - Navigate to the directory where you installed the server (e.g., the directory specified during SteamCMD setup).

Example path:
> ```C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die Dedicated Server```

or

> ```C:\7dtd_server``` (for SteamCMD, depending on your setup)

---

## Step 2: Download and Extract WalkerSim2

1. Download the latest WalkerSim2 release, e.g., `WalkerSim2-0.9.3.zip`.
2. Use a tool like **7-Zip** or **WinRAR** to extract the archive.
3. Extracting should yield a folder named:
WalkerSim2

---

## Step 3: Install WalkerSim2

1. Go to the Mods folder in your dedicated server installation directory.
2. Copy the extracted **WalkerSim2** folder into the Mods folder.

The folder structure should be:
> ```C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die Dedicated Server\Mods\WalkerSim2\```

or

> ```C:\7dtd_server\Mods\WalkerSim2\``` (for SteamCMD)

Inside the `WalkerSim2` folder, confirm files like `ModInfo.xml` and other related files/folders are present.

---

## Step 4: Launch the Dedicated Server

1. Start the **7 Days to Die Dedicated Server**:
   - For **Steam**, launch via the Steam interface or a batch file.
   - For **SteamCMD**, run the server using the appropriate command (e.g., `startserver.bat` or `7DaysToDieServer.exe`).
2. If installed correctly, WalkerSim2 will load automatically.
3. Connect to the server from a client to verify functionality.

---

## Troubleshooting

- **Server fails to start or shows errors?**
    - Verify the mod is directly in the Mods folder (not nested, e.g., `Mods\WalkerSim2\WalkerSim2\ModInfo.xml` ).
    - Ensure the WalkerSim2 version is compatible with your server version.

- **Persistent issues?**
    - Consult the official WalkerSim2 documentation or community support channels.

---

## Important: Verify 0_TFP_Harmony

WalkerSim2 requires the 0_TFP_Harmony core mod, which should be in your Mods folder.

- **If 0_TFP_Harmony is missing:**
  1. For **Steam**:
     - Open **Steam**.
     - Right-click **7 Days to Die Dedicated Server**, then select **Properties**.
     - Navigate to the **Installed Files** tab.
     - Click **Verify integrity of game files**.
  2. For **SteamCMD**:
     - Run SteamCMD and use force_install_dir and app_update 294420 to update/verify the server files.

Steam or SteamCMD will restore missing files, including 0_TFP_Harmony.