# 0.9.6 (in development)

# 0.9.5
- Fix: The editor no longer resets the simulation when changing parameters.
- Fix: Some zombies were not despawned when getting too far away.
- Fix: Console help text was not correctly printed.
- Fix: Changing the World in the Editor does not update the preview.
- Fix: Zombies start running after being spawned.
- Fix: Zombies trying to dig when using `Wander` for `PostSpawnBehavior`
- Improve: Simulation is now fully deterministic.
- Improve: Using faster speeds in the Editor will now remain accurate, uses multi-threading.
- Improve: Default configuration has been updated for a more immersive experience.
- Improve: Change random distribution of POI's, bigger POI's will be prefered now.
- Feature: Added `SpawnProtectionTime`, this disables zombie spawning when entering a new game for the first time.
- Feature: Added `PostSpawnBehavior` which controls what agents will do once spawned in-game, right now limited to `Wander` and `ChaseActivator`.
- Feature: The Editor can now zoom in and out on the simulation preview.

# 0.9.4
- Fix: Zombie spawning now obeys tags of biome spawner.
- Fix: Zombies killed are removed from the active list, reducing excessive logging and correctly respawning zombies.
- Fix: Some agents were not updated depending on the population count.
- Improve: Reduce memory footprint and better performance by limiting the spawn queue.
- Improve: All players now have a chance to get zombie spawns, previously a single player could potentially exhaust the maximum allowed.
- Improve: Detection of where 7 Days to Die is installed, this is now obtained from Steam data.

# 0.9.3
- Fix: Flock processors always navigating towards the center.

# 0.9.2
- Fix: Support for version 1.2.
- Change: WalkerSim.xml configuration changed, old configuration files will no longer work.
- Change: Print a hint to the console when the local configuration does not match the one in the saved simulation.

# 0.9.1
- Change: Raise density limit to 4000 and max agents to 1000000.
