In WalkerSim, agents are controlled by movement systems to create diverse behaviors, each system defines a set of movement processors that will control the movement of agents.
Movement systems also specify which groups of agents are controlled and addresses those groups by id.

The `<ProcessorGroup>` element within `<MovementProcessors>` in the XML configuration allows you to assign movement processors to a group using the `Group` attribute. A special group ID of `-1` applies processors to all groups or distributes them across groups in a round-robin fashion.

## Understanding Group IDs

Group IDs are integers that identify distinct groups of agents in the simulation. They control which groups of agents are affected by the movement processors defined in a `<ProcessorGroup>` within `<MovementProcessors>`.

  - **Assignment**: The number of groups is determined by dividing the total number of agents (calculated from `PopulationDensity` and the simulation area) by `GroupSize`. For example, if `PopulationDensity` is 100 agents per square kilometer, the simulation area is 1 km², and `GroupSize` is 20, there are 5 groups (IDs 0 through 4).
  - **Usage**: In the XML configuration, the `Group` attribute of a `<ProcessorGroup>` specifies which group the contained `<Processor>` elements apply to. For example, `Group="0"` applies processors to agents in group ID 0.
  - **Constraints**: Group IDs must be non-negative integers (0 or higher) with the exception of `-1` and cannot exceed `N-1`, where `N` is the total number of groups. If a `Group` value exceeds this limit, a warning is logged, and the processors for that group are ignored.
  - **Grouping Behavior**: The `StartAgentsGrouped` parameter determines if agents in the same group start close together. If `true`, agents are placed near each other based on `AgentStartPosition`.

**Important**: Group IDs are zero-based indices, `-1` is an exception and means it will be distributed evenly to all groups that have no other system assigned. 
If the simulation has `N` groups of agents (determined by the total number of agents, derived from `PopulationDensity`, divided by `GroupSize`), valid group IDs range from `0` to `N-1`. For example, with 4 groups, the maximum group ID is 3 (IDs: 0, 1, 2, 3).

### Example

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
  </ProcessorGroup>
</MovementProcessors>
```

This assigns the `FlockSameGroup` processor to agents in group ID 0, causing them to cluster with same-group agents at normal simulation speed (`SpeedScale="1.0"`), wander aimlessly after spawning, and be visualized in red.

## Groups with `Group="-1"`

Systems that specify a `Group` value of `-1` will evenly distribute among all groups that have no specific system assigned.

  - **Behavior**: When one or more `<ProcessorGroup>` elements have `Group="-1"`, the simulation assigns these processors to all groups, cycling through the `-1` definitions if multiple exist. For example, with 5 groups (IDs 0–4) and two `-1` processor groups, the first `-1` group is assigned to group 0, the second to group 1, the first to group 2, and so on.
  - **Purpose**: This allows applying a common set of processors to all agents or creating varied behaviors across groups without specifying each group individually.
  - **Color Handling**: The `Color` attribute is optional for each `<ProcessorGroup>` and only used by the Editor.

### Example

```xml
<MovementProcessors>
  <ProcessorGroup Group="-1" SpeedScale="1.0" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

With 5 groups, this applies the `WorldEvents` processor to all groups (0–4), with agents moving at normal simulation speed, chasing the entity or event that triggered their spawn, and visualized in green.

### Multiple `-1` Groups Example

```xml
<MovementProcessors>
  <ProcessorGroup Group="-1" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
  <ProcessorGroup Group="-1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#0000FF">
    <Processor Type="StickToRoads" Power="0.5" />
  </ProcessorGroup>
</MovementProcessors>
```

With 5 groups:

  - Group 0: `WorldEvents`, normal speed, wander, green.
  - Group 1: `StickToRoads`, 1.5x speed, chase activator, blue.
  - Group 2: `WorldEvents`, normal speed, wander, green.
  - Group 3: `StickToRoads`, 1.5x speed, chase activator, blue.
  - Group 4: `WorldEvents`, normal speed, wander, green.

This creates an alternating pattern of behaviors with different simulation movement speeds and post-spawn actions.

## Influencing Movement Processors with Groups

### Group
The `Group` attribute in `<ProcessorGroup>` tailors movement behaviors to specific agent subsets, enabling diverse simulation dynamics.

### SpeedScale
The `SpeedScale` attribute controls how fast agents move within the simulation (e.g., `1.0` for normal speed, `2.0` for double speed), but does not affect the movement speed of spawned entities in the game world.

### PostSpawnBehavior
The `PostSpawnBehavior` attribute defines what agents do immediately after spawning in the game world, with two options:

  - **Wander**: Agents move aimlessly in the game world after spawning, following the movement processors defined for their group. This is useful for creating ambient or exploratory behaviors, such as zombies roaming without a specific target.
  - **ChaseActivator**: Agents pursue the entity or event (e.g., a player or world event) that triggered their spawn. This creates aggressive or reactive behaviors, such as zombies chasing a player who caused their spawn.

### PostSpawnWanderSpeed
The `PostSpawnWanderSpeed` attribute defines the movement speed the spawned agents will use for wandering, if set to `NoOverride` it will use the setting from the game. When spawned agents
are alerted or start attacking the player then they will always use the setting from the game, this only applies for them wandering. The options are identical to those in the game.

  - **NoOverride**: Will use the game setting.
  - **Walk**: Normal walking speed.
  - **Jog**: Jogging speed.
  - **Run**: Running speed.
  - **Sprint**: Sprinting speed.
  - **Nightmare**: Pretty fast.

### MovementProcessors
The schema defines the following processor types (`MovementProcessorType`):

  - **FlockAnyGroup**: Agents flock with any group within `Distance`, using `Power`.
  - **AlignAnyGroup**: Agents align with any group within `Distance`, using `Power`.
  - **AvoidAnyGroup**: Agents avoid any group within `Distance`, using `Power`.
  - **FlockSameGroup**: Agents flock with their own group within `Distance`, using `Power`.
  - **AlignSameGroup**: Agents align with their own group within `Distance`, using `Power`.
  - **AvoidSameGroup**: Agents avoid their own group within `Distance`, using `Power`.
  - **FlockOtherGroup**: Agents flock with other groups within `Distance`, using `Power`.
  - **AlignOtherGroup**: Agents align with other groups within `Distance`, using `Power`.
  - **AvoidOtherGroup**: Agents avoid other groups within `Distance`, using `Power`.
  - **Wind**: Agents are influenced by wind, using `Power`.
  - **WindInverted**: Agents are influenced by inverted wind, using `Power`.
  - **StickToRoads**: Agents stick to roads, using `Power`.
  - **AvoidRoads**: Agents avoid roads, using `Power`.
  - **StickToPOIs**: Agents stick to points of interest, using `Power`.
  - **AvoidPOIs**: Agents avoid points of interest, using `Power`.
  - **WorldEvents**: Agents react to world events, using `Power`.

### Group Influence

  - **Specific Group IDs (e.g., `Group="0"`, `Group="1"`)**:

    - Processors apply only to the specified group.
    - Useful for distinct behaviors, e.g., one group flocks at normal speed and wanders, while another sticks to roads at higher speed and chases activators.
    - Overrides `-1` processors for that group.

  - **Generic Group ID (`Group="-1"`)**:

     - Processors are distributed across all groups, as described above.
     - Ideal for universal behaviors (e.g., all agents react to wind and wander) or alternating behaviors with varying speeds and post-spawn actions.

  - **Combining Specific and Generic Groups**:

    - Specific groups take precedence, and `-1` groups apply to undefined groups.
    - Example: Assign unique processors, speeds, and post-spawn behaviors to group 0 and use `-1` for others.

- **Processor Interactions**:

  - `FlockSameGroup`, `AlignSameGroup`, `AvoidSameGroup`, `FlockOtherGroup`, `AlignOtherGroup`, and `AvoidOtherGroup` rely on group IDs for same-group or other-group interactions.
  - `FlockAnyGroup`, `AlignAnyGroup`, and `AvoidAnyGroup` consider all nearby agents, but can be restricted to specific groups via `Group`.
  - `Wind`, `WindInverted`, `StickToRoads`, `AvoidRoads`, `StickToPOIs`, `AvoidPOIs`, and `WorldEvents` are group-agnostic but can be applied selectively.
  - `PostSpawnBehavior` influences how these processors are expressed after spawning. For example, `ChaseActivator` with `WorldEvents` makes agents aggressively pursue event sources, while `Wander` with `FlockSameGroup` creates cohesive, roaming groups.

### Example: Specific and Generic Groups

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="-1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

With 3 groups (IDs 0–2):

  - Group 0: `FlockSameGroup` and `AvoidRoads`, normal speed, wander, red. Agents cluster with same-group members, avoid roads, and roam aimlessly after spawning.
  - Group 1: `WorldEvents`, 1.5x speed, chase activator, green. Agents pursue the spawn trigger (e.g., a player) at faster simulation speed.
  - Group 2: `WorldEvents`, 1.5x speed, chase activator, green. Same as Group 1.

## Usage Notes

  - **Group ID Validation**: Ensure specific `Group` values do not exceed `N-1` (where `N` is total agents / `GroupSize`). Invalid IDs trigger a warning, and processors are ignored.
  - **Using `-1` for Simplicity**: Use `Group="-1"` for universal or alternating behaviors without specifying each group.
  - **Combining Behaviors**: Mix specific and generic groups for detailed control and broad applicability.
  - **SpeedScale**: The `SpeedScale` attribute (e.g., `1.0` for normal, `2.0` for double) controls agent movement speed in the simulation, not the speed of spawned entities in the game world.
  - **PostSpawnBehavior**: Required for each `<ProcessorGroup>`. `Wander` makes agents roam aimlessly, ideal for ambient behaviors; `ChaseActivator` makes agents pursue the spawn trigger, suitable for aggressive or event-driven behaviors.
  - **Visualization**: The `Color` attribute (e.g., `#FF0000`) is required and sets agent color in the viewer.
  - **Processor Attributes**:
    - `Distance`: Optional, used by `Flock*`, `Align*`, and `Avoid*` processors for nearby agent queries.
    - `Power`: Required, determines influence strength for all processors.
  - **Performance**: Large numbers of groups or processors may impact performance, especially with complex behaviors.
  - **Debugging**: Enable `LogSpawnDespawn` in `<DebugOptions>` to log group-related spawn/despawn events.
  - **Positioning**: `AgentStartPosition` and `AgentRespawnPosition` (`RandomBorderLocation`, `RandomLocation`, `RandomPOI`, `Mixed`, or `None` for respawn) affect group placement, especially when `StartAgentsGrouped` is `true`.
  - **Simulation Start**: `FastForwardAtStart` accelerates initial simulation setup, and `SpawnProtectionTime` delays agent spawning for new players.

## Example XML Configuration

Below is a schema-compliant `<MovementProcessors>` configuration:

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="1" SpeedScale="1.2" PostSpawnBehavior="ChaseActivator" Color="#0000FF">
    <Processor Type="StickToPOIs" Power="0.5" />
  </ProcessorGroup>
  <ProcessorGroup Group="-1" SpeedScale="1.5" PostSpawnBehavior="Wander" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

With 4 groups (IDs 0–3):

  - Group 0: Clusters with same-group agents and avoids roads, normal speed, wanders (red). Agents roam aimlessly after spawning.
  - Group 1: Moves toward POIs, 1.2x speed, chases activator (blue). Agents pursue the spawn trigger along points of interest.
  - Group 2: Reacts to world events, 1.5x speed, wanders (green, from `-1`). Agents roam while reacting to events.
  - Group 3: Reacts to world events, 1.5x speed, wanders (green, from `-1`). Same as Group 2.

## Full WalkerSim Example

To illustrate groups in context, here’s a complete `<WalkerSim>` configuration:

```xml
<WalkerSim>
  <DebugOptions>
    <LogSpawnDespawn>true</LogSpawnDespawn>
  </DebugOptions>
  <RandomSeed>1234</RandomSeed>
  <PopulationDensity>100</PopulationDensity>
  <StartAgentsGrouped>true</StartAgentsGrouped>
  <FastForwardAtStart>true</FastForwardAtStart>
  <GroupSize>20</GroupSize>
  <AgentStartPosition>RandomPOI</AgentStartPosition>
  <AgentRespawnPosition>RandomBorderLocation</AgentRespawnPosition>
  <PauseDuringBloodmoon>false</PauseDuringBloodmoon>
  <SpawnProtectionTime>300</SpawnProtectionTime>
  <MovementProcessors>
    <ProcessorGroup Group="0" SpeedScale="1.0" PostSpawnBehavior="Wander" Color="#FF0000">
      <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
      <Processor Type="AvoidRoads" Power="0.4" />
    </ProcessorGroup>
    <ProcessorGroup Group="-1" SpeedScale="1.5" PostSpawnBehavior="ChaseActivator" Color="#00FF00">
      <Processor Type="WorldEvents" Power="0.8" />
    </ProcessorGroup>
  </MovementProcessors>
</WalkerSim>
```

This creates 5 groups (assuming 100 agents from `PopulationDensity` / 20), with group 0 clustering and avoiding roads at normal speed while wandering, and groups 1–4 chasing world event activators at 1.5x speed. Agents start at POIs, respawn at borders, are grouped at the start, and have a 300-second spawn protection for new players.

This guide aligns with the latest WalkerSim schema and provides a user-friendly reference for configuring groups. For more details, refer to the schema or other WalkerSim documentation.