namespace WalkerSim
{
    internal static class Game
    {
        public static bool IsHost()
        {
            if (GameManager.IsDedicatedServer)
                return true;

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                return true;

            return false;
        }
    }
}
