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
                Name = "show",
                Description = "Opens a new window with the simulation rendering, works only in singleplayer games.",
                Handler = new Action<CommandSenderInfo>((sender) =>
                {
                    LocalPlayerUI.primaryUI.windowManager.Open("walkersim", true);
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

                    SdtdConsole.Instance.Output("--- Simulation Statistics ---");
                    SdtdConsole.Instance.Output("  World Size: {0}", sim.WorldSize);
                    SdtdConsole.Instance.Output("  Ticks: {0}", sim.Ticks);
                    SdtdConsole.Instance.Output("  Active: {0}", sim.Running);
                    SdtdConsole.Instance.Output("  Paused: {0}", sim.Paused);
                    SdtdConsole.Instance.Output("  Players: {0}", sim.PlayerCount);
                    SdtdConsole.Instance.Output("  Total Agents: {0}", sim.Agents.Count);
                    SdtdConsole.Instance.Output("  Total Groups: {0}", sim.GroupCount);
                    SdtdConsole.Instance.Output("  Successful Spawns: {0}", sim.SuccessfulSpawns);
                    SdtdConsole.Instance.Output("  Failed Spawns: {0}", sim.FailedSpawns);
                    SdtdConsole.Instance.Output("  Total Despawns: {0}", sim.TotalDespawns);
                    SdtdConsole.Instance.Output("  Active Agents: {0}", sim.ActiveCount);
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
            var res = "Usage: walkersim <command>.\n" +
                "List of commands:\n";

            foreach (var cmd in SubCommand.Commands)
            {
                res += string.Format("  {0} - {1}\n",
                    cmd.Name,
                    cmd.Description);
            }

            return res;
        }

        private void ShowHelpText(string error)
        {
            if (error != null && error != "")
            {
                SdtdConsole.Instance.Output("ERROR: " + error);
            }
            SdtdConsole.Instance.Output(GetHelpText());
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
