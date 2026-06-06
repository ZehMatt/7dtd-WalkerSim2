# Game Stage Spawning (Advanced)

!!! warning "Advanced Feature for Overhauls"
    This feature only does something when the game's `spawning.xml` uses the `mings`/`maxgs` attributes. The base game does not ship them, so on vanilla setups this has no effect and spawning behaves exactly as before. It is primarily useful for overhaul mods that scale zombie difficulty with the player game stage.

## What It Does

The game's `spawning.xml` controls which entity groups spawn in each biome. Some overhaul mods add two extra attributes to a `<spawn>` entry to restrict it to a game stage range:

- `mings` - the minimum game stage (inclusive) for the entry to be used
- `maxgs` - the maximum game stage (inclusive) for the entry to be used

The base game **ignores** these attributes entirely, so its own spawner and any mod relying on it must reimplement the gating. WalkerSim now reads them directly and applies the same gating to its own spawns, so roaming zombies match the game stage progression an overhaul intends.

## How It Works

When WalkerSim selects what to spawn at a location, it:

1. Samples the **combined game stage of the players near the spawn**, using the same diminishing returns party formula the game uses for hordes and sleepers (the highest game stage counts fully, the next half, and so on).
2. Takes the candidate biome spawn groups for that location (biome plus POI tags, exactly as before).
3. Keeps only the groups whose `[mings, maxgs]` range contains the sampled game stage.
4. Picks an entity from the remaining groups as usual.

### Attribute Rules

| Attributes present | Behavior |
|---|---|
| neither `mings` nor `maxgs` | Ungated. The entry is always eligible (unchanged from before). |
| `mings` only | No upper bound. Eligible once the game stage reaches `mings`. |
| `maxgs` only | No lower bound. Eligible until the game stage passes `maxgs`. |
| both | Eligible only while the game stage is within the range. |

Entries that specify neither attribute are never affected, so mixing gated and ungated entries in the same biome works as expected.

## Where the Data Comes From

WalkerSim reads the **fully merged and patched** `spawning.xml` that the game loads, so every modlet's xpath patches are already applied and load order is respected. You do not configure anything in `WalkerSim.xml` for this; you (or the overhaul) add `mings`/`maxgs` to the game's spawn entries.

### Example

A modlet patch that gates a biome's night spawns by game stage:

```xml
<append xpath="/spawning/biome[@name='pine_forest']">
    <spawn id="ws_pf_n_early" time="Night" maxcount="999" respawndelay="0.9" entitygroup="ZombiesAll"   maxgs="32"/>
    <spawn id="ws_pf_n_mid"   time="Night" maxcount="999" respawndelay="0.9" entitygroup="ZombiesNight" mings="33" maxgs="65"/>
    <spawn id="ws_pf_n_late"  time="Night" maxcount="999" respawndelay="0.9" entitygroup="ZombiesFeral" mings="66"/>
</spawning>
```

At game stage 20 only the first entry is eligible, at 50 only the second, and from 66 onward only the third.

!!! note "Each entry needs a unique id"
    The game identifies each `<spawn>` by its `id` within a biome and refuses to load two entries with the same `id` in the same biome. WalkerSim keys the game stage range by that same `id`, so always give gated entries distinct ids.

## Interaction With Spawn Group Masks

Game stage gating applies to the biome based spawning path only. The [Spawn Group Masks](spawn-groups.md) feature (`ws_spawngroups.png`) is an explicit override and is not game stage gated.

## Verifying It Works

Use the [`walkersim gamestages`](../commands.md#gamestages) console command at your location. It prints your game stage, the sampled area game stage, how many gated entries were captured, and for each biome spawn group its captured range (or `ungated`) and whether it is currently eligible. A group showing a real range rather than `ungated` confirms the `mings`/`maxgs` were matched to that group.

!!! note "Host Only"
    Like all spawning, this runs on the host (or server). Clients do not make spawn decisions.
