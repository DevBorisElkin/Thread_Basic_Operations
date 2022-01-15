using System;
using System.Threading;

namespace Thread_Basic_Operations
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test_1_Lock();
            //Test_2_Monitor();
            //Test_3_ManualResetEvent();
            //Test_4_AutoResetEvent();
            //Test_5_Mutex();
            Test_6_SemaphoreExample();
        }

        #region lock test

        static IntRefType locker_lock; // can be object
        static void Test_1_Lock() // test for lock keyword
        {
            locker_lock = new IntRefType();
            for (int i = 0; i < 5; i++)
            {
                new Thread(DoWork_Lock).Start();
            }
        }

        public static void DoWork_Lock()
        {
            lock (locker_lock)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting");

                Thread.Sleep(2000);

                locker_lock.Value += 1;
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed, locker value: {locker_lock.Value}");
            }
        }
        #endregion

        #region Monitor test

        static IntRefType locker_monitor; // can be object
        static void Test_2_Monitor() // test for Monitor
        {
            locker_monitor = new IntRefType();
            for (int i = 0; i < 5; i++)
            {
                new Thread(DoWork_Monitor).Start();
            }
        }

        public static void DoWork_Monitor()
        {
            try
            {
                Monitor.Enter(locker_monitor);

                Thread.Sleep(1000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} starting");

                Thread.Sleep(2000);

                // in case of Exception with Monitor we get flexibility to release locked object in finally{} block
                //int divider = 0;
                //int divided = 2 / divider;

                locker_monitor.Value += 1;
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed, locker value: {locker_monitor.Value}");
            }
            catch (Exception e) { }
            finally
            {
                Monitor.Exit(locker_monitor);
            }
            
        }

        #endregion

        #region Manual Reset Event Example

        static ManualResetEvent resetEvent = new ManualResetEvent(false); // false = Reset, true = Set

        // Как работает?
        /*
        ManualResetEvent предстовляет из себя механизм, подобно флагу, который позволяет "замораживать" потоки,
        в потоках, которые нужно заморозить, вызываем resetEvent.WaitOne(); // resetEvent - экземпляр класса ManualResetEvent
        resetEvent.WaitOne(); заставит поток ожидать, пока значение resetEvent не станет true
        При создании экземпляра ManualResetEvent(state) мы указываем state, true или false, или ниже в коде можем вызвать
        .Reset(); (равно базовому значению false)/ .Set(); (равно базовому значению true)

        ManualResetEvent releases all waiting threads at a one single call resetEvent.Set();
        On the other hand, AutoResetEvent releases only one waiting thread, therefore, to keep releasing threads by one,
        you need to call autoResetEvent.Set() each time for each waiting thread
         */
        static void Test_3_ManualResetEvent()
        {
            Thread writeThread = new Thread(Test_3_Write);
            writeThread.Start();

            for (int i = 0; i < 5; i++)
            {
                new Thread(Test_3_Read).Start();
            }
        }

        static void Test_3_Write()
        {
            resetEvent.Reset();
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Writing");
            Thread.Sleep(5000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished Writing");
            resetEvent.Set();
        }

        static void Test_3_Read()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Waiting...");
            resetEvent.WaitOne();
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Reading");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished Reading");
        }

        #endregion

        #region Auto Reset Event Test

        // AutoResetEvent очень похож на ManualResetEvent

        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        static void Test_4_AutoResetEvent()
        {
            for (int i = 0; i < 5; i++)
            {
                new Thread(AutoResetEvent_Writing).Start();
            }

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(5000);
                autoResetEvent.Set();
            }

            // this will cause logical error(2 threads or more will start writing all at once)
            // , however, the code will execute anyways. Mutex on the other hand won't allow to call Set() outside
            // of the synchronized block of code, and will throw an exception if programmer tries to do that
            //Thread.Sleep(3000);
            //autoResetEvent.Set();
        }

        static void AutoResetEvent_Writing()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Waiting...");
            autoResetEvent.WaitOne();
            Thread.Sleep(1000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Writing...");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished Writing");
        }

        #endregion

        #region Mutex example

        // Consider Mutex as a locker which only one thread at a time can gain access to. Thread requests access to the mitex via
        // mutex.WaitOne(); one thread gains access and proceed foreward, other threads will wait, then the first thread
        // will release mutex and one of waiting threads will access mutex and so on..

        // mutex if protected from releasing access -> only thread which gained it can release it.

        static Mutex mutex = new Mutex();

        static void Test_5_Mutex()
        {
            for (int i = 0; i < 5; i++)
            {
                new Thread(Write_Mutex).Start();
            }

            // this will call:
            //System.ApplicationException:
            //'Object synchronization method was called from an unsynchronized block of code.'
            //mutex.ReleaseMutex();
            // mutex can be released only from synchronized block of code
        }

        static void Write_Mutex()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Waiting...");
            
            mutex.WaitOne(); // Thread requesting mutex, if it's released, proceeds foreward
            
            Thread.Sleep(1000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Writing...");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished Writing");
            mutex.ReleaseMutex(); // Releases mutex which means other thread can gain access to the mutex
        }

        #endregion

        #region Semaphore example

        //static Semaphore semaphore = new Semaphore(1, 1);
        static Semaphore semaphore = new Semaphore(2, 2);
        //static Semaphore semaphore = new Semaphore(2, 1); // initial count = 2, max count = 1, will throw an exception

        // Semaphore - позволяет указывать, сколько потоков будет работать над процессом

        static void Test_6_SemaphoreExample()
        {
            for (int i = 0; i < 5; i++)
            {
                new Thread(Semaphore_Write).Start();
            }
        }

        static void Semaphore_Write()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Waiting...");

            semaphore.WaitOne();
            Thread.Sleep(1000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is Writing...");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished Writing");
            semaphore.Release(); // You can specify the amount you want to release
        }
        #endregion
    }
}
