namespace Statiq.Web
{
    public enum ProcessTiming
    {
        /// <summary>
        /// Executed before pipeline execution.
        /// </summary>
        BeforeExecution,

        /// <summary>
        /// Executed after all pipelines have executed.
        /// </summary>
        AfterExecution,

        /// <summary>
        /// Executed after normal pipelines are executed and before deployment pipelines are executed.
        /// This is the same as <see cref="AfterExecution"/> if no deployment pipelines are executed.
        /// </summary>
        BeforeDeployment
    }
}
