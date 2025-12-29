# Sound System

One of WalkerSim's most dynamic features is how it handles sounds. Unlike the base game where sounds only affect nearby zombies, WalkerSim makes sounds influence the entire simulation, creating a more realistic and strategic gameplay experience.

## How It Works

When you make loud noises in the game world - firing guns, causing explosions, or breaking materials - WalkerSim captures these sounds and creates **sound events** in the simulation. These events have:

- **Position**: Where the sound originated
- **Radius**: How far the sound can be heard
- **Duration**: How long the sound continues to attract zombies

Virtual zombies across the map react to these sound events, even if they're far away and not yet spawned as real zombies. They'll start moving toward the sound source, gradually converging on that location.

## What Makes Sounds?

WalkerSim uses the game's built-in noise system, which means it responds to all the same sounds that normally attract zombies:

### Weapons
- **Guns** - Different weapons have different noise levels. A pistol is quieter than a shotgun, which is quieter than an automatic rifle
- **Explosives** - TNT, grenades, rockets, and pipe bombs create massive sound events
- **Bows and crossbows** - Entirely silent
- **Melee weapons** - Entirely silent

### Activities
- **Breaking materials** - Different materials make different amounts of noise:
  - Glass blocks: Loud
  - Metal blocks: Quite loud
  - Wood blocks: Moderate noise
  - Stone blocks: Quite small
  - Digging stone: Very small
- **Falling trees** - Trees make noise when they fall
- **Looting cars** - Very small amount of noise
- **Destroying cars** - Reasonably loud
- **Vehicles** - Minibikes, 4x4 trucks, and gyrocopters generate no noise
- **Generators** - Generate no noise
- **Large gates** - Make noise when opening/closing
- **Doors and hatches** - Silent

### Volume Scaling
The game automatically scales sound volume based on various factors:

- Player stealth modifiers
- Silencer attachments on weapons
- Crouching (some sounds are muffled when crouched)
- Environmental factors

## Sound Event Merging

To keep performance smooth, WalkerSim intelligently merges nearby sound events. When multiple sounds happen close together:

- If sounds occur within 25 meters of each other, they merge into a single larger event
- The merged event takes the longer duration of the two sounds
- The radius grows to encompass both sound sources (up to a maximum of 500 meters)
- The position shifts slightly toward the center of both sounds

This creates realistic scenarios - for example, a prolonged firefight will create a large, sustained sound event rather than hundreds of tiny individual ones.

## Two Types of Responses

WalkerSim handles sound in two distinct ways:

### 1. Virtual Zombie Response (Simulation-Wide)

**What happens**: All virtual zombies in the simulation that can "hear" the sound start moving toward it.

**Characteristics**:

- Affects virtual zombies across the entire map
- Creates lasting consequences - zombies keep converging even after you leave
- Sound events persist for their full duration (typically 1-2 minutes)
- Zombies within the sound radius will gradually cluster around the source
- Always active, regardless of configuration

**Result**: When you return to an area where you made noise, you might find it swarming with zombies that wandered in from surrounding areas.

### 2. Enhanced Sound Awareness (Real Zombies)

**What happens**: Already-spawned zombies in the game world are immediately alerted and investigate the sound.

**Characteristics**:

- Only affects zombies that are already spawned as real game entities
- Works within the sound's calculated travel distance
- Zombies in idle state will investigate the sound source
- They'll remain interested for about 2 minutes
- Can be disabled via configuration ([EnhancedSoundAwareness](configuring/configuration/base.md#enhancedsoundawareness))

**Result**: Nearby zombies immediately turn and start moving toward the sound, even if they couldn't see you before.

## Sound Calculations

When a sound occurs, WalkerSim calculates how far it travels using this formula:

```
Travel Distance = (Volume × Volume Scale × 3.0) × Sound Distance Scale
```

Where:

- **Volume**: Base volume from the game's noise data
- **Volume Scale**: Modifier based on circumstances (silencers, crouching, etc.)
- **Sound Distance Scale**: Your configuration setting (default 1.0)

The game then scales this distance based on the sound's "heat map strength" - a value that determines how much the sound should attract zombies (0.0 to 1.0).

## Configuration Options

### EnhancedSoundAwareness

**Default**: `true`

**What it controls**: Whether already-spawned zombies are immediately alerted to sounds.

- **True**: Spawned zombies will investigate sounds they can hear
- **False**: Only virtual zombies in the simulation respond to sounds

**When to disable**: 

- If you want sounds to only affect the long-term simulation without immediate consequences
- If you prefer more predictable zombie behavior
- For a more traditional 7 Days to Die experience

See: [EnhancedSoundAwareness configuration](configuring/configuration/base.md#enhancedsoundawareness)

### SoundDistanceScale

**Default**: `1.0`

**What it controls**: Multiplier for how far sounds travel in the simulation.

- **1.0**: Normal distance (game default)
- **2.0**: Sounds travel twice as far
- **0.5**: Sounds only travel half the distance
- **Higher values**: More zombies affected, more challenging
- **Lower values**: Fewer zombies affected, more forgiving

**Examples**:
- Set to `1.5` to make guns and explosions attract zombies from farther away
- Set to `0.7` to reduce the impact of noise on zombie distribution
- Set to `2.0` or higher for extreme challenge where any noise has map-wide consequences

See: [SoundDistanceScale configuration](configuring/configuration/base.md#sounddistancescale)

## Strategic Implications

Understanding the sound system changes how you play:

### Combat Strategies

**Quieter weapons exist**: Some weapons generate less noise than others. Experiment to find which tools and weapons work best for stealthy gameplay.

**Noise creates danger zones**: After a big fight, that area becomes a zombie magnet. Other zombies from across the simulation will converge there, making it dangerous to return soon after.

**Timing matters**: If you need to make noise (mining, combat), consider doing it before leaving an area so you're not there when zombies arrive.

### Base Building

**Location matters more**: Building near roads or POIs where zombies naturally cluster means your construction noise will affect more zombies.

**Construction planning**: Some activities during building may generate noise (such as chopping down trees). Plan construction sessions carefully, or build far from zombie-heavy areas.

### Looting and Exploration

**Some breaking is noisy**: Breaking certain materials like glass creates noise. Virtual zombies will start converging on that location.

**Return trips are riskier**: That building you looted yesterday might have a horde around it today from the noise you made.

**Plan escape routes**: After making noise looting, be ready to leave the area before zombies arrive.

## Viewing Sound Events

In singleplayer, you can use console commands to monitor sound events:

- **`walkersim show`** - Opens the map with temporary overlay showing sound events as circles
- **`walkersim map enable`** - Permanently enables the map overlay to see sound events

Sound events appear as colored circles on the map overlay, with the radius showing how far they can be heard.

See: [Console Commands](commands.md)

**Related Configuration**:
- [EnhancedSoundAwareness](configuring/configuration/base.md#enhancedsoundawareness)
- [SoundDistanceScale](configuring/configuration/base.md#sounddistancescale)
- [Logging Options](configuring/configuration/base.md#logging)

**Related Commands**:
- [walkersim show](commands.md#show)
- [walkersim map](commands.md#map)
- [walkersim stats](commands.md#stats)
