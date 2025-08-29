# WalkerSim 2

WalkerSim transforms the way zombies and hordes behave in 7 Days to Die.  
Instead of relying on random spawns, it runs a **persistent simulation** of zombie activity across the world.  
In this system, zombies — referred to as **agents** — exist and move within a **virtual 2D world** that mirrors the actual game map.

These agents do not interact with the player or the game until conditions are right for them to be spawned, creating a world that feels much more alive and reactive.

---

## How Does It Work?

At the heart of WalkerSim is a **virtual 2D simulation** that represents the loaded 7 Days to Die world.  
When the game starts, WalkerSim generates thousands of virtual agents based on the map size and configured population density.

These agents roam the virtual 2D world by following simple, configurable behaviors. Examples include:

 - Favoring roads and paths
 - Grouping together in herds
 - Staying near Points of Interest (POIs)
 - Wandering freely based on environmental factors

Agents move constantly within the simulation, completely independent of whether players are nearby.  
Only when they come close to a player will they cross into the real game world as actual zombies.

Below is a visual example of the simulation in action (sped up for better demonstration):
<figure markdown="1">
![Simulation Example](img/simulation.gif)
</figure>
## Is this a server mod?

Yes. You can install WalkerSim2 on dedicated servers. Clients do ***NOT*** require to install it to join the server.

---

## When Do Zombies Spawn?

When a player joins the world, WalkerSim registers them into the simulation with a view radius based on their chunk loading distance.

As players move through the world, agents (simulated zombies) wander independently inside the virtual 2D simulation.  
When an agent enters a player's activation zone, it triggers a spawn event — bringing the agent into the real game world as a visible, interactive zombie.
<figure markdown="1">
![Spawn Activation](img/activation.png)
</figure>

The activation zone is the border of the view radius and has the size of 8 meters. Agents that manage to enter the inner circle will not spawn, they sometimes
can end up in the inner circle when they fail to spawn and that can be due to various reasons, a common cause is the position at where they activate the spawn
is not a suitable location to place them in-game.

Importantly, not all spawned zombies are new.  
If an agent had previously been spawned, it retains its current state such as health and entity class inside the simulation which means, 
when it spawns again near a player, it will have the same health and entity class as before.

The type of zombie that appears is determined by:

- The biome the player is currently in
- The spawn rules defined in the game's `entitygroups.xml`
- POI tags.

This system ensures that zombies feel persistent, reactive, and naturally tied to the world rather than randomly appearing.

---

## When Do Zombies Despawn?

WalkerSim continues tracking spawned zombies after they appear in the game.  
If a zombie moves beyond the player’s maximum view range, it is **despawned** and returned to the simulation, preserving:

- Its health
- Its zombie class/type

When that same agent later returns to a player’s area, it spawns again, maintaining full continuity as if it had been wandering all along.

---

## Will it influence Animal spawning?

No.

## Will it influence the Bloodmoon Hordes?

Yes and No, this can be configured, by default the simulation will not spawn any new zombies during Bloodmoon.

---

