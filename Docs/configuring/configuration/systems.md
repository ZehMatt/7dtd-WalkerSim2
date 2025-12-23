# Movement Systems

This controls how zombies move around on the virtual map. You can make different groups of zombies behave in different ways.

## How Groups Work

The mod automatically creates groups of zombies based on your settings:

**Number of groups** = (Total zombies) ÷ (GroupSize)

For example:

- If you have 100 zombies total
- And GroupSize is 20
- You get 5 groups (numbered 0, 1, 2, 3, 4)

## Group Settings

Each group can have these settings:

### Group Number

Which group this applies to.

- Use a specific number (0, 1, 2, etc.) to set up one group
- Use -1 to apply to all groups without specific settings

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

### Color

What color to show this group in the Editor tool. This is just for viewing and doesn't affect the game.

Use colors like: #FF0000 (red), #00FF00 (green), #0000FF (blue)

## Processors

You can add different processors to each group. Each processor has two settings:

- **Power**: How strong the behavior is (higher = stronger)
- **Distance**: How far away the behavior works (only some behaviors use this)

See the Processors page for all the different types you can use.

## Simple Example

Here's a basic setup with 2 different groups:

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
  </ProcessorGroup>
  <ProcessorGroup Group="1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

This creates:

- Group 0: Zombies stick together and wander randomly (red in Editor)
- Group 1: Zombies move faster, chase loud noises, and chase you when they spawn (green in Editor)

## Using -1 for All Groups

You can use Group="-1" to apply the same behavior to all groups:

```xml
<MovementProcessors>
  <ProcessorGroup Group="-1" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

This makes all zombie groups react to loud noises and wander after spawning.

## Mixing Specific and All Groups

You can set up some specific groups and let the rest use -1:

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
  </ProcessorGroup>
  <ProcessorGroup Group="-1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

With 3 groups total:

- Group 0: Uses the specific settings (stick together, wander)
- Group 1 and 2: Use the -1 settings (react to noise, chase you)
