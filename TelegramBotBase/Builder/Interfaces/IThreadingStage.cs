using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface IThreadingStage
    {
        /// <summary>
        /// Configures the building stage to use a single thread for processing.
        /// </summary>
        /// <remarks>This method is typically used to optimize performance in scenarios where
        /// multithreading is unnecessary or could introduce complexity, such as when working with non-thread-safe
        /// resources or ensuring sequential execution.</remarks>
        /// <returns>An <see cref="IBuildingStage"/> instance configured to operate on a single thread.</returns>
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
        /// Configures the application to use a thread pool with the specified number of worker and I/O threads.
        /// </summary>
        /// <remarks>This method is typically used to optimize thread pool usage for applications with
        /// specific concurrency or I/O requirements.</remarks>
        /// <param name="workerThreads">The number of worker threads to allocate in the thread pool. Must be a positive integer.</param>
        /// <param name="ioThreads">The number of I/O threads to allocate in the thread pool. Must be a positive integer.</param>
        /// <returns>An <see cref="IBuildingStage"/> instance that allows further configuration of the application.</returns>
        public IBuildingStage UseThreadPool(int workerThreads, int ioThreads);

    }
}
