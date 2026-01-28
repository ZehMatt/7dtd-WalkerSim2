using System;
using System.Collections.Generic;

namespace WalkerSim.Console
{
    static class ConsoleOutput
    {
        public static void Log(string fmt, params object[] args)
        {
            var formatted = string.Format(fmt, args);
            SdtdConsole.Instance.Output("{0}", formatted);
            LogFileSink.Instance.Message(Logging.Level.Info, formatted);
        }
    }

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
                        ConsoleOutput.Log("Overlay for map window enabled.");
                    }
                    else
                    {
                        MapDrawing.IsEnabled = false;
                        ConsoleOutput.Log("Overlay for map window disabled.");
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
                    var averageSimTime = sim.AverageSimTime * 1000.0f;
                    var ticksPerSecond = sim.AverageSimTime > 0 ? 1 / sim.AverageSimTime : 0;

                    ConsoleOutput.Log("--- Simulation Statistics ---");
                    ConsoleOutput.Log("  World Size: {0}", sim.WorldSize);
                    ConsoleOutput.Log("  Ticks: {0}", sim.Ticks);
                    ConsoleOutput.Log("  Unscaled Ticks: {0}", sim.UnscaledTicks);
                    ConsoleOutput.Log("  Simulation Time: {0}", string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds));
                    ConsoleOutput.Log("  Active: {0}", sim.Running);
                    ConsoleOutput.Log("  Paused: {0}", sim.Paused);
                    ConsoleOutput.Log("  Players: {0}", sim.PlayerCount);
                    ConsoleOutput.Log("  Total Agents: {0}", numTotal);
                    ConsoleOutput.Log("  Alive Agents: {0} - {1}%", numAlive, (numAlive / (float)numTotal) * 100.0f);
                    ConsoleOutput.Log("  Dead Agents: {0} - {1}%", numDead, (numDead / (float)numTotal) * 100.0f);
                    ConsoleOutput.Log("  Active Agents: {0}", sim.ActiveCount);
                    ConsoleOutput.Log("  Total Groups: {0}", sim.GroupCount);
                    ConsoleOutput.Log("  Successful Spawns: {0}", sim.SuccessfulSpawns);
                    ConsoleOutput.Log("  Failed Spawns: {0}", sim.FailedSpawns);
                    ConsoleOutput.Log("  Total Despawns: {0}", sim.TotalDespawns);
                    ConsoleOutput.Log("  Bloodmoon: {0}", sim.IsBloodmoon);
                    ConsoleOutput.Log("  DayTime: {0}", sim.IsDayTime);
                    ConsoleOutput.Log("  Time Scale: {0}", sim.TimeScale);
                    ConsoleOutput.Log("  Average Tick Time: {0}ms, {1}/ps", averageSimTime, ticksPerSecond);
                    ConsoleOutput.Log("  Wind Direction: {0}", sim.WindDirection);
                    ConsoleOutput.Log("  Wind Target: {0}", sim.WindDirectionTarget);
                    ConsoleOutput.Log("  Next Wind Change: {0}", sim.TickNextWindChange);
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
                        ConsoleOutput.Log("No configuration loaded.");
                        return;
                    }

                    ConsoleOutput.Log("--- Configuration ---");
                    ConsoleOutput.Log("  Random Seed: {0}", config.RandomSeed);
                    ConsoleOutput.Log("  Population Density: {0} agents/km²", config.PopulationDensity);
                    ConsoleOutput.Log("  Group Size: {0}", config.GroupSize);
                    ConsoleOutput.Log("  Fast Forward At Start: {0}", config.FastForwardAtStart);
                    ConsoleOutput.Log("  Start Agents Grouped: {0}", config.StartAgentsGrouped);
                    ConsoleOutput.Log("  Start Position: {0}", config.StartPosition);
                    ConsoleOutput.Log("  Respawn Position: {0}", config.RespawnPosition);
                    ConsoleOutput.Log("  Pause During Bloodmoon: {0}", config.PauseDuringBloodmoon);
                    ConsoleOutput.Log("  Sound Distance Scale: {0}", config.SoundDistanceScale);
                    ConsoleOutput.Log("");
                    ConsoleOutput.Log("--- Processor Groups ({0}) ---", config.Processors.Count);
                    for (int i = 0; i < config.Processors.Count; i++)
                    {
                        var group = config.Processors[i];
                        ConsoleOutput.Log("  Group {0}:", group.Group == -1 ? "Any" : group.Group.ToString());
                        ConsoleOutput.Log("    Speed Scale: {0}", group.SpeedScale);
                        ConsoleOutput.Log("    Post Spawn Behavior: {0}", group.PostSpawnBehavior);
                        ConsoleOutput.Log("    Post Spawn Wander Speed: {0}", group.PostSpawnWanderSpeed);
                        ConsoleOutput.Log("    Color: {0}", group.Color);
                        ConsoleOutput.Log("    Processors ({0}):", group.Entries.Count);
                        foreach (var processor in group.Entries)
                        {
                            if (processor.Distance > 0)
                                ConsoleOutput.Log("      - {0} (Distance: {1}, Power: {2})", processor.Type, processor.Distance, processor.Power);
                            else
                                ConsoleOutput.Log("      - {0} (Power: {1})", processor.Type, processor.Power);
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
                        ConsoleOutput.Log("No player found for this command.");
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
                        ConsoleOutput.Log("No spawn group found at position {0}, {1} ({2}).", x, y, pos);
                    }

                    ConsoleOutput.Log("Spawn group at position {0}, {1} ({2}): {3} - {4}, {5}",
                        x, y, pos,
                        spawnGroup.EntityGroupDay, spawnGroup.EntityGroupNight, spawnGroup.ColorString);
                }),
            },
            new SubCommand
            {
                Name = "jonah",
                Description = "Find out what this is",
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

                    if(player != null)
                    {
                        if(Simulation.Instance.EnableZombieRain(player.entityId))
                        {
                            ConsoleOutput.Log("Jonah mode now active.");
                        }
                        else
                        {
                            ConsoleOutput.Log("Jonah mode now inactive.");
                        }

                    }
                }),
            }
        };

        public static void ShowHelpTextDirectly()
        {
            ConsoleOutput.Log("=== WalkerSim Commands ===");
            ConsoleOutput.Log("");
            ConsoleOutput.Log("Usage: walkersim <command> [arguments]");
            ConsoleOutput.Log("");

            // Find the longest command name for alignment
            int maxNameLength = 0;
            foreach (var cmd in Commands)
            {
                if (cmd.Name.Length > maxNameLength)
                    maxNameLength = cmd.Name.Length;
            }

            ConsoleOutput.Log("Available Commands:");
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

                ConsoleOutput.Log("  {0}{1}{2}",
                    cmdLine,
                    new string(' ', padding),
                    cmd.Description);
            }
            ConsoleOutput.Log("");
            ConsoleOutput.Log("Examples:");
            ConsoleOutput.Log("  walkersim stats");
            ConsoleOutput.Log("  walkersim map enable");
            ConsoleOutput.Log("  walkersim timescale 2.0");
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
                ConsoleOutput.Log("[ERROR] " + error);
                ConsoleOutput.Log("");
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
