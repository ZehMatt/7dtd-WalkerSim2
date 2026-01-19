using System;
using System.Collections.Generic;

namespace WalkerSim.Console
{
    class SubCommand
    {
        public string Name;
        public string Description;
        public Delegate Handler;

        public static SubCommand[] Commands = new[] {
            new SubCommand
            {
                Name = "help",
                Description = "Shows this help message.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    ShowHelpTextDirectly();
                }),
            },
            new SubCommand
            {
                Name = "show",
                Description = "Opens the map window and temporarily enables the overlay. To keep the overlay enabled use `walkersim map enable`.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    MapDrawing.IsTemporarilyEnabled = true;

                    EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                    if (primaryPlayer != null)
                    {
                        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
                        XUiC_WindowSelector.OpenSelectorAndWindow(uiforPlayer.entityPlayer, "map");
                    }
                }),
            },
            new SubCommand
            {
                Name = "map",
                Description = "Enables or disables the overlay in the map window, the argument is `enable` or `disable` or a boolean.",
                Handler = new Action<CommandSenderInfo, string>((sender, option) =>
                {
                    if(option.ToLowerInvariant() == "enable" || option == "1" || option.ToLowerInvariant() == "true")
                    {
                        MapDrawing.IsEnabled = true;
                        SdtdConsole.Instance.Output("Overlay for map window enabled.");
                    }
                    else
                    {
                        MapDrawing.IsEnabled = false;
                        SdtdConsole.Instance.Output("Overlay for map window disabled.");
                    }
                }),
            },
            new SubCommand
            {
                Name = "pause",
                Description = "Pauses the simulation which also stops spawning and despawning.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    Simulation.Instance.SetPaused(true);
                }),
            },
            new SubCommand
            {
                Name = "resume",
                Description = "Resumes the simulation and also the spawning/despawning.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    Simulation.Instance.SetPaused(false);
                }),
            },
            new SubCommand
            {
                Name = "restart",
                Description = "Reloads the configuration and restarts the simulation.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    WalkerSimMod.RestartSimulation();
                }),
            },
            new SubCommand
            {
                Name = "stats",
                Description = "Print out some statistics from the simulation.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    var sim = Simulation.Instance;
                    var numDead = sim.NumAgentsDead;
                    var numTotal = sim.Agents.Count;
                    var numAlive = numTotal - numDead;
                    var secsElapsed = sim.GetSimulationTimeSeconds();
                    var timeSpan = TimeSpan.FromSeconds(secsElapsed);

                    SdtdConsole.Instance.Output("--- Simulation Statistics ---");
                    SdtdConsole.Instance.Output("  World Size: {0}", sim.WorldSize);
                    SdtdConsole.Instance.Output("  Ticks: {0}", sim.Ticks);
                    SdtdConsole.Instance.Output("  Simulation Time: {0}", string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds));
                    SdtdConsole.Instance.Output("  Active: {0}", sim.Running);
                    SdtdConsole.Instance.Output("  Paused: {0}", sim.Paused);
                    SdtdConsole.Instance.Output("  Players: {0}", sim.PlayerCount);
                    SdtdConsole.Instance.Output("  Total Agents: {0}", numTotal);
                    SdtdConsole.Instance.Output("  Alive Agents: {0} - {1}%", numAlive, (numAlive / (float)numTotal) * 100.0f);
                    SdtdConsole.Instance.Output("  Dead Agents: {0} - {1}%", numDead, (numDead / (float)numTotal) * 100.0f);
                    SdtdConsole.Instance.Output("  Active Agents: {0}", sim.ActiveCount);
                    SdtdConsole.Instance.Output("  Total Groups: {0}", sim.GroupCount);
                    SdtdConsole.Instance.Output("  Successful Spawns: {0}", sim.SuccessfulSpawns);
                    SdtdConsole.Instance.Output("  Failed Spawns: {0}", sim.FailedSpawns);
                    SdtdConsole.Instance.Output("  Total Despawns: {0}", sim.TotalDespawns);
                    SdtdConsole.Instance.Output("  Bloodmoon: {0}", sim.IsBloodmoon);
                    SdtdConsole.Instance.Output("  DayTime: {0}", sim.IsDayTime);
                    SdtdConsole.Instance.Output("  Time Scale: {0}", sim.TimeScale);
                    SdtdConsole.Instance.Output("  Average Tick Time: {0}, {1}/s", sim.AverageSimTime, 1.0f / sim.AverageSimTime);
                    SdtdConsole.Instance.Output("  Wind Direction: {0}", sim.WindDirection);
                    SdtdConsole.Instance.Output("  Wind Target: {0}", sim.WindDirectionTarget);
                    SdtdConsole.Instance.Output("  Next Wind Change: {0}", sim.TickNextWindChange);
                }),
            },
            new SubCommand
            {
                Name = "timescale",
                Description = "Sets the timescale of the simulation, can be used to speed it up or slow it down.",
                Handler = new Action<CommandSenderInfo, float>((sender, timeScale) =>
                {
                    Simulation.Instance.TimeScale = timeScale;
                }),
            },
            new SubCommand
            {
                Name = "config",
                Description = "Dumps the currently loaded configuration.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    var config = Simulation.Instance.Config;
                    if (config == null)
                    {
                        SdtdConsole.Instance.Output("No configuration loaded.");
                        return;
                    }

                    SdtdConsole.Instance.Output("--- Configuration ---");
                    SdtdConsole.Instance.Output("  Random Seed: {0}", config.RandomSeed);
                    SdtdConsole.Instance.Output("  Population Density: {0} agents/km²", config.PopulationDensity);
                    SdtdConsole.Instance.Output("  Group Size: {0}", config.GroupSize);
                    SdtdConsole.Instance.Output("  Fast Forward At Start: {0}", config.FastForwardAtStart);
                    SdtdConsole.Instance.Output("  Start Agents Grouped: {0}", config.StartAgentsGrouped);
                    SdtdConsole.Instance.Output("  Start Position: {0}", config.StartPosition);
                    SdtdConsole.Instance.Output("  Respawn Position: {0}", config.RespawnPosition);
                    SdtdConsole.Instance.Output("  Pause During Bloodmoon: {0}", config.PauseDuringBloodmoon);
                    SdtdConsole.Instance.Output("  Sound Distance Scale: {0}", config.SoundDistanceScale);
                    SdtdConsole.Instance.Output("");
                    SdtdConsole.Instance.Output("--- Processor Groups ({0}) ---", config.Processors.Count);
                    for (int i = 0; i < config.Processors.Count; i++)
                    {
                        var group = config.Processors[i];
                        SdtdConsole.Instance.Output("  Group {0}:", group.Group == -1 ? "Any" : group.Group.ToString());
                        SdtdConsole.Instance.Output("    Speed Scale: {0}", group.SpeedScale);
                        SdtdConsole.Instance.Output("    Post Spawn Behavior: {0}", group.PostSpawnBehavior);
                        SdtdConsole.Instance.Output("    Post Spawn Wander Speed: {0}", group.PostSpawnWanderSpeed);
                        SdtdConsole.Instance.Output("    Color: {0}", group.Color);
                        SdtdConsole.Instance.Output("    Processors ({0}):", group.Entries.Count);
                        foreach (var processor in group.Entries)
                        {
                            if (processor.Distance > 0)
                                SdtdConsole.Instance.Output("      - {0} (Distance: {1}, Power: {2})", processor.Type, processor.Distance, processor.Power);
                            else
                                SdtdConsole.Instance.Output("      - {0} (Power: {1})", processor.Type, processor.Power);
                        }
                    }
                }),
            },
            new SubCommand
            {
                Name = "maskinfo",
                Description = "",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    EntityPlayer player = null;
                    if(sender.RemoteClientInfo == null)
                    {
                        player = GameManager.Instance.World.GetPrimaryPlayer() as EntityPlayer;
                    }
                    else
                    {
                        var entityId = sender.RemoteClientInfo.entityId;
                        player = GameManager.Instance.World.GetEntity(entityId) as EntityPlayer;
                    }

                    if (player == null)
                    {
                        SdtdConsole.Instance.Output("No player found for this command.");
                        return;
                    }

                    var mapData = Simulation.Instance.MapData;
                    var spawnGroups = mapData.SpawnGroups;

                    var pos = VectorUtils.ToSim(player.position);
                    var worldMins = mapData.WorldMins;
                    var worldMaxs = mapData.WorldMaxs;
                    var worldSize = mapData.WorldSize;
                    var x = (int)MathEx.Remap(pos.X, worldMins.X, worldMaxs.X, 0f, worldSize.X);
                    var y = (int)worldSize.Y - (int)MathEx.Remap(pos.Y, worldMins.Y, worldMaxs.Y, 0f, worldSize.Y);

                    // Get the spawn group mask for this position.
                    var spawnGroup = spawnGroups.GetSpawnGroup(x, y);
                    if (spawnGroup == null)
                    {
                        SdtdConsole.Instance.Output("No spawn group found at position {0}, {1} ({2}).", x, y, pos);
                    }

                    SdtdConsole.Instance.Output("Spawn group at position {0}, {1} ({2}): {3} - {4}, {5}",
                        x, y, pos,
                        spawnGroup.EntityGroupDay, spawnGroup.EntityGroupNight, spawnGroup.ColorString);
                }),
            }
        };

        public static void ShowHelpTextDirectly()
        {
            SdtdConsole.Instance.Output("=== WalkerSim Commands ===");
            SdtdConsole.Instance.Output("");
            SdtdConsole.Instance.Output("Usage: walkersim <command> [arguments]");
            SdtdConsole.Instance.Output("");

            // Find the longest command name for alignment
            int maxNameLength = 0;
            foreach (var cmd in Commands)
            {
                if (cmd.Name.Length > maxNameLength)
                    maxNameLength = cmd.Name.Length;
            }

            SdtdConsole.Instance.Output("Available Commands:");
            foreach (var cmd in Commands)
            {
                // Get parameter info
                var parameters = cmd.Handler.Method.GetParameters();
                bool requiresSenderInfo = parameters.Length > 0 && parameters[0].ParameterType == typeof(CommandSenderInfo);
                int startIndex = requiresSenderInfo ? 1 : 0;

                string paramString = "";
                for (int i = startIndex; i < parameters.Length; i++)
                {
                    paramString += $" <{parameters[i].Name}>";
                }

                string cmdLine = cmd.Name + paramString;
                int padding = maxNameLength + 20 - cmdLine.Length;
                if (padding < 2)
                    padding = 2;

                SdtdConsole.Instance.Output("  {0}{1}{2}",
                    cmdLine,
                    new string(' ', padding),
                    cmd.Description);
            }
            SdtdConsole.Instance.Output("");
            SdtdConsole.Instance.Output("Examples:");
            SdtdConsole.Instance.Output("  walkersim stats");
            SdtdConsole.Instance.Output("  walkersim map enable");
            SdtdConsole.Instance.Output("  walkersim timescale 2.0");
        }
    }

    public class CommandWalkerSim : ConsoleCmdAbstract
    {
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count == 0)
            {
                ShowHelpText("No command was specified.");
                return;
            }

            var command = _params[0].ToLowerInvariant();
            _params.RemoveAt(0);

            // Special handling for help command
            if (command == "help")
            {
                SubCommand.ShowHelpTextDirectly();
                return;
            }

            SubCommand subCommand = null;
            for (int i = 0; i < SubCommand.Commands.Length; i++)
            {
                if (SubCommand.Commands[i].Name == command)
                {
                    subCommand = SubCommand.Commands[i];
                    break;
                }
            }

            if (subCommand == null)
            {
                ShowHelpText($"Unknown command: {command}");
                return;
            }

            // Get parameter info from the handler
            var parameters = subCommand.Handler.Method.GetParameters();

            // Determine if the handler expects CommandSenderInfo as the first parameter
            bool requiresSenderInfo = parameters.Length > 0 && parameters[0].ParameterType == typeof(CommandSenderInfo);
            int expectedParamCount = requiresSenderInfo ? parameters.Length - 1 : parameters.Length;

            // Validate parameter count
            if (_params.Count != expectedParamCount)
            {
                string usage = command;
                int startIndex = requiresSenderInfo ? 1 : 0;
                for (int i = startIndex; i < parameters.Length; i++)
                {
                    usage += $" <{parameters[i].ParameterType.Name}>";
                }
                ShowHelpText($"Invalid number of parameters. Expected {expectedParamCount}, got {_params.Count}. Usage: {usage}");
                return;
            }

            // Prepare arguments for invocation
            object[] invokeArgs = new object[parameters.Length];
            if (requiresSenderInfo)
            {
                invokeArgs[0] = _senderInfo;
            }

            // Parse user-provided parameters
            int paramStart = requiresSenderInfo ? 1 : 0;
            for (int i = paramStart; i < parameters.Length; i++)
            {
                var param = parameters[i];
                try
                {
                    invokeArgs[i] = Convert.ChangeType(_params[i - paramStart], param.ParameterType);
                }
                catch (Exception ex)
                {
                    ShowHelpText($"Failed to parse parameter '{param.Name}' as {param.ParameterType.Name}: {ex.Message}");
                    return;
                }
            }

            try
            {
                subCommand.Handler.DynamicInvoke(invokeArgs);
            }
            catch (Exception ex)
            {
                ShowHelpText($"Error executing command: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private string GetHelpText()
        {
            return "WalkerSim - Zombie simulation mod. Use 'walkersim help' for available commands.";
        }

        private void ShowHelpText(string error)
        {
            if (error != null && error != "")
            {
                SdtdConsole.Instance.Output("[ERROR] " + error);
                SdtdConsole.Instance.Output("");
            }
            SubCommand.ShowHelpTextDirectly();
        }

        public override string[] getCommands()
        {
            return new string[] { "walkersim" };
        }

        public override string getDescription()
        {
            return GetHelpText();
        }
    }
}
