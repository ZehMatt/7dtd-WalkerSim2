[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/ZehMatt/zasm/blob/master/LICENSE)
[![Discord](https://img.shields.io/discord/243577046616375297.svg?logo=discord&label=Discord)](https://discord.gg/9QGHS4wbFu)

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

Below is a visual example of the simulation in action (sped up 16x for better demonstration):

![Simulation Example](./Docs/assets/simulation.gif?raw=true)

## Documentation

You can find the full documentation [here](https://7dtd-walkersim2.readthedocs.io/).
