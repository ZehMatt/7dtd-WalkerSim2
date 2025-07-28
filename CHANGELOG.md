# 0.9.10 (in progress)
- Fix: Better compatibility with NPC mods, simulation only spawns zombie based entities now.
- Improve: Editor can now load and save the state, this allows inspecting the saved `walkersim.bin`.
- Feature: Added `EnhancedSoundAwareness`, this improves the distance to where spawned zombies react to noises.
 
# 0.9.9
- Fix: Clients without admin permission can restart the simulation with the console command.
- Fix: Activation radius was too big causing issues with spawning.
- Fix: During spawn it could select the entity class `None` which was substituted by `ZombiesAll` which skewed the probabilities causing some classes to never spawn.
- Fix [#15]: Pausing the simulation via command is not working.
- Change: Add option for the activation radius `SpawnActivationRadius`, default is 96m.
- Change: Added options to change the logging behavior, also log into a dedicated WalkerSim log file.

# 0.9.8
- Change: Support for game version 2.0, older versions will not work.

# 0.9.7
- Fix: Allow clients with admin permission to run console commands such as `walkersim stats`, `walkersim show` will still only work on local games.
- Fix: Incorrect calculation of the view radius, the spawn area was reduced by 50%.
- Fix: State persisting between games when playing single player causing unintentional problems.
- Change: Use 80% of maximum allowed zombies instead of 50%, 20% will be reserved for sleepers.
- Improve: Spawning logic now takes into account how many spawned zombies are near players preventing one player taking all spawn slots, better balances multiplayer games.

# 0.9.6
- Fix: Editor crash trying to locate generated worlds.
- Fix: Condition that allowed to spawn more zombies than the game allows.
- Improve: When zombies fail to spawn the next attempt will be delayed as it will otherwise stall the spawn of other zombies.
- Improve: Better performance for the in-game preview using `walkersim show`.
- Change: Remove dependency to System.Drawing which eliminates the need to install libgdiplus on linux.

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
