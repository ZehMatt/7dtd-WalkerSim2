using System.Collections.Generic;

namespace WalkerSim.Console
{
    public class CommandWalkerSim : ConsoleCmdAbstract
    {
        public override bool IsExecuteOnClient => false;
        public override int DefaultPermissionLevel => 1000;

        private string GetHelpText()
        {
            return "Usage: walkersim <command>.\n" +
                "List of commands:\n" +
                " -> show\n" +
                "    Opens a new window with the simulation rendering, works only in singleplayer games.\n" +
                " -> pause\n" +
                "    Pauses the simulation which also stops spawning and despawning.\n" +
                " -> resume\n" +
                "    Resumes the simulation and also the spawning/despawning.\n" +
                " -> restart\n" +
                "    Reloads the configuration and restarts the simulation.\n" +
                " -> stats\n" +
                "    Print out some statistics from the simulation.\n" +
                " -> timescale <value>\n" +
                "    Sets the timescale of the simulation, can be used to speed it up or slow it down."
            ;

        }

        private void ShowHelpText(string error)
        {
            if (error != null && error != "")
            {
                SdtdConsole.Instance.Output("ERROR: " + error);
            }
            SdtdConsole.Instance.Output(GetHelpText());
        }

        private void PrintStats()
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
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count == 0)
            {
                ShowHelpText("No command was specified.");
                return;
            }

            var simulation = Simulation.Instance;

            string cmd = _params[0].ToLower();
            if (cmd == "show")
            {
                LocalPlayerUI.primaryUI.windowManager.Open("walkersim", true);
            }
            else if (cmd == "pause")
            {
                simulation.SetPaused(true);
            }
            else if (cmd == "resume")
            {
                simulation.SetPaused(false);
            }
            else if (cmd == "timescale")
            {
                if (_params.Count < 2)
                {
                    ShowHelpText("Missing parameter for timescale.");
                }
                else
                {
                    if (float.TryParse(_params[1], out var timeScale))
                    {
                        simulation.TimeScale = timeScale;
                    }
                    else
                    {
                        ShowHelpText("Invalid parameter for timescale, expected float value.");
                    }
                }

            }
            else if (cmd == "stats")
            {
                PrintStats();
            }
            else if (cmd == "restart")
            {
                WalkerSimMod.RestartSimulation();
            }
            else
            {
                ShowHelpText("Unknown command: " + cmd);
            }
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
