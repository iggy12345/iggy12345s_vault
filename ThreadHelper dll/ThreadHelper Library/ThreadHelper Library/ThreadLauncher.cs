﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ThreadHelper_Library
{
    public class ThreadLauncher<T>
    {
        // Contains and launches threads as told to.

        private List<TSubject<T>> modules; // List of the modules in the list of threads
        private List<Thread> Threads;   // List of threads to be launched.
        private List<bool> isLaunched;        // Used to tell when the threads have been launched
        private Queue<CommQueueData<T>> CommQueue;     // Queue used for callback communication
        private bool isExitting;


        public ThreadLauncher()
        {
            Threads = new List<Thread>();
            modules = new List<TSubject<T>>();
            isLaunched = new List<bool>();
            CommQueue = new Queue<CommQueueData<T>>();
            isExitting = false;

            Thread TxThread = new Thread(new ThreadStart(TxManager));
            TxThread.Start();   // Starts the TxManager
        }

        public void Exit()
        {
            // Sends a mass exit message to all modules notifying to shut down.
            foreach (TSubject<T> module in modules)
                module.Exit();

            // Waits for any last minute shutdown procedures
            bool isDone = false;
            int iter = 0;
            do
            {
                isDone = true;
                foreach (Thread t in Threads)
                    if (t.IsAlive)
                        isDone = false;

            } while (!isDone && iter++ < 5000);
        }

        public void Add(TSubject<T> module)
        {
            // Adds a module to the list of handled threads
            modules.Add(module);                                    // Adds the module to the list
            module.MessageTx += OnMessageRx;                        // Subscribes to the module's messageTx event
            Threads.Add(new Thread(new ThreadStart(module.Start))); // Adds the thread to the list
            isLaunched.Add(false);                                  // Adds a boolean status flag to the list as well
        }

        public void Remove(int index, bool overrideLaunch = false)
        {
            // Removes a module from the list of handled threads
            if (isLaunched[index] && !overrideLaunch)
                throw new InvalidOperationException("Cannot halt a thread that is already launched!");
            else
            {
                if (isLaunched[index])
                    Threads[index].Abort(); // Aborts the thread if it's currently running.

                // Removes the thread from the list and its corresponding status boolean
                Threads.RemoveAt(index);
                isLaunched.RemoveAt(index);
                modules.RemoveAt(index);
            }
        }

        public void Remove(TSubject<T> module, bool overrideLaunch = false)
        {
            // Removes a module from the list of handled threads
            if (modules.Contains(module))
                Remove(modules.FindIndex(x => x.Id == module.Id), overrideLaunch);
            else
                throw new InvalidOperationException("The thread list does not contain that module!");
        }

        // Operators
        public static ThreadLauncher<T> operator +(ThreadLauncher<T> launcher, TSubject<T> module)
        {
            // Adds the modules to the lists
            launcher.Add(module);
            return launcher;
        }

        public static ThreadLauncher<T> operator -(ThreadLauncher<T> launcher, TSubject<T> module)
        {
            // Adds the modules to the lists
            launcher.Remove(module);
            return launcher;
        }

        protected virtual void OnMessageRx(object sender, MessageEventArgs<T> e)
        {
            // Triggered when another module tries to send out a message
            CommQueue.Enqueue(e.Message);   // Enqueues the message
        }

        protected virtual void OnMessageTx(CommQueueData<T> e)
        {
            // Sends a message from the queue as soon as it's queued
            if (e.Type == CommMessageType.Addressed)
            {
                // Sends an addressed message to all modules that have the name in the message
                if (modules.Find(x => e.Addressee == x.Name) != null)
                    foreach (TSubject<T> item in modules.FindAll(x => e.Addressee == x.Name))
                        item.ReceiveMailbox(e.Message); // Inserts the message into the mailbox
            }
            else if (e.Type == CommMessageType.UnAddressed)
            {
                // Sends a message to every module
                foreach (TSubject<T> module in modules)
                    module.ReceiveMailbox(e.Message);
            }
            else
                throw new IndexOutOfRangeException("Message Type unknown!");
        }

        private void TxManager()
        {
            // Monitors the CommQueue, sending messages as needed
            while(!isExitting)
            {
                if (CommQueue.Count > 0)
                    OnMessageTx(CommQueue.Dequeue());
            }
        }
    }
}
