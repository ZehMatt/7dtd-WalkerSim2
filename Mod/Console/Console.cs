using System.Collections.Generic;

namespace WalkerSim.Console
{
    public class CommandWalkerSim : ConsoleCmdAbstract
    {
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count < 1)
                return;

            string cmd = _params[0];
            if (cmd.ToLower() == "show")
            {
                Log.Out("Showing the window");

                LocalPlayerUI.primaryUI.windowManager.Open("walkersim", true);
            }
        }

        public override string[] getCommands()
        {
            return new string[] { "walkersim" };
        }

        public override string getDescription()
        {
            return "";
        }
    }
}
