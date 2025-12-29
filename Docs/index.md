# WalkerSim 2

## What Does This Mod Do?

WalkerSim 2 changes how zombies work in 7 Days to Die.

Normally, zombies just appear randomly near you. With this mod, zombies exist all the time on the map - even when you can't see them.

## How It Works

Think of it like this: The mod creates a pretend version of your game map. Thousands of virtual zombies walk around on this pretend map all the time.

These virtual zombies can:

- Walk on roads
- Group together
- Hang around buildings
- Wander around randomly

They keep walking around whether you are near them or not.

When a virtual zombie gets close to where you are, the mod makes it turn into a real zombie in your game.

Below is a visual example of the simulation in action (sped up for better demonstration):
<figure markdown="1">
![Simulation Example](img/simulation.gif)
</figure>

## Is This a Server Mod?

Yes. The mod runs on the server side. This means:

- Install it on servers
- Players joining don't need to install anything

---

## When Do Zombies Appear?

When you get close to a virtual zombie, it appears in your game as a real zombie you can see and fight.

<figure markdown="1">
![Spawn Activation](img/activation.png)
</figure>

The picture above shows how this works. Virtual zombies walk around in a circle around you. When they cross into the colored ring near the edge, they become real zombies in your game.

The zombie that appears matches your area:

- Desert zombies in the desert
- Snow zombies in the snow
- Uses the game's normal zombie spawn rules

---

## When Do Zombies Disappear?

When zombies walk too far away from you, they turn back into virtual zombies. The mod remembers:

- How much health they had
- What type of zombie they were

So if that same zombie comes back later, it will still be hurt if you damaged it before.

## When Do Zombies Respawn?

Zombies will only truly respawn if they are killed.

## Where Do Zombies Respawn?

Where zombies respawn in the simulation depends on the configuration, see [AgentRespawnPosition](configuring/configuration/base.md#agentrrespawnposition).

## Will I have to keep the Editor running?

No. The Editor is a tool to help create a configuration and export it to the xml.

## Does This Change Animals?

No. Animals work exactly the same as in the normal game.

## What About Blood Moon?

You can choose in the settings. By default, the mod stops during Blood Moon nights so the normal Blood Moon zombies work as usual, see [PauseDuringBloodmoon](configuring/configuration/base.html#pauseduringbloodmoon)
