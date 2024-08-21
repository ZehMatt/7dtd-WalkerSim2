using System.Collections.Generic;

namespace WalkerSim
{
    internal static class SoundInfo
    {
        public class Info
        {
            public float Radius;
        }

        private static Dictionary<string, Info> _sounds = new Dictionary<string, Info>();

        static SoundInfo()
        {
            CreateDefaults();
        }

        public static void SetSoundInfo(string name, float radius)
        {
            _sounds[name] = new Info
            {
                Radius = radius,
            };
        }

        public static Info GetSoundInfo(string name)
        {
            if (_sounds.TryGetValue(name, out var info))
            {
                return info;
            }

            return null;
        }

        private static void CreateDefaults()
        {
            // Explosions.
            SetSoundInfo("explosion_grenade", 700);
            SetSoundInfo("explosion1", 700);
            // Normal weapon fire.
            SetSoundInfo("m136_fire", 300);
            SetSoundInfo("pistol_fire", 200);
            SetSoundInfo("mp5_fire", 250);
            SetSoundInfo("blunderbuss_fire", 350);
            SetSoundInfo("autoshotgun_fire", 350);
            SetSoundInfo("pump_shotgun_fire", 350);
            SetSoundInfo("shotgundb_fire", 350);
            SetSoundInfo("44magnum_fire", 450);
            SetSoundInfo("desertvulture_fire", 450);
            SetSoundInfo("tacticalar_fire", 350);
            SetSoundInfo("ak47_fire", 400);
            SetSoundInfo("sniperrifle_fire", 500);
            SetSoundInfo("m60_fire", 500);
            SetSoundInfo("sharpshooter_fire", 700);
            // Silenced weapon fire.
            SetSoundInfo("ak47_s_fire", 150);
            SetSoundInfo("pistol_s_fire", 100);
            SetSoundInfo("sniperrifle_s_fire", 250);
            SetSoundInfo("mp5_s_fire", 150);
            SetSoundInfo("pump_shotgun_s_fire", 250);
            SetSoundInfo("hunting_rifle_s_fire", 250);
        }

        // TODO: Load this from the xml.
    }
}
