namespace Statiq.Web
{
    /// <summary>
    /// Defines when during engine execution the process will be run.
    /// </summary>
    public enum ProcessTiming
    {
        /// <summary>
        /// The process should be started only once before other processes on the first engine execution.
        /// </summary>
        Initialization,

        /// <summary>
        /// The process should be started before each pipeline execution.
        /// </summary>
        BeforeExecution,

        /// <summary>
        /// The process should be started after all pipelines have executed.
        /// </summary>
        AfterExecution,

        /// <summary>
        /// The process should be started after normal pipelines are executed and before deployment pipelines are executed.
        /// If no deployment pipelines are executed, this is effectively the same as <see cref="AfterExecution"/>.
        /// </summary>
        BeforeDeployment
    }
}
