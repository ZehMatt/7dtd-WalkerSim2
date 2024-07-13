using System.Collections.Generic;

namespace WalkerSim
{
    internal static class SoundInfo
    {
        public class Info
        {
            public float Radius;
            public float DecayRate;
        }

        private static Dictionary<string, Info> _sounds = new Dictionary<string, Info>();

        static SoundInfo()
        {
            CreateDefaults();
        }

        public static void SetSoundInfo(string name, float radius, float decayRate)
        {
            _sounds[name] = new Info
            {
                Radius = radius,
                DecayRate = decayRate
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
            SetSoundInfo("explosion_grenade", 700, 1.0f);
            SetSoundInfo("explosion1", 700, 1.0f);
            // Normal weapon fire.
            SetSoundInfo("m136_fire", 300, 1.0f);
            SetSoundInfo("pistol_fire", 200, 1.0f);
            SetSoundInfo("mp5_fire", 250, 1.0f);
            SetSoundInfo("blunderbuss_fire", 350, 1.0f);
            SetSoundInfo("autoshotgun_fire", 350, 1.0f);
            SetSoundInfo("pump_shotgun_fire", 350, 1.0f);
            SetSoundInfo("shotgundb_fire", 350, 1.0f);
            SetSoundInfo("44magnum_fire", 450, 1.0f);
            SetSoundInfo("desertvulture_fire", 450, 1.0f);
            SetSoundInfo("tacticalar_fire", 350, 1.0f);
            SetSoundInfo("ak47_fire", 400, 1.0f);
            SetSoundInfo("sniperrifle_fire", 500, 1.0f);
            SetSoundInfo("m60_fire", 500, 1.0f);
            SetSoundInfo("sharpshooter_fire", 700, 1.0f);
            // Silenced weapon fire.
            SetSoundInfo("ak47_s_fire", 150, 0.5f);
            SetSoundInfo("pistol_s_fire", 100, 0.5f);
            SetSoundInfo("sniperrifle_s_fire", 250, 0.5f);
            SetSoundInfo("mp5_s_fire", 150, 0.5f);
            SetSoundInfo("pump_shotgun_s_fire", 250, 0.5f);
            SetSoundInfo("hunting_rifle_s_fire", 250, 0.5f);
        }

        // TODO: Load this from the xml.
    }
}
