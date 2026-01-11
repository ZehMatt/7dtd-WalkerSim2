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
Zombies move toward the nearest road and follow it.

- Uses Distance: No
- Example: Zombies walk along roads and highways

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
Zombies visit cities in a rotation pattern. Each zombie group targets one city for 20 minutes, wandering throughout the area, before moving to the next city in the sequence. Different zombie groups may target different cities at the same time.

- Uses Distance: No
- Example: Zombies spend time in one city exploring it, then travel to the next city when 20 minutes have passed

## Example Settings

Here's how to set up zombie behaviors in your config file:

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
    <Processor Type="StickToPOIs" Power="0.5" />
  </ProcessorGroup>
  <ProcessorGroup Group="2" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FFFF00">
    <Processor Type="PreferCities" Power="0.7" />
    <Processor Type="FlockSameGroup" Distance="20.0" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="3" SpeedScale="1.2" PostSpawnBehavior="Wander" Color="#00FFFF">
    <Processor Type="AvoidCities" Distance="100.0" Power="0.8" />
  </ProcessorGroup>
  <ProcessorGroup Group="4" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF00FF">
    <Processor Type="CityVisitor" Power="0.9" />
  </ProcessorGroup>
</MovementProcessors>
```

This creates:

- Group 0: Zombies stick together in groups, avoid roads, and wander randomly
- Group 1: Zombies go to buildings, chase loud noises, move faster, and chase you when they spawn
- Group 2: Zombies prefer cities, congregate in towns, and stay in groups
- Group 3: Zombies avoid cities within 100 meters and stay in the wilderness
- Group 4: Zombies visit cities in rotation, migrating between different population centers

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
