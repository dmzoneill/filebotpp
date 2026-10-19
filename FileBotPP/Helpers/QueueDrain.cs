using System;
using System.Collections.Concurrent;
using FileBotPP.Tree;

namespace FileBotPP.Helpers
{
    internal static class QueueDrain
    {
        public static void Drain(ConcurrentQueue<IFileItem> queue, string status, Action<IFileItem> onItem)
        {
            Factory.Instance.WindowFileBotPp.set_status_text(status);

            IFileItem item;
            while (queue.TryDequeue(out item))
            {
                onItem(item);
            }
        }
    }
}
