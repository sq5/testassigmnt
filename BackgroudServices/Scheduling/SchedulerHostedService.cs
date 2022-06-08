// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudArchive.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroudServices.Scheduling
{
    public class SchedulerHostedService : HostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private readonly List<SchedulerTaskWrapper> _scheduledTasks = new List<SchedulerTaskWrapper>();

        public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks, IServiceScopeFactory serviceScopeFactory)
        {
            var referenceTime = DateTime.UtcNow;
            _serviceScopeFactory = serviceScopeFactory;

            foreach (var scheduledTask in scheduledTasks)
            {
                _scheduledTasks.Add(new SchedulerTaskWrapper
                {
                    isRunning = false,
                    ServiceName = scheduledTask.ServiceName,
                    Task = scheduledTask,
                    NextRunTime = JobScheduler.GetNextOccurenceTime(scheduledTask.ServiceName).Value
                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                _backgroundServiceLog.AddInfo("Started Background Services Mantainer", "Background Services Mantainer");
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();

                await taskFactory.StartNew(
                    async () =>
                    {
                        try
                        {
                            taskThatShouldRun.isRunning = true;
                            await taskThatShouldRun.Task.ExecuteAsync(cancellationToken);
                            taskThatShouldRun.isRunning = false;
                        }
                        catch (Exception ex)
                        {
                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            UnobservedTaskException?.Invoke(this, args);

                            if (!args.Observed)
                            {
                                throw;
                            }
                        }
                    },
                    cancellationToken);
            }
        }

        private class SchedulerTaskWrapper
        {
            public bool isRunning;
            public string ServiceName { get; set; }
            public IScheduledTask Task { get; set; }
            public DateTime NextRunTime { get; set; }

            public void Increment()
            {
                NextRunTime = JobScheduler.GetNextOccurenceTime(ServiceName).Value;
            }

            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && !isRunning;
            }
        }
    }
}
