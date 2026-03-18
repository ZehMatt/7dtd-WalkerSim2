# Processors

These control how zombies move around on the virtual map. Each processor has a **Power** setting (how strong it is) and some have a **Distance** setting (how far away it works).

## Behaviors That Make Zombies Group Together

### FlockAnyGroup
Zombies move toward any nearby zombies.

- Uses Distance: Yes
- Example: Makes all zombies cluster together into one big group

### FlockSameGroup
Zombies move toward other zombies in their own group only.

- Uses Distance: Yes
- Example: Keeps zombies with the same behavior together

### FlockOtherGroup
Zombies move toward zombies in different groups.

- Uses Distance: Yes
- Example: Makes different zombie groups merge together

## Behaviors That Make Zombies Move the Same Direction

### AlignAnyGroup
Zombies face the same direction as nearby zombies.

- Uses Distance: Yes
- Example: Creates a horde all walking the same way

### AlignSameGroup
Zombies face the same direction as zombies in their group.

- Uses Distance: Yes
- Example: Makes group members move together like a unit

### AlignOtherGroup
Zombies face the same direction as zombies in other groups.

- Uses Distance: Yes
- Example: Makes different groups copy each other's direction

## Behaviors That Make Zombies Spread Out

### AvoidAnyGroup
Zombies move away from any nearby zombies.

- Uses Distance: Yes
- Example: Spreads zombies out across the map

### AvoidSameGroup
Zombies move away from zombies in their own group.

- Uses Distance: Yes
- Example: Makes groups spread out without bunching up

### AvoidOtherGroup
Zombies move away from zombies in different groups.

- Uses Distance: Yes
- Example: Makes groups stay separate from each other

## Behaviors Using Wind

### Wind
Zombies move in the direction the wind is blowing.

- Uses Distance: No
- Example: Zombies drift with the wind

### WindInverted
Zombies move against the wind direction.

- Uses Distance: No
- Example: Zombies push against the wind

## Behaviors Using Roads

### StickToRoads
Zombies navigate along roads using a waypoint graph built from the map's road data. When near a road, agents follow it node-to-node. At intersections they pick a direction (usually continuing forward, sometimes turning). At dead ends they turn around and walk back to the last intersection. Agents that drift too far from their target road node will re-acquire the nearest road.

- Uses Distance: No
- Example: Zombies walk along roads and highways, properly traversing intersections and turning around at dead ends

### AvoidRoads
Zombies move away from roads.

- Uses Distance: No
- Example: Zombies stay in the wilderness away from roads

## Behaviors Using Buildings

### StickToPOIs
Zombies move toward the nearest building or landmark.

- Uses Distance: No
- Example: Zombies gather at towns, gas stations, and houses

### AvoidPOIs
Zombies move away from buildings.

- Uses Distance: Yes
- Example: Zombies avoid populated areas

## Behaviors Using Cities

### PreferCities
Zombies prefer to stay in cities. When inside a city, they wander naturally throughout the area. When outside, they move toward the nearest city.

- Uses Distance: No
- Example: Zombies congregate in towns and cities, creating realistic population centers

### AvoidCities
Zombies avoid cities and stay in the wilderness. When inside a city, they push toward the nearest exit. When outside, they move away from city boundaries.

- Uses Distance: Yes
- Example: Zombies avoid populated areas and are found in forests, mountains, and rural areas

### CityVisitor
Zombies travel to cities using a stateful behavior with three phases: selecting a target city, traveling to it, and exploring it before selecting a new destination. The stay duration is configurable via Param1 (minimum) and Param2 (maximum), specified in real-time minutes.

**How it works:**
- **Idle Phase**: Selects a random target city using weighted selection (larger cities are more likely to be chosen)
- **Approaching Phase**: Travels toward the target city until arrival
- **Arrived Phase**: Explores the city for a random duration between Param1 and Param2 real-time minutes with hash-based wandering patterns that change every 60 seconds, then returns to Idle

The selection is deterministic based on agent group and current time, causing agents to pick new destinations at different times. Unlike PreferCities (which keeps agents in one general area), CityVisitor makes agents migrate between cities, creating dynamic population shifts across the map.

- Uses Distance: No
- Example: Zombie groups travel between cities, spending 20 minutes exploring before moving to the next destination

## Behaviors Using Biomes

These processors use a signed distance field (SDF) computed from the map's biome data to attract or repel agents toward specific biome types. A **Biome** parameter selects which biome to target: Snow, Pine Forest, Desert, Wasteland, or Burnt Forest.

The force is proportional to how far the agent is from the biome boundary — stronger when far away, tapering to zero at the edge to prevent overshooting.

### StickToBiome
Zombies are attracted toward a specific biome. Agents outside the target biome are pulled toward it. Once inside, a gentle nudge near the boundary keeps them from drifting out, while deep inside no force is applied so other processors drive movement freely.

- Uses Distance: No
- Uses Biome: Yes (select target biome type)
- Example: Zombies stay in the desert, creating biome-specific population zones

### AvoidBiome
Zombies are repelled from a specific biome. Agents inside the target biome are pushed out. Once outside, no force is applied.

- Uses Distance: No
- Uses Biome: Yes (select target biome type)
- Example: Zombies avoid snowy areas and stay in warmer biomes

## Example Settings

Here's how to set up zombie behaviors in your config file:

```xml
<Systems>
  <System Weight="2" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </System>
  <System Weight="1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
    <Processor Type="StickToPOIs" Power="0.5" />
  </System>
  <System Weight="1" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FFFF00">
    <Processor Type="PreferCities" Power="0.7" />
    <Processor Type="FlockSameGroup" Distance="20.0" Power="0.4" />
  </System>
  <System Weight="1" SpeedScale="1.2" PostSpawnBehavior="Wander" Color="#00FFFF">
    <Processor Type="StickToBiome" Power="0.5" Param1="5" />
    <Processor Type="AvoidCities" Distance="100.0" Power="0.8" />
  </System>
  <System Weight="1" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF00FF">
    <Processor Type="CityVisitor" Power="0.9" Param1="15" Param2="30" />
  </System>
</Systems>
```

This creates five systems with groups distributed by weight:

- System 1 (weight 2, gets most groups): Zombies stick together in groups, avoid roads, and wander randomly
- System 2: Zombies go to buildings, chase loud noises, move faster, and chase you when they spawn
- System 3: Zombies prefer cities, congregate in towns, and stay in groups
- System 4: Zombies stick to the desert biome and avoid cities
- System 5: Zombies visit cities in rotation, migrating between different population centers

## Understanding Power and Distance

**Power**: How strong the behavior is
- 0.5 = weak effect
- 1.0 = medium effect
- 2.0 = strong effect

**Distance**: How far away (in meters) the behavior works
- 10.0 = only affects very close zombies
- 50.0 = affects zombies far away
- Only some behaviors use this setting

## Tips

- Start with Power values around 0.5 to 1.0, then adjust
- You can combine multiple behaviors (like FlockSameGroup + StickToPOIs)
- Higher Power values = stronger behavior
- Behaviors without Distance work on the whole map or use their own range
