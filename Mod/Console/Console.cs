using System;
using System.Collections.Generic;

namespace WalkerSim.Console
{
    class SubCommand
    {
        public string Name;
        public string Description;
        public bool RequiresAdmin;
        public Delegate Handler;

        public static SubCommand[] Commands = new[] {
            new SubCommand
            {
                Name = "show",
                Description = "Opens a new window with the simulation rendering, works only in singleplayer games.",
                RequiresAdmin = false,
                Handler = new Action(() =>
                {
                    LocalPlayerUI.primaryUI.windowManager.Open("walkersim", true);
                }),
            },
            new SubCommand
            {
                Name = "pause",
                Description = "Pauses the simulation which also stops spawning and despawning.",
                RequiresAdmin = true,
                Handler = new Action(() =>
                {
                    Simulation.Instance.SetPaused(true);
                }),
            },
            new SubCommand
            {
                Name = "resume",
                Description = "Resumes the simulation and also the spawning/despawning.",
                RequiresAdmin = true,
                Handler = new Action(() =>
                {
                    Simulation.Instance.SetPaused(false);
                }),
            },
            new SubCommand
            {
                Name = "restart",
                Description = "Reloads the configuration and restarts the simulation.",
                RequiresAdmin = true,
                Handler = new Action(() =>
                {
                    WalkerSimMod.RestartSimulation();
                }),
            },
            new SubCommand
            {
                Name = "stats",
                Description = "Print out some statistics from the simulation.",
                RequiresAdmin = false,
                Handler = new Action(() =>
                {
                    var sim = Simulation.Instance;

                    SdtdConsole.Instance.Output("--- Simulation Statistics ---");
                    SdtdConsole.Instance.Output("  World Size: {0}", sim.WorldSize);
                    SdtdConsole.Instance.Output("  Players: {0}", sim.PlayerCount);
                    SdtdConsole.Instance.Output("  Total Agents: {0}", sim.Agents.Count);
                    SdtdConsole.Instance.Output("  Total Groups: {0}", sim.GroupCount);
                    SdtdConsole.Instance.Output("  Successful Spawns: {0}", sim.SuccessfulSpawns);
                    SdtdConsole.Instance.Output("  Failed Spawns: {0}", sim.FailedSpawns);
                    SdtdConsole.Instance.Output("  Total Despawns: {0}", sim.TotalDespawns);
                    SdtdConsole.Instance.Output("  Active Agents: {0}", sim.ActiveCount);
                    SdtdConsole.Instance.Output("  Ticks: {0}", sim.Ticks);
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
                RequiresAdmin = true,
                Handler = new Action<float>(timeScale =>
                {
                    Simulation.Instance.TimeScale = timeScale;
                }),
            },
        };
    }

    public class CommandWalkerSim : ConsoleCmdAbstract
    {
        public override bool IsExecuteOnClient => false;

        public override int DefaultPermissionLevel => 1000;

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

            var userIsAdmin = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_senderInfo.RemoteClientInfo) >= 1000;
            if (subCommand.RequiresAdmin && !userIsAdmin)
            {
                ShowHelpText("You do not have permission to execute this command.");
                return;
            }

            // Get parameter info from the handler
            var parameters = subCommand.Handler.Method.GetParameters();

            // Validate parameter count
            if (_params.Count != parameters.Length)
            {
                string usage = command;
                for (int i = 0; i < parameters.Length; i++)
                {
                    usage += $" <{parameters[i].ParameterType.Name}>";
                }
                ShowHelpText($"Invalid number of parameters. Expected {parameters.Length}, got {_params.Count}. Usage: {usage}");
                return;
            }

            // Parse parameters
            object[] parsedArgs = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                try
                {
                    parsedArgs[i] = Convert.ChangeType(_params[i], param.ParameterType);
                }
                catch (Exception ex)
                {
                    ShowHelpText($"Failed to parse parameter '{param.Name}' as {param.ParameterType.Name}: {ex.Message}");
                    return;
                }
            }

            try
            {
                subCommand.Handler.DynamicInvoke(parsedArgs);
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
                res += string.Format("  {0} - {1}{2}\n",
                    cmd.Name,
                    cmd.RequiresAdmin ? "[Admin] " : "",
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
