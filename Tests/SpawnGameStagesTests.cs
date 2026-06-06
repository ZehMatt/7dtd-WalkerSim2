using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WalkerSim.Tests
{
    [TestClass]
    public class SpawnGameStagesTests
    {
        // Mirrors how the game derives BiomeSpawnEntityGroupData.idHash from the <spawn> id.
        static int Hash(string id) => id.GetHashCode();

        const string Spawning = @"
<spawning>
  <biome name='pine_forest'>
    <spawn id='f_early' time='Night' maxcount='1' entitygroup='ZombiesAll'   maxgs='32'/>
    <spawn id='f_mid'   time='Night' maxcount='1' entitygroup='ZombiesNight' mings='33' maxgs='65'/>
    <spawn id='f_late'  time='Night' maxcount='1' entitygroup='ZombiesFeral' mings='99'/>
    <spawn id='f_plain' time='Day'   maxcount='1' entitygroup='ZombiesAll'/>
    <spawn id='f_v1'    time='Night' maxcount='1' entitygroup='ZombiesAll' tags='downtown'    mings='10' maxgs='20'/>
    <spawn id='f_v2'    time='Night' maxcount='1' entitygroup='ZombiesAll' tags='wilderness'  mings='50' maxgs='60'/>
  </biome>
</spawning>";

        static void Load() => SpawnGameStages.Parse(XElement.Parse(Spawning));

        [TestMethod]
        public void CapturesFullAndPartialRangesById()
        {
            Load();

            Assert.IsTrue(SpawnGameStages.TryGetRange("pine_forest", Hash("f_early"), out var min, out var max));
            Assert.AreEqual(int.MinValue, min, "maxgs only should leave the lower bound open");
            Assert.AreEqual(32, max);

            Assert.IsTrue(SpawnGameStages.TryGetRange("pine_forest", Hash("f_mid"), out min, out max));
            Assert.AreEqual(33, min);
            Assert.AreEqual(65, max);

            Assert.IsTrue(SpawnGameStages.TryGetRange("pine_forest", Hash("f_late"), out min, out max));
            Assert.AreEqual(99, min);
            Assert.AreEqual(int.MaxValue, max, "mings only should leave the upper bound open");
        }

        [TestMethod]
        public void EntriesWithoutGameStageAreUngated()
        {
            Load();

            // f_plain specifies neither mings nor maxgs, so it must not be recorded and
            // TryGetRange must report the always-eligible defaults.
            bool found = SpawnGameStages.TryGetRange("pine_forest", Hash("f_plain"), out var min, out var max);
            Assert.IsFalse(found);
            Assert.AreEqual(int.MinValue, min);
            Assert.AreEqual(int.MaxValue, max);
        }

        [TestMethod]
        public void SameGroupDifferentIdsDoNotCollide()
        {
            Load();

            // f_v1 and f_v2 share entitygroup, time and maxcount but differ by id and range.
            // Keying by the spawn id keeps them distinct (the bug avoided from the reference impl).
            Assert.IsTrue(SpawnGameStages.TryGetRange("pine_forest", Hash("f_v1"), out var min1, out var max1));
            Assert.IsTrue(SpawnGameStages.TryGetRange("pine_forest", Hash("f_v2"), out var min2, out var max2));
            Assert.AreEqual(10, min1);
            Assert.AreEqual(20, max1);
            Assert.AreEqual(50, min2);
            Assert.AreEqual(60, max2);
        }

        [TestMethod]
        public void UnknownBiomeOrIdIsUngated()
        {
            Load();
            Assert.IsFalse(SpawnGameStages.TryGetRange("desert", Hash("f_mid"), out _, out _));
            Assert.IsFalse(SpawnGameStages.TryGetRange("pine_forest", Hash("does_not_exist"), out _, out _));
        }

        [TestMethod]
        public void OnlyGatedEntriesAreCaptured()
        {
            Load();
            // f_early, f_mid, f_late, f_v1, f_v2 are gated; f_plain is not.
            Assert.AreEqual(5, SpawnGameStages.Count);
        }
    }
}
