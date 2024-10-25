﻿using Finite_State_Machine_Designer.Configuration;
using Finite_State_Machine_Designer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Finite_State_Machine_Designer.Services
{
    public class DeleteUnconfirmedUsersService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IOptions<UsersConfig> usersConfig,
        ILogger<DeleteUnconfirmedUsersService> logger
            ) : BackgroundService
    {
        /// <summary>
        /// Deletes users that haven't confirmed within 
        /// <see cref="UsersConfig.MaxUnconfirmedDays"/> in the database.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan deletionInterval
                = TimeSpan.FromDays(usersConfig.Value.DeleteUnconfirmedInterval);
            TimeSpan maxUnconfirmedTime
                = TimeSpan.FromDays(usersConfig.Value.MaxUnconfirmedDays);
            using PeriodicTimer timer = new(deletionInterval);

            try
            {
                await DeleteUnconfirmedUsers(
                        maxUnconfirmedTime, stoppingToken);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                    await DeleteUnconfirmedUsers(
                        maxUnconfirmedTime, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        private async Task DeleteUnconfirmedUsers(TimeSpan maxUnconfirmedTime,
            CancellationToken cancelToken)
        {
            try
            {
                await using ApplicationDbContext dbContext
                    = await dbFactory.CreateDbContextAsync(cancelToken);
                int numDeleted = await dbContext.Database.ExecuteSqlAsync(
                    @$"DELETE dbo.AspNetUsers
                    WHERE EmailConfirmed = 0
                    AND ({DateTime.UtcNow.Ticks} - CreationTime) 
                    >= {maxUnconfirmedTime.Ticks}", cancelToken);
                logger.LogInformation(
                    "Deleted {Num} users that didn't confirm emails for {Time}",
                    numDeleted, maxUnconfirmedTime);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("{Error}", ex.ToString());
            }
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
