# Movement Systems

This controls how zombies move around on the virtual map. You can make different groups of zombies behave in different ways.

## How Groups Work

The total population is split among the systems, and each system divides its share into groups of its own size.

1. The total number of zombies comes from the map size and `PopulationDensity`.
2. Each system gets a share of that population proportional to its **Weight**.
3. Each system splits its share into groups of its own **GroupSize**.

For example, with 200 total zombies and two systems:

- System A with `Weight="3"` and `GroupSize="20"` gets ~150 zombies → ~8 groups of 20
- System B with `Weight="1"` and `GroupSize="50"` gets ~50 zombies → 1 group of 50

## System Settings

Each system can have these settings:

### Weight

What share of the total population this system controls, relative to other systems.

- `Weight="1"` is the default
- Higher values mean more zombies are assigned to this system
- If you only have one system, it gets the entire population regardless of its weight

### GroupSize

How many zombies are in each of this system's groups.

- `GroupSize="100"` is the default
- The system's share of the population is divided into groups of this size (the last group may be smaller)
- Smaller groups mean more, more independent clusters; larger groups mean fewer, larger clusters

### StartPosition

Where this system's agents spawn when the simulation begins.

- **Global**: Use the base configuration's `AgentStartPosition` (default)
- **RandomBorderLocation**: Around the edges of the map
- **RandomLocation**: Anywhere on the map
- **RandomPOI**: At buildings and landmarks
- **RandomCity**: In cities
- **Mixed**: A mix of the above

### StartBiome

Restricts the start location to a biome. Combined with `StartPosition`, so for example `RandomCity` + `Snow` places agents in cities that lie in the snow biome.

- **Any**: No biome preference, the whole map is used (default)
- **Snow / PineForest / Desert / Wasteland / BurntForest**: Only positions within that biome (falls back to the chosen location anywhere if the biome isn't present or is too sparse on the map)

### RespawnPosition

Where this system's agents come back after they die.

- **Global**: Use the base configuration's `AgentRespawnPosition` (default)
- **None**: Dead agents don't come back
- **RandomBorderLocation**: Come back at the edges of the map
- **RandomLocation**: Come back anywhere on the map
- **RandomPOI**: Come back at buildings
- **RandomCity**: Come back in cities
- **Mixed**: A mix of the above

### RespawnBiome

Restricts the respawn location to a biome, the same way `StartBiome` constrains `StartPosition`.

- **Any**: No biome preference, the whole map is used (default)
- **Snow / PineForest / Desert / Wasteland / BurntForest**: Only positions within that biome (falls back to the chosen location anywhere if the biome isn't present or is too sparse on the map)

### SpeedScale

How fast zombies move on the virtual map (not in the game).

- 1.0 = normal speed
- 2.0 = twice as fast
- 0.5 = half speed

This only affects the simulation, not how fast zombies move when they spawn in your game.

### PostSpawnBehavior

What zombies do after they appear in your game:

- **Wander**: Zombies just walk around randomly
- **ChaseActivator**: Zombies chase whatever made them spawn (usually you)
- **Nothing**: No post-spawn order is given, zombies stand idle until the game's normal AI reacts to something

### PostSpawnWanderSpeed

How fast zombies walk when they're just wandering (not chasing you):

- **NoOverride**: Use the game's normal setting
- **Walk**: Slow walking
- **Jog**: Faster walking
- **Run**: Running
- **Sprint**: Fast running
- **Nightmare**: Very fast

This only matters when zombies are wandering. When they see you or attack, they use the game's normal speed.

### MapEdgeBehavior

What happens when an agent of this system reaches the edge of the simulated map:

- **Warp**: The agent wraps around to the opposite side of the map (default, original behavior)
- **Bounce**: The agent's velocity is nudged back toward the center and its position is clamped to the map bounds. Useful for keeping agents inside the playable area without them disappearing
- **Clamp**: The agent's position is simply clamped to the map bounds without changing velocity. The agent will pile up against the edge until other forces push it away

This is set per-system, so different groups can use different edge behaviors.

### Color

What color to show this group in the Editor tool. This is just for viewing and doesn't affect the game.

Use colors like: #FF0000 (red), #00FF00 (green), #0000FF (blue)

## Processors

You can add different processors to each system. Each processor has two settings:

- **Power**: How strong the behavior is (higher = stronger)
- **Distance**: How far away the behavior works (only some behaviors use this)

See the Processors page for all the different types you can use.

## Simple Example

Here's a basic setup with one system that controls all groups:

```xml
<Systems>
  <System Weight="1" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="WorldEvents" Power="0.8" />
  </System>
</Systems>
```

This makes all zombie groups stick together and react to loud noises.

## Multiple Systems with Weights

You can define multiple systems with different weights to create diverse behavior:

```xml
<Systems>
  <System Weight="2" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
  </System>
  <System Weight="1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </System>
</Systems>
```

With 6 groups total:

- Groups 0–3: Use the first system (weight 2 → gets twice as many groups). Zombies stick together and wander randomly (red in Editor).
- Groups 4–5: Use the second system (weight 1). Zombies move faster, chase loud noises, and chase you when they spawn (green in Editor).

## Equal Weights

If all systems have the same weight, groups are split evenly:

```xml
<Systems>
  <System Weight="1" SpeedScale="0.8" PostSpawnBehavior="Wander" Color="#44AA44">
    <Processor Type="Wind" Power="0.7" />
  </System>
  <System Weight="1" SpeedScale="0.8" PostSpawnBehavior="Wander" Color="#9E4244">
    <Processor Type="WindInverted" Power="0.7" />
  </System>
  <System Weight="1" SpeedScale="0.8" PostSpawnBehavior="Wander" Color="#57A8BF">
    <Processor Type="StickToRoads" Power="1.0" />
  </System>
</Systems>
```

With 9 groups, each system gets 3 groups.
