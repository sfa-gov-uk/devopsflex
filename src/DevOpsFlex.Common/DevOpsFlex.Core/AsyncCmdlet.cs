﻿namespace DevOpsFlex.Core
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Management.Automation;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Events;

    /// <summary>
    /// Serves as a base class for derived cmdlets that need to run async work and that do not
    /// depend on the Windows PowerShell runtime and that can be called directly from within another cmdlet.
    /// </summary>
    public class AsyncCmdlet : Cmdlet
    {
        private readonly Subject<BuildEvent> _threadSafeSubject = new Subject<BuildEvent>();

        /// <summary>
        /// The adapter that queues work to be done on the main thread.
        /// </summary>
        protected readonly ThreadQueue ThreadAdapter = new ThreadQueue();

        protected IObservable<BuildEvent> EventStream => _threadSafeSubject.AsObservable();

        /// <summary>
        /// Static initialization for all <see cref="AsyncCmdlet"/> classes.
        /// Currently it just ensures assembly redirects are properly applied.
        /// </summary>
        static AsyncCmdlet()
        {
            AppDomain.CurrentDomain.ApplyRedirects();
        }

        /// <summary>
        /// Process a block of asynchronous work coming in as an array of <see cref="Task"/>.
        /// </summary>
        /// <param name="tasks">The block of asyncronous work to be performed.</param>
        protected void ProcessAsyncWork(Task[] tasks)
        {
            Contract.Requires(tasks != null);

            var workers = tasks.Length;

            foreach (var task in tasks)
            {
                task.ContinueWith(_ => { if (--workers == 0) ThreadAdapter.Complete(); });
            }

            AsyncPump.Run(async () => await ThreadAdapter.ListenAsync(o => _threadSafeSubject.OnNext((BuildEvent)o)));

            Task.WaitAll(tasks);
        }
    }
}
