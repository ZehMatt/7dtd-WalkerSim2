using System.Collections.Generic;

namespace WalkerSim.Console
{
    public class CommandWalkerSim : ConsoleCmdAbstract
    {
        private string GetHelpText()
        {
            return "Usage: walkersim <command>.\n" +
                "List of commands:\n" +
                " - show\n" +
                "   Opens a new window with the simulation rendering, works only in singleplayer games.\n" +
                " - pause\n" +
                "   Pauses the simulation which also stops spawning and despawning.\n" +
                " - resume\n" +
                "   Resumes the simulation and also the spawning/despawning.";
        }

        private void ShowHelpText(string error)
        {
            if (error != null && error != "")
            {
                SdtdConsole.Instance.Output("ERROR: " + error);
            }
            SdtdConsole.Instance.Output(GetHelpText());
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count == 0)
            {
                ShowHelpText("No command was specified");
                return;
            }

            string cmd = _params[0].ToLower();
            if (cmd == "show")
            {
                LocalPlayerUI.primaryUI.windowManager.Open("walkersim", true);
            }
            else if (cmd == "pause")
            {
                Simulation.Instance.SetPaused(true);
            }
            else if (cmd == "resume")
            {
                Simulation.Instance.SetPaused(false);
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
