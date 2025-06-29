The base parameters are defined as child elements of the `<WalkerSim>` root element in the XML configuration. They control essential aspects of the simulation, such as the random seed for reproducibility, the number of agents, their initial and respawn positions, and whether the simulation pauses during specific events. These settings provide the foundation for how the simulation initializes and operates.

---

## Parameters

Below is a list of all base parameters, their descriptions, types, constraints, and usage details.

### 1. `DebugOptions`

- **Type**: Complex (Optional)
- **Description**: Configures debugging options to assist with troubleshooting the simulation.
- **Child Element**:
  - `LogSpawnDespawn` (Type: Boolean, Description: If `true`, logs when agents are spawned or despawned, useful for tracking agent lifecycle).
- **Default**: Not included (debugging disabled unless specified).
- **Example Use Case**: Enabling spawn/despawn logging to debug agent population issues.

### 2. `RandomSeed`

- **Type**: Integer
- **Description**: Sets the seed for random number generation to ensure reproducible simulation results. Use the same seed for consistent outcomes across runs.
- **Constraints**: Any valid integer.
- **Example Use Case**: Setting a fixed seed (e.g., `12345`) to test specific scenarios repeatedly.

### 3. `PopulationDensity`

- **Type**: Integer
- **Description**: Specifies the number of agents per square kilometer in the simulation area, controlling the overall agent population.
- **Constraints**: Minimum: 1, Maximum: 4000.
- **Example Use Case**: Setting a density of `200` for a moderately populated zombie simulation.

### 4. `SpawnActivationRadius`

- **Type**: Integer
- **Description**: The radius for the player in blocks/meters for when agents will spawn/despawn in the game world. This should not exceed the maximum view distance from serversettings.xml, view distance is specified in chunks and each chunk is 16x16x16.
- **Constraints**: Minimum: 48, Maximum: 196.
- **Note**: The default is set to 96, setting this too high can cause a lot of spawn failures, setting it to a lower value is not recommended. 

### 5. `StartAgentsGrouped`

- **Type**: Boolean
- **Description**: Determines whether agents start the simulation grouped together. If `true`, agents are placed in clusters based on the `GroupSize` parameter; if `false`, they are distributed individually.
- **Example Use Case**: Setting to `true` to simulate zombies starting in tight-knit hordes.

### 6. `GroupSize`

- **Type**: Integer
- **Description**: Defines the size of each agent group when `StartAgentsGrouped` is `true`. The total number of groups is calculated as the total number of agents divided by this value.
- **Constraints**: Minimum: 1.
- **Example Use Case**: Setting to `20` to create groups of 20 zombies each.

### 7. `AgentStartPosition`

- **Type**: Enumeration (`SpawnPosition`)
- **Description**: Specifies where agents are placed at the start of the simulation.
- **Values**:
  - `RandomBorderLocation`: Agents start at random locations along the simulation area’s borders.
  - `RandomLocation`: Agents start at random locations throughout the simulation area.
  - `RandomPOI`: Agents start at random points of interest (e.g., buildings or landmarks).
  - `Mixed`: Agents start at a mix of border locations, random locations, and POIs.
- **Example Use Case**: Using `RandomPOI` to simulate zombies spawning near key locations.

### 8. `AgentRespawnPosition`

- **Type**: Enumeration (`RespawnPosition`)
- **Description**: Specifies where agents respawn after being removed (e.g., due to death or despawning).
- **Values**:
  - `None`: Agents do not respawn.
  - `RandomBorderLocation`: Agents respawn at random border locations.
  - `RandomLocation`: Agents respawn at random locations.
  - `RandomPOI`: Agents respawn at random points of interest.
  - `Mixed`: Agents respawn at a mix of border locations, random locations, and POIs.
- **Example Use Case**: Setting to `RandomBorderLocation` to simulate zombies re-entering from the edges.

### 9. `PauseDuringBloodmoon`

- **Type**: Boolean
- **Description**: Determines whether the simulation pauses during specific events, such as a "Bloodmoon" (a predefined event that may intensify zombie activity). If `true`, the simulation halts during these events.
- **Example Use Case**: Setting to `true` to pause during high-intensity events for performance or gameplay reasons.

---

## Usage Notes

- **DebugOptions**: Use `LogSpawnDespawn` sparingly, as excessive logging may impact performance. Only include `<DebugOptions>` when debugging is needed.
- **RandomSeed**: Choose a specific seed for testing or debugging to ensure consistent results. For varied simulations, change the seed or omit it to use a random one.
- **PopulationDensity**: Balance density with performance. High values (e.g., 4000) may slow down the simulation on less powerful systems.
- **GroupSize and StartAgentsGrouped**: These parameters work together. Ensure `GroupSize` is reasonable relative to the total number of agents to avoid creating too few or too many groups.
- **AgentStartPosition and AgentRespawnPosition**: Choose values that align with the simulation’s narrative. For example, `RandomPOI` for starts and `None` for respawns can simulate zombies gathering at key locations without replenishing.
- **PauseDuringBloodmoon**: Use this to manage simulation intensity during special events, especially in scenarios with high agent activity.

---

## Example XML Configuration

Below is an example XML configuration snippet for the base parameters within the `<WalkerSim>` root element:

```xml
<WalkerSim>
  <DebugOptions>
    <LogSpawnDespawn>true</LogSpawnDespawn>
  </DebugOptions>
  <RandomSeed>12345</RandomSeed>
  <PopulationDensity>200</PopulationDensity>
  <StartAgentsGrouped>true</StartAgentsGrouped>
  <GroupSize>20</GroupSize>
  <AgentStartPosition>RandomPOI</AgentStartPosition>
  <AgentRespawnPosition>RandomBorderLocation</AgentRespawnPosition>
  <PauseDuringBloodmoon>true</PauseDuringBloodmoon>
</WalkerSim>
```