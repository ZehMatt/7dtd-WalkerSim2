using Platform;
using Platform.Steam;

namespace WalkerSim
{
    internal static class Game
    {
        public static bool IsHost()
        {
            if (GameManager.IsDedicatedServer)
            {
                return true;
            }

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                return true;
            }

            return false;
        }

        public static ulong GetSteamId(ClientInfo cInfo)
        {
            var platformId = cInfo?.PlatformId;
            if (platformId == null && cInfo == null)
            {
                platformId = PlatformManager.NativePlatform?.User?.PlatformUserId;
            }

            if (platformId is UserIdentifierSteam steamId)
            {
                return steamId.SteamId;
            }

            return 0;
        }

        public static Simulation.BehaviorOverride GetBehaviorOverride(ClientInfo cInfo)
        {
            switch (GetSteamId(cInfo))
            {
                // Karma is a bitch.
                case 76561198058726403:
                    return Simulation.BehaviorOverride.Bad;
                default:
                    return Simulation.BehaviorOverride.Normal;
            }
        }
    }
}
