// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroudServices.Scheduling;
using CloudArchive.Services;
using DATABASE.Context;
using Microsoft.Extensions.DependencyInjection;

namespace CloudArchive.ScheduledTasks
{
    public class MailSenderBackgroundService: IScheduledTask
    {
        public string ServiceName { get => "MailSenderBackgroundService"; }
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MailSenderBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IBackgroundServiceLog backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                    SearchServiceDBContext dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                    IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var emailQueue = dbContext.EmailQueue.Where(eq => !eq.Sent.HasValue);
                    foreach (var email in emailQueue)
                    {
                        var result = await emailService.SendEmailAsync(
                            email.Recipients.Split(',').ToList(), 
                            email.Subject, 
                            email.Body, 
                            email.EmailQueueDocFiles.Select(ed => ed.DocFile).ToList());
                        if (result == "OK")
                        {
                            email.Sent = DateTime.Now;
                        }
                        else
                        {
                            backgroundServiceLog.AddError(result, ServiceName);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
            }
            await Task.Delay(JobScheduler.GetWaitDelay(ServiceName), cancellationToken);
        }
    }
}
