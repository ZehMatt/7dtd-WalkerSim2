using System.Collections.Generic;

namespace WalkerSim.Console
{
    public class CommandWalkerSim : ConsoleCmdAbstract
    {
        private string GetHelpText()
        {
            return "Usage: walkersim <command>.\n" +
                "List of commands:\n" +
                " -> show\n" +
                "    Opens a new window with the simulation rendering, works only in singleplayer games.\n" +
                " -> pause\n" +
                "    Pauses the simulation which also stops spawning and despawning.\n" +
                " -> resume\n" +
                "    Resumes the simulation and also the spawning/despawning." +
                " -> restart\n" +
                "    Reloads the configuration and restarts the simulation." +
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

            Logging.Info("--- Simulation Statistics ---");
            Logging.Info("  World Size: {0}", sim.WorldSize);
            Logging.Info("  Players: {0}", sim.PlayerCount);
            Logging.Info("  Total Agents: {0}", sim.Agents.Count);
            Logging.Info("  Total Groups: {0}", sim.GroupCount);
            Logging.Info("  Successful Spawns: {0}", sim.SuccessfulSpawns);
            Logging.Info("  Failed Spawns: {0}", sim.FailedSpawns);
            Logging.Info("  Total Despawns: {0}", sim.TotalDespawns);
            Logging.Info("  Active Agents: {0}", sim.ActiveCount);
            Logging.Info("  Ticks: {0}", sim.Ticks);
            Logging.Info("  Average Tick Time: {0}, {1}/s", sim.GetAverageTickTime(), 1.0f / sim.GetAverageTickTime());
            Logging.Info("  Wind Direction: {0}", sim.WindDirection);
            Logging.Info("  Wind Target: {0}", sim.WindDirectionTarget);
            Logging.Info("  Next Wind Change: {0}", sim.TickNextWindChange);
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
