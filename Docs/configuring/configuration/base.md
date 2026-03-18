These are the main settings that control how your zombie simulation works. You can edit these in the XML file or using the Editor tool.

## Settings List

Here are all the settings you can change:

### Logging

**What it does**: Makes the mod write information to log files. This helps you see what the mod is doing.

**Options**:

- **General**: Writes basic information about the simulation
- **Spawns**: Writes when zombies appear
- **Despawns**: Writes when zombies disappear
- **EntityClassSelection**: Writes which zombie types are chosen
- **Events**: Writes when loud noises or other events happen

**Default**: All logging is turned off

**When to use it**: Turn on logging when you want to understand what the mod is doing or fix problems.

### RandomSeed

**What it does**: Controls the randomness of the simulation. Using the same number makes zombies behave the same way every time.

**Value**: Any number

**When to use it**: Use the same number to test things repeatedly. Change the number to get different zombie behavior.

### PopulationDensity

**What it does**: Controls how many zombies exist on your map.

**Value**: A number between 1 and 4000. This is how many zombies per square kilometer.

**Example**: Setting this to 200 creates a medium amount of zombies.

### SpawnActivationRadius

**What it does**: How close a virtual zombie must be to you before it becomes real.

**Value**: A number between 48 and 196 meters. Default is 96.

**Important**: Don't set this higher than how far you can see in the game. Setting it too high can cause problems. 

### StartAgentsGrouped

**What it does**: Makes zombies start together in groups, or spread out individually.

**Value**: True (start in groups) or False (start spread out)

**Example**: Set to True to make zombies start in clumps like a horde.

### EnhancedSoundAwareness

**What it does**: Makes zombies hear loud sounds from farther away.

**Value**: True (zombies hear better) or False (normal hearing)

**Example**: When True, zombies will walk toward gunshots and explosions from much farther away than normal.

### SoundDistanceScale

**What it does**: Makes sounds travel farther or shorter to attract zombies.

**Value**: A number between 0.1 and 10.0
- 1.0 = normal sound distance
- 2.0 = sounds travel twice as far
- 0.5 = sounds only travel half as far

**Default**: 1.0

**Example**: Set to 1.5 to make zombies react to sounds from farther away.

### GroupSize

**What it does**: How many zombies are in each group.

**Value**: Any number 1 or higher

**Example**: Setting this to 20 means each zombie group has 20 zombies.

**Note**: The total number of groups is: (Total zombies) divided by (GroupSize)

### AgentStartPosition

**What it does**: Where zombies start when the simulation begins.

**Options**:

- **RandomBorderLocation**: Zombies start around the edges of the map
- **RandomLocation**: Zombies start anywhere on the map
- **RandomPOI**: Zombies start at buildings and landmarks
- **RandomCity**: Zombies start in cities
- **Mixed**: A mix of all the above

**Example**: Use RandomPOI to make zombies start at towns and buildings.

### AgentRespawnPosition

**What it does**: Where zombies come back after they die.

**Options**:

- **None**: Dead zombies don't come back
- **RandomBorderLocation**: Zombies come back at the edges of the map
- **RandomLocation**: Zombies come back anywhere on the map
- **RandomPOI**: Zombies come back at buildings
- **RandomCity**: Zombies come back in cities
- **Mixed**: A mix of all the above

**Example**: Use RandomBorderLocation to make zombies return from the edges of the map.

### PauseDuringBloodmoon

**What it does**: Stops the simulation during Blood Moon nights.

**Value**: True (stop during Blood Moon) or False (keep running)

**Example**: Set to True so the normal Blood Moon zombies work without interference from this mod.

!!! note
    Even with this set to True, any zombies already spawned by WalkerSim when Blood Moon starts will remain in the game world. However, they won't behave like Blood Moon horde zombies - they won't automatically seek out players. Only the zombies spawned by the Blood Moon event itself will hunt you down. WalkerSim zombies will continue following their normal behavior.

### SpawnProtectionTime

**What it does**: Gives you time to get ready before zombies start spawning when you first start a new game.

**Value**: Number of seconds to wait before spawning zombies. Default is 300 (5 minutes).

**How it works**: When you start a brand new game for the first time, the mod waits this many seconds before spawning any zombies near you. This only applies when spawning for the very first time after starting a new game, not after dying or respawning.

**Example**: Set to 600 (10 minutes) to give yourself more time to gather supplies. Set to 0 to start with zombies immediately.

### InfiniteZombieLifetime

**What it does**: Prevents zombies spawned by WalkerSim from dying due to the game's built-in lifetime expiration. The base game assigns a limited lifetime to spawned zombies, causing them to suddenly die after some time. Enabling this setting overrides that behavior. This does not make zombies invincible to damage — they can still be killed normally by players.

**Value**: True (prevent expiration) or False (use game default lifetime)

**Default**: False

**When to use it**: Enable this if you notice zombies suddenly dying for no apparent reason. This is caused by the game's internal lifetime system and is unrelated to player damage.

### MaxSpawnedZombies

**What it does**: Limits how many zombies WalkerSim can spawn in the game world at once.

**Value**: Can be a percentage (like "75%") or an absolute number (like "48").

**How it works**: 
- If you use a percentage, it's based on the game's MaxSpawnedZombies setting from your server settings
- For example, if the game allows 64 zombies and you set "50%", WalkerSim will only spawn up to 32 zombies
- You can use percentages above 100% (like "150%") but this may cause performance issues
- If you use an absolute number, that's the maximum zombies WalkerSim will spawn, capped between 1 and 200
- This limit ensures WalkerSim doesn't use up all the zombie spawn slots, leaving room for other zombie spawns

**Default**: "75%" (75% of game's MaxSpawnedZombies setting)

**Note**: The Editor tool only supports setting this as a percentage. To use an absolute number value, you must edit the XML file directly.

**Example**: Set to "50%" for fewer zombies, "100%" to use the full game limit, or "32" to always spawn a maximum of 32 zombies regardless of game settings.

### PopulationStartPercent

**What it does**: Controls what percentage of the total zombie population is active when the game starts. The remaining zombies are inactive and will gradually wake up over time.

**Value**: A number between 0 and 100. Default is 100 (all zombies active immediately).

**How it works**: On day 1, only this percentage of the total population will be wandering the map. The rest are dormant and will gradually become active as game days pass, reaching full population by the day set in `FullPopulationAtDay`. When zombies wake up, they spawn near the current position of their group (if `StartAgentsGrouped` is enabled) rather than at their original starting location.

**Example**: Set to 10 to start with only 10% of the zombie population on day 1, creating a slower early game that ramps up over time.

### FullPopulationAtDay

**What it does**: The game day by which the zombie population reaches 100%. Population scales linearly from `PopulationStartPercent` on day 1 to full population on this day.

**Value**: Any number 1 or higher. Default is 1 (full population from the start).

**How it works**: The population grows linearly between day 1 and this day. For example, if `PopulationStartPercent` is 10 and `FullPopulationAtDay` is 8, you'll have 10% on day 1, about 25% on day 2, and so on until 100% on day 8. After this day, the population stays at 100%.

**Example**: Set to 14 to have the population gradually fill up over the first two weeks of gameplay. Pair with a low `PopulationStartPercent` for a survival experience that starts quiet and gets increasingly dangerous.

---
