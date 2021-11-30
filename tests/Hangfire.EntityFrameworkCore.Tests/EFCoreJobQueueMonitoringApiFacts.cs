﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Hangfire.EntityFrameworkCore.Tests;

public class EFCoreJobQueueMonitoringApiFacts : EFCoreStorageTest
{
    [Fact]
    public static void Ctor_Throws_WhenStorageParameterIsNull()
    {
        EFCoreStorage storage = null;

        Assert.Throws<ArgumentNullException>(nameof(storage),
            () => new EFCoreJobQueueMonitoringApi(storage));
    }

    [Fact]
    public void Ctor_CreatesInstance()
    {
        var storage = CreateStorageStub();

        var instance = new EFCoreJobQueueMonitoringApi(storage);

        Assert.Same(storage, Assert.IsType<EFCoreStorage>(instance.GetFieldValue("_storage")));
    }

    [Fact]
    public void GetEnqueuedJobIds_Throws_IfQueueParameterIsNull()
    {
        string queue = null;
        var instance = new EFCoreJobQueueMonitoringApi(CreateStorageStub());

        Assert.Throws<ArgumentNullException>(nameof(queue),
            () => instance.GetEnqueuedJobIds(queue, 0, 1));
    }

    [Fact]
    public void GetEnqueuedJobIds_ReturnsEmptyCollection_IfQueueIsEmpty()
    {
        string queue = "queue";
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetEnqueuedJobIds(queue, 5, 15);

        Assert.Empty(result);
    }

    [Fact]
    public void GetEnqueuedJobIds_ReturnsCorrectResult()
    {
        string queue = "queue";
        var jobs = Enumerable.Repeat(0, 10).
            Select(_ => new HangfireJob
            {
                InvocationData = InvocationDataStub,
                QueuedJobs = new List<HangfireQueuedJob>
                {
                        new HangfireQueuedJob
                        {
                            Queue = queue,
                        }
                },
            }).
            ToArray();
        UseContextSavingChanges(context => context.AddRange(jobs));
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetEnqueuedJobIds(queue, 3, 2).ToArray();

        Assert.Equal(2, result.Length);
        var jobIds = jobs.SelectMany(x => x.QueuedJobs).OrderBy(x => x.Id).
            Select(x => x.JobId.ToString(CultureInfo.InvariantCulture)).
            ToArray();
        Assert.Equal(jobIds[3], result[0]);
        Assert.Equal(jobIds[4], result[1]);
    }

    [Fact]
    public void GetFetchedJobIds_Throws_IfQueueParameterIsNull()
    {
        string queue = null;
        var instance = new EFCoreJobQueueMonitoringApi(CreateStorageStub());

        Assert.Throws<ArgumentNullException>(nameof(queue),
            () => instance.GetFetchedJobIds(queue, 0, 1));
    }

    [Fact]
    public void GetFetchedJobIds_ReturnsEmptyCollection_IfQueueIsEmpty()
    {
        string queue = "queue";
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetFetchedJobIds(queue, 5, 15);

        Assert.Empty(result);
    }

    [Fact]
    public void GetFetchedJobIds_ReturnsCorrectResult()
    {
        string queue = "queue";
        var jobs = Enumerable.Repeat(0, 10).
            Select(_ => new HangfireJob
            {
                InvocationData = InvocationDataStub,
                QueuedJobs = new List<HangfireQueuedJob>
                {
                        new HangfireQueuedJob
                        {
                            Queue = queue,
                            FetchedAt = DateTime.UtcNow,
                        }
                },
            }).
            ToArray();
        UseContextSavingChanges(context => context.AddRange(jobs));
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetFetchedJobIds(queue, 3, 2).ToArray();

        Assert.Equal(2, result.Length);
        var jobIds = jobs.SelectMany(x => x.QueuedJobs).OrderBy(x => x.Id).
            Select(x => x.JobId.ToString(CultureInfo.InvariantCulture)).
            ToArray();
        Assert.Equal(jobIds[3], result[0]);
        Assert.Equal(jobIds[4], result[1]);
    }

    [Fact]
    public void GetQueues_ReturnsEmptyCollection_WhenQueuedItemsNotExisits()
    {
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var queues = instance.GetQueues();

        Assert.Empty(queues);
    }

    [Fact]
    public void GetQueues_ReturnsAllGivenQueues()
    {
        var date = DateTime.UtcNow;
        var queues = Enumerable.Repeat(0, 5).
            Select(x => Guid.NewGuid().ToString()).
            ToArray();
        var job = new HangfireJob
        {
            InvocationData = InvocationDataStub,
            QueuedJobs = queues.
                Select(x => new HangfireQueuedJob
                {
                    Queue = x,
                }).
                ToList()
        };
        UseContextSavingChanges(context => context.Add(job));
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetQueues();

        Assert.Equal(queues.OrderBy(x => x), result.OrderBy(x => x));
    }

    [Fact]
    public void GetQueueStatistics_Throws_whenQueueParametrIsNull()
    {
        string queue = null;
        var instance = new EFCoreJobQueueMonitoringApi(CreateStorageStub());

        Assert.Throws<ArgumentNullException>(nameof(queue),
            () => instance.GetQueueStatistics(queue));
    }

    [Fact]
    public void GetQueueStatistics_ReturnsZeroes_WhenQueueIsEmpty()
    {
        string queue = "queue";
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetQueueStatistics(queue);

        Assert.NotNull(result);
        Assert.Equal(default, result.Enqueued);
        Assert.Equal(default, result.Fetched);
    }

    [Fact]
    public void GetQueueStatistics_ReturnsCorrectResult_WhenQueueIsEmpty()
    {
        string queue = "queue";
        var jobs = Enumerable.Range(0, 5).
            Select(index => new HangfireJob
            {
                InvocationData = InvocationDataStub,
                QueuedJobs = new List<HangfireQueuedJob>
                {
                        new HangfireQueuedJob
                        {
                            Queue = queue,
                            FetchedAt = index < 2 ? default(DateTime?) : DateTime.UtcNow,
                        }
                },
            }).
            ToArray();
        UseContextSavingChanges(context => context.AddRange(jobs));
        var instance = new EFCoreJobQueueMonitoringApi(Storage);

        var result = instance.GetQueueStatistics(queue);

        Assert.NotNull(result);
        Assert.Equal(2, result.Enqueued);
        Assert.Equal(3, result.Fetched);
    }
}
