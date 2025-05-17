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
        /// Configures the building stage to use the thread pool for executing tasks.
        /// </summary>
        /// <remarks>This method enables the use of the thread pool for task execution, which can improve
        /// performance in scenarios where multiple tasks need to be executed concurrently. Ensure that the thread pool
        /// is appropriate for your workload and does not introduce contention or resource exhaustion.</remarks>
        /// <returns>An <see cref="IBuildingStage"/> instance configured to use the thread pool.</returns>
        public IBuildingStage UseThreadPool();

        /// <summary>
        /// Using the threadpool for managing requests.
        /// </summary>
        /// <param name="workerThreads">Number of concurrent working threads.</param>
        /// <param name="ioThreads">Number of concurrent I/O threads.</param>
        /// <returns></returns>
        public IBuildingStage UseThreadPool(int workerThreads, int ioThreads);

    }
}
