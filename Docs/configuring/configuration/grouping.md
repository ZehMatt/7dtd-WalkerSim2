In WalkerSim, agents can be organized into groups to create diverse behaviors, such as different zombie hordes moving in unique ways. Each group is identified by a **group ID**, a non-negative integer that determines which agents are affected by specific movement processors. The `<ProcessorGroup>` element in the XML configuration allows you to assign movement processors to a group using the `Group` attribute. A special group ID of `-1` is used to apply processors to all groups or distribute them across groups in a specific way.

**Important**: Group IDs are zero-based indices. If the simulation has `N` groups (determined by total agents divided by `GroupSize`), the valid group IDs range from `0` to `N-1`. For example, with 4 groups, the maximum group ID is 3 (IDs: 0, 1, 2, 3).

This document covers:
- How group IDs are assigned and used.
- The behavior of groups with a `Group` value of `-1`.
- How group specifications influence movement processors.

## Understanding Group IDs

Group IDs are integers that identify distinct groups of agents in the simulation. They are used to control which agents are affected by the movement processors defined in a `<ProcessorGroup>`.

- **Assignment**: The number of groups is determined by the total number of agents divided by the `GroupSize` parameter (set in the base parameters). For example, if you have 100 agents and a `GroupSize` of 20, there will be 5 groups (IDs 0 through 4).
- **Usage**: In the XML configuration, the `Group` attribute of a `<ProcessorGroup>` specifies which group the contained processors apply to. For example, `Group="0"` applies processors to agents in group ID 0.
- **Constraints**: Group IDs must be non-negative integers (0 or higher) and cannot exceed `N-1`, where `N` is the total number of groups. If a `Group` value exceeds this limit, a warning is logged, and the processors for that group are ignored.

### Example

```xml
<ProcessorGroup Group="0" SpeedScale="1.0" Color="#FF0000">
  <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
</ProcessorGroup>
```

This assigns the `FlockSameGroup` processor to agents in group ID 0, causing them to cluster together.

## Groups with `Group="-1"`

A `Group` value of `-1` is a special case that indicates the processors should apply to **all groups** or be **distributed across groups**. The simulation handles this by alternating the processors defined for `-1` across all available groups in a round-robin fashion.

- **Behavior**: If you define one or more `<ProcessorGroup>` elements with `Group="-1"`, the simulation assigns these processors to all groups, cycling through the `-1` groups if there are multiple. For example, if you have 5 groups (IDs 0–4) and two `-1` processor groups, the first `-1` group is assigned to group 0, the second to group 1, the first again to group 2, and so on.
- **Purpose**: This allows you to apply a common set of processors to all agents without specifying each group individually, or to create varied behaviors by distributing different processors across groups.
- **Color Handling**: If no `Color` is specified for a `-1` group, a default purple (magenta) color is used for visualization in the simulation viewer.

### Example

```xml
<ProcessorGroup Group="-1" SpeedScale="1.0" Color="#00FF00">
  <Processor Type="WorldEvents" Power="0.8" />
</ProcessorGroup>
```

If there are 5 groups, this processor group applies the `WorldEvents` processor to all groups (0–4), with all agents reacting to noise events and visualized in green.

### Multiple `-1` Groups Example

```xml
<ProcessorGroup Group="-1" SpeedScale="1.0" Color="#00FF00">
  <Processor Type="WorldEvents" Power="0.8" />
</ProcessorGroup>
<ProcessorGroup Group="-1" SpeedScale="1.2" Color="#0000FF">
  <Processor Type="StickToPOIs" Power="0.5" />
</ProcessorGroup>
```

With 5 groups:
- Group 0 gets the first `-1` group (`WorldEvents`, green).
- Group 1 gets the second `-1` group (`StickToPOIs`, blue).
- Group 2 gets the first `-1` group again (`WorldEvents`, green).
- Group 3 gets the second `-1` group (`StickToPOIs`, blue).
- Group 4 gets the first `-1` group (`WorldEvents`, green).

This creates an alternating pattern of behaviors across groups.

## Influencing Movement Processors with Groups

Specifying group IDs in the `<ProcessorGroup>` element allows you to tailor movement behaviors to specific subsets of agents, creating diverse and complex simulation dynamics. The `Group` attribute directly influences how movement processors are applied:

- **Specific Group IDs (e.g., `Group="0"`, `Group="1"`)**:
  - Processors are applied only to agents in the specified group.
  - This is useful for creating distinct behaviors for different groups. For example, one group might flock together while another avoids roads.
  - If a specific group ID is defined, it overrides any `-1` processors for that group, ensuring precise control.

- **Generic Group ID (`Group="-1"`)**:
  - Processors are distributed across all groups, as described above.
  - This is ideal for applying universal behaviors (e.g., all agents react to wind) or creating varied behaviors without specifying every group.

- **Combining Specific and Generic Groups**:
  - You can mix specific and generic groups in the same configuration. Specific groups take precedence, and `-1` groups fill in for any groups not explicitly defined.
  - For example, you can assign unique processors to group 0 and use `-1` to apply a default behavior to all other groups.

- **Impact on Movement Processors**:
  - Processors like `FlockSameGroup`, `AlignSameGroup`, and `AvoidSameGroup` only affect agents within the same group, making group IDs critical for their behavior.
  - Processors like `FlockOtherGroup`, `AlignOtherGroup`, and `AvoidOtherGroup` rely on group IDs to identify "other" groups, enabling interactions between different groups.
  - Processors like `Wind` or `StickToRoads` are group-agnostic but can still be applied to specific groups for targeted effects.

### Example: Specific and Generic Groups

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="-1" SpeedScale="1.2" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

With 3 groups (IDs 0–2):
- Group 0: Uses the specific processors (`FlockSameGroup` and `AvoidRoads`), visualized in red.
- Group 1: Uses the `-1` processor (`WorldEvents`), visualized in green.
- Group 2: Uses the `-1` processor (`WorldEvents`), visualized in green.

This creates a simulation where group 0 agents flock together and avoid roads, while groups 1 and 2 react to noise events.

## Usage Notes

- **Group ID Validation**: Ensure that specific `Group` values (e.g., `Group="5"`) do not exceed the total number of groups (e.g., `N-1`, where `N` is the number of groups). Invalid IDs trigger a warning, and the processors are ignored.
- **Using `-1` for Simplicity**: Use `Group="-1"` when you want a single set of processors to apply to all agents or to alternate behaviors across groups without specifying each one.
- **Combining Behaviors**: Combine specific and generic groups to balance detailed control with broad applicability. For example, use specific groups for key behaviors and `-1` for default behaviors.
- **Group Visualization**: The `Color` attribute in `<ProcessorGroup>` sets the visual color for agents in the simulation viewer. If unspecified, `-1` groups default to purple (magenta), and unassigned groups receive a color from a predefined table.
- **Processor Interactions**: Choose processors that leverage group IDs (e.g., `FlockSameGroup`, `AvoidOtherGroup`) to create dynamic interactions between groups, such as rival hordes or cooperative clusters.
- **Performance**: Be mindful of the number of groups and processors, as large numbers can impact performance, especially with complex behaviors.

## Example XML Configuration

Below is an example XML configuration snippet for the `<MovementProcessors>` section, demonstrating specific and generic group usage:

```xml
<MovementProcessors>
  <ProcessorGroup Group="0" SpeedScale="1.0" Color="#FF0000">
    <Processor Type="FlockSameGroup" Distance="15.0" Power="0.6" />
    <Processor Type="AvoidRoads" Power="0.4" />
  </ProcessorGroup>
  <ProcessorGroup Group="1" SpeedScale="1.2" Color="#0000FF">
    <Processor Type="StickToPOIs" Power="0.5" />
  </ProcessorGroup>
  <ProcessorGroup Group="-1" SpeedScale="1.1" Color="#00FF00">
    <Processor Type="WorldEvents" Power="0.8" />
  </ProcessorGroup>
</MovementProcessors>
```

With 4 groups (IDs 0–3):
- Group 0: Flocks together and avoids roads (red).
- Group 1: Moves toward POIs (blue).
- Group 2: Reacts to world events (green, from `-1`).
- Group 3: Reacts to world events (green, from `-1`).

This setup creates a simulation with two specialized groups and two groups sharing a common behavior.

This guide provides a user-friendly reference for configuring grouping in WalkerSim. For details on other configuration options, refer to the **Base Parameters Documentation** or **Movement Processors Documentation**.