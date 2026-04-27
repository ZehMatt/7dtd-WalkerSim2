# Movement Systems

This controls how zombies move around on the virtual map. You can make different groups of zombies behave in different ways.

## How Groups Work

The mod automatically creates groups of zombies based on your settings:

**Number of groups** = (Total zombies) ÷ (GroupSize)

For example:

- If you have 100 zombies total
- And GroupSize is 20
- You get 5 groups (numbered 0, 1, 2, 3, 4)

## How Systems Get Assigned to Groups

Each movement system has a **Weight** value that determines how many groups it controls. Groups are distributed proportionally based on weight.

For example, with 5 groups and two systems:

- System A with `Weight="2"` gets ~67% of groups → groups 0, 1, 2
- System B with `Weight="1"` gets ~33% of groups → groups 3, 4

If you only have one system, it controls all groups regardless of its weight value.

Every system is guaranteed at least one group.

## System Settings

Each system can have these settings:

### Weight

How many groups this system controls, relative to other systems.

- `Weight="1"` is the default
- Higher values mean more groups are assigned to this system
- The actual number of groups depends on the total group count and other systems' weights

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
