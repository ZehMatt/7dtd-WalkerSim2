# 0.9.4 (ongoing)
- Fix: Zombie spawning now obeys tags of biome spawner
- Fix: Zombies killed are removed from the active list, reducing excessive logging and correctly respawning zombies.
- Fix: Some agents were not updated depending on the population count.
- Improve: Reduce memory footprint and better performance by limiting the spawn queue.
- Improve: All players now have a chance to get zombie spawns, previously a single player could potentially exhaust the maximum allowed.

# 0.9.3
- Fix: Flock processors always navigating towards the center.

# 0.9.2
- Fix: Support for version 1.2.
- Change: WalkerSim.xml configuration changed, old configuration files will no longer work.
- Change: Print a hint to the console when the local configuration does not match the one in the saved simulation.

# 0.9.1
- Change: Raise density limit to 4000 and max agents to 1000000.
