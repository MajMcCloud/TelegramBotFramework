using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface IThreadingStage
    {
        /// <summary>
        /// Uses one single thread for message loop. (Default)
        /// </summary>
        /// <returns></returns>
        public IBuildingStage UseSingleThread();

        /// <summary>
        /// Using the threadpool for managing requests.
        /// </summary>
        /// <param name="workerThreads">Number of concurrent working threads.</param>
        /// <param name="ioThreads">Number of concurrent I/O threads.</param>
        /// <returns></returns>
        public IBuildingStage UseThreadPool(int workerThreads = 2, int ioThreads = 1);

    }
}
