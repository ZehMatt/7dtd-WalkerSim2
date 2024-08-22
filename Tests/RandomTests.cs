using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WalkerSim.Tests
{
    [TestClass]
    public class RandomTests
    {
        const int SampleSize = 1000000; // Number of samples
        const int BucketCount = 100; // Divide the range into 10 buckets

        [TestMethod]
        public void TestBasic()
        {
            var prng = new WalkerSim.Random(0x1234, 0x4321);
            Assert.AreEqual(prng.State0, 0x1234U);
            Assert.AreEqual(prng.State1, 0x4321U);
            Assert.AreEqual(prng.Generate(), 0x80000246U);
            Assert.AreEqual(prng.State0, 0xBC247A5EU);
            Assert.AreEqual(prng.State1, 0x80000246U);
            for (var i = 0; i < 1000; ++i)
            {
                prng.Generate();
            }
            Assert.AreEqual(prng.State0, 0x0A597A43U);
            Assert.AreEqual(prng.State1, 0x12FC0827U);
        }

        [TestMethod]
        public void TestNextDistribution()
        {
            for (int p = 0; p < 32; p++)
            {
                const int maxValue = 100;
                int seed = 1 << p;
                var random = new WalkerSim.Random(seed);

                // Array to count how many numbers fall into each bucket
                int[] buckets = new int[BucketCount];

                // Generate numbers and categorize them into buckets
                for (int i = 0; i < SampleSize; i++)
                {
                    int value = random.Next(maxValue);
                    int bucketIndex = value * BucketCount / maxValue;
                    buckets[bucketIndex]++;
                }

                // Expected count per bucket for uniform distribution
                double expectedCount = SampleSize / (double)BucketCount;

                // Allow some margin of error (5% in this case)
                double tolerance = 0.05 * expectedCount;

                for (int i = 0; i < BucketCount; i++)
                {
                    Console.WriteLine($"Bucket {i}: {buckets[i]} (Expected: {expectedCount}, Tolerance: {tolerance})");
                    Assert.IsTrue(System.Math.Abs(buckets[i] - expectedCount) <= tolerance,
                        $"Bucket {i} is outside the tolerance range: {buckets[i]} vs expected {expectedCount}");
                }
            }
        }

        [TestMethod]
        public void TestNextDoubleDistribution()
        {
            for (int p = 0; p < 32; p++)
            {
                int seed = 1 << p;
                var random = new WalkerSim.Random(seed);

                // Array to count how many numbers fall into each bucket
                int[] buckets = new int[BucketCount];

                // Generate numbers and categorize them into buckets
                for (int i = 0; i < SampleSize; i++)
                {
                    double value = random.NextDouble();
                    int bucketIndex = (int)(value * BucketCount);
                    buckets[bucketIndex]++;
                }

                // Expected count per bucket for uniform distribution
                double expectedCount = SampleSize / (double)BucketCount;

                // Allow some margin of error (5% in this case)
                double tolerance = 0.05 * expectedCount;

                for (int i = 0; i < BucketCount; i++)
                {
                    Console.WriteLine($"Bucket {i}: {buckets[i]} (Expected: {expectedCount}, Tolerance: {tolerance})");
                    Assert.IsTrue(System.Math.Abs(buckets[i] - expectedCount) <= tolerance,
                        $"Bucket {i} is outside the tolerance range: {buckets[i]} vs expected {expectedCount}");
                }
            }
        }
    }
}
