Movement processors determine the movement and interactions of agents in the simulation. They can be applied to specific groups of agents or all agents, as defined in the XML configuration. Each processor uses a `Power` parameter to control the strength of its effect and, in some cases, a `Distance` parameter to define the range of influence (in meters).

**Important**: Not all processors use the `Distance` parameter. When configuring processors in the XML, the `Distance` attribute is optional and ignored by processors that do not use it.

---

## Common Parameters

- **`Power`**: A decimal value that scales the strength of the processor’s effect. Higher values result in stronger movement influences (e.g., 0.5 for moderate, 2.0 for strong).
- **`Distance`**: A decimal value (in meters) defining the range within which the processor affects nearby agents or objects. **Not all processors use this parameter** (see individual descriptions).

---

## Processors

Below is a list of all movement processors, their purposes, behaviors, and whether they use the `Distance` parameter.

### 1. `FlockAnyGroup`

- **Description**: Moves the agent toward the center of all nearby agents, regardless of their group.
- **Behavior**: Attracts the agent to the average position of nearby agents within the specified distance, creating a clustering effect.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Simulating zombies grouping together in a crowd.

### 2. `FlockSameGroup`

- **Description**: Moves the agent toward the center of nearby agents in the same group.
- **Behavior**: Similar to `FlockAnyGroup`, but only affects agents in the same group, promoting group cohesion.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Keeping zombies of the same type together.

### 3. `FlockOtherGroup`

- **Description**: Moves the agent toward the center of nearby agents in different groups.
- **Behavior**: Attracts the agent to the average position of agents in other groups within the specified distance.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Simulating zombies drawn to rival groups.

### 4. `AlignAnyGroup`

- **Description**: Aligns the agent’s movement direction with that of all nearby agents.
- **Behavior**: Adjusts the agent’s direction to match the average direction of nearby agents within the specified distance, creating synchronized movement.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Simulating a unified movement pattern in a zombie horde.

### 5. `AlignSameGroup`

- **Description**: Aligns the agent’s movement direction with that of nearby agents in the same group.
- **Behavior**: Similar to `AlignAnyGroup`, but only considers agents in the same group.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Ensuring group members move in the same direction.

### 6. `AlignOtherGroup`

- **Description**: Aligns the agent’s movement direction with that of nearby agents in different groups.
- **Behavior**: Adjusts the agent’s direction to match the average direction of agents in other groups within the specified distance.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Simulating zombies mimicking the movement of other groups.

### 7. `AvoidAnyGroup`

- **Description**: Moves the agent away from all nearby agents.
- **Behavior**: Pushes the agent away from nearby agents within the specified distance, with stronger repulsion for closer agents, preventing overcrowding.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Spreading out zombies to avoid clustering.

### 8. `AvoidSameGroup`

- **Description**: Moves the agent away from nearby agents in the same group.
- **Behavior**: Similar to `AvoidAnyGroup`, but only affects agents in the same group.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Preventing zombies in the same group from bunching up.

### 9. `AvoidOtherGroup`

- **Description**: Moves the agent away from nearby agents in different groups.
- **Behavior**: Pushes the agent away from agents in other groups within the specified distance.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Simulating territorial behavior between zombie groups.

### 10. `Wind`

- **Description**: Moves the agent in the direction of the wind.
- **Behavior**: Applies a constant movement influence in the wind’s direction, scaled by `Power`. The `Distance` parameter is ignored.
- **Uses Distance**: **No**.
- **Example Use Case**: Simulating zombies drifting with the wind.

### 11. `WindInverted`

- **Description**: Moves the agent against the direction of the wind.
- **Behavior**: Applies a movement influence opposite to the wind’s direction, scaled by `Power`. The `Distance` parameter is ignored.
- **Uses Distance**: **No**.
- **Example Use Case**: Simulating zombies resisting the wind.

### 12. `StickToRoads` 

- **Description**: Moves the agent toward the nearest road.
- **Behavior**: Attracts the agent to the closest road, with stronger attraction to asphalt roads than offroad paths. The `Distance` parameter is ignored.
- **Uses Distance**: **No**.
- **Example Use Case**: Simulating zombies following roads or paths.

### 13. `AvoidRoads`

- **Description**: Moves the agent away from the nearest road.
- **Behavior**: Repels the agent from the closest road, with stronger repulsion for asphalt roads. The `Distance` parameter is ignored.
- **Uses Distance**: **No**.
- **Example Use Case**: Simulating zombies avoiding infrastructure.

### 14. `StickToPOIs`

- **Description**: Moves the agent toward the nearest point of interest (POI), such as a building or landmark.
- **Behavior**: Attracts the agent to the closest POI, ignoring POIs already crowded with agents. The `Distance` parameter is ignored.
- **Uses Distance**: **No**.
- **Example Use Case**: Simulating zombies gathering at key locations.

### 15. `AvoidPOIs`

- **Description**: Moves the agent away from nearby points of interest (POIs).
- **Behavior**: Repels the agent from POIs within the specified distance, with stronger repulsion for closer POIs.
- **Uses Distance**: **Yes** (meters).
- **Example Use Case**: Simulating zombies avoiding populated areas.

### 16. `WorldEvents`

- **Description**: Moves the agent toward noise-based events, such as explosions or disturbances.
- **Behavior**: Attracts the agent to the location of noise events, with stronger attraction closer to the event. The `Distance` parameter is ignored (uses the event’s radius instead).
- **Uses Distance**: **No**.
- **Example Use Case**: Simulating zombies drawn to loud noises.

---

## Usage Notes

- **Distance Parameter**: The `Distance` parameter is measured in meters and defines the range of influence for processors like `FlockAnyGroup` or `AvoidPOIs`. For processors that do not use `Distance` (e.g., `Wind`, `StickToRoads`), the attribute is optional and ignored in the XML configuration.
- **Power Balancing**: Use the `Power` parameter to control the strength of each processor. For example, a `Power` of 0.5 creates a moderate effect, while 2.0 creates a strong effect. Test different values to balance behaviors.
- **Group Interactions**: Combine processors like `FlockSameGroup`, `AlignOtherGroup`, and `AvoidAnyGroup` to create complex group dynamics, such as cohesive groups that avoid rivals.
- **Environmental Dependencies**: Processors like `StickToRoads`, `AvoidRoads`, `StickToPOIs`, and `AvoidPOIs` require map data (roads and POIs) to function. Ensure the simulation environment is properly configured.
- **Event-Based Movement**: The `WorldEvents` processor depends on noise events being present in the simulation. Use this for dynamic, event-driven behavior.

---

## Example XML Configuration

Below is an example XML snippet for configuring movement processors in the `<MovementProcessors>` section of the WalkerSim configuration:

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="1" SpeedScale="1.5" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
    <Processor Type="StickToPOIs" Power="0.5" />
  </ProcessorGroup>
</MovementProcessors>
```