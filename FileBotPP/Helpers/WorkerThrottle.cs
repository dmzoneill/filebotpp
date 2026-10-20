using System;
using System.Threading;

namespace FileBotPP.Helpers
{
    internal static class WorkerThrottle
    {
        public static void WaitForIdle(Func<int> workingCount)
        {
            Thread.Sleep(Factory.Instance.Random.Next(10, 40));
            Factory.Instance.LogLines.Enqueue("Waiting for threads");

            var count = 2;

            while (count > 1)
            {
                count = workingCount();
                Thread.Sleep(Factory.Instance.Random.Next(10, 40));
            }
        }

        public static void Limit(Func<int> workingCount, int num, bool wait)
        {
            var count = num + 1;

            while (count > num)
            {
                count = workingCount();
                Thread.Sleep(wait ? Factory.Instance.Random.Next(10, 40) : 5);
            }

            Thread.Sleep(wait ? Factory.Instance.Random.Next(10, 40) : 5);
        }
    }
}
