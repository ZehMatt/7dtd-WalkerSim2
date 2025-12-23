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
- **Mixed**: A mix of all the above

**Example**: Use RandomPOI to make zombies start at towns and buildings.

### AgentRespawnPosition

**What it does**: Where zombies come back after they die.

**Options**:

- **None**: Dead zombies don't come back
- **RandomBorderLocation**: Zombies come back at the edges of the map
- **RandomLocation**: Zombies come back anywhere on the map
- **RandomPOI**: Zombies come back at buildings
- **Mixed**: A mix of all the above

**Example**: Use RandomBorderLocation to make zombies return from the edges of the map.

### PauseDuringBloodmoon

**What it does**: Stops the simulation during Blood Moon nights.

**Value**: True (stop during Blood Moon) or False (keep running)

**Example**: Set to True so the normal Blood Moon zombies work without interference from this mod.

---

## Example Settings

Here's a basic example you can copy and paste:

```xml
<WalkerSim>
  <Logging>
    <General>true</General>
    <Spawns>false</Spawns>
    <Despawns>false</Despawns>
    <EntityClassSelection>false</EntityClassSelection>
    <Events>false</Events>
  </Logging>
  <RandomSeed>12345</RandomSeed>
  <PopulationDensity>200</PopulationDensity>
  <SpawnActivationRadius>96</SpawnActivationRadius>
  <StartAgentsGrouped>true</StartAgentsGrouped>
  <EnhancedSoundAwareness>true</EnhancedSoundAwareness>
  <SoundDistanceScale>1.0</SoundDistanceScale>
  <GroupSize>20</GroupSize>
  <AgentStartPosition>RandomPOI</AgentStartPosition>
  <AgentRespawnPosition>RandomBorderLocation</AgentRespawnPosition>
  <PauseDuringBloodmoon>true</PauseDuringBloodmoon>
</WalkerSim>
```
