using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thread_Basic_Operations
{
    class Program
    {
        static void Main(string[] args)
        {
            Test_0_AsyncAwait();
            //Test_1_Lock();
            //Test_2_Monitor();
            //Test_3_ManualResetEvent();
            //Test_4_AutoResetEvent();
            //Test_5_Mutex();
            //Test_6_SemaphoreExample();
        }

        #region Async Await Example

        // When there's no synchronization context in the app (Like in the simple ConsoleApp), async/await has no difference to creating
        // new threads approach, because without sync context it will create new threads anyway
        // unlike when we have sync context (Like in the WindowsForms app) async await will try to execute asynchronous code within
        // the thread which calls the code (e.g. main thread)

        // internally out C# apps implement such thing as state machine, which divides code into chunks and then decides on which thread to
        // execute them

        // but, as tests show, code in the main method was executed with thread(1), in the newwly created thread with id 5
        // and all async await with thread(4)
 
        static int delay = 10000;
        static async void Test_0_AsyncAwait()
        {
            Console.WriteLine("Hello_1 " + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("Hello_2 " + Thread.CurrentThread.ManagedThreadId);

            AsyncWork();
            Thread thread = new Thread(ThreadWork);
            thread.Name = "CoolWorker";
            thread.Start();

            Console.WriteLine("Hello_5 " + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("Hello_6 " + Thread.CurrentThread.ManagedThreadId);

            AsyncWork_2();

            AsyncWork_3();

            Console.WriteLine("Hello_16 " + Thread.CurrentThread.ManagedThreadId);
            Console.ReadKey();
        }

        static async void AsyncWork()
        {
            Console.WriteLine("Hello_3 " + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(delay);
            Console.WriteLine("Hello_4 " + Thread.CurrentThread.ManagedThreadId);
        }
        static void ThreadWork()
        {
            Console.WriteLine("Hello_7 " + Thread.CurrentThread.ManagedThreadId);

            AdditionalAsyncWorkForThread();

            Console.WriteLine("Hello_8 " + Thread.CurrentThread.ManagedThreadId);
            Task.Delay(delay).Wait();
            Console.WriteLine("Hello_9 " + Thread.CurrentThread.ManagedThreadId);
        }

        async static void AdditionalAsyncWorkForThread()
        {
            Console.WriteLine("Hello_10 " + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(4000);
            Console.WriteLine("Hello_11 " + Thread.CurrentThread.ManagedThreadId);
        }
        static async void AsyncWork_2()
        {
            Console.WriteLine("Hello_12 " + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(delay);
            Console.WriteLine("Hello_13 " + Thread.CurrentThread.ManagedThreadId);
        }
        static async void AsyncWork_3()
        {
            Console.WriteLine("Hello_14 " + Thread.CurrentThread.ManagedThreadId);
            //await Task.Delay(delay + 2000);
            await Task.Delay(1000);

            // this one even got lost when method was declared as Task and awaited in the main method
            Console.WriteLine("Hello_15 " + Thread.CurrentThread.ManagedThreadId);
        }

        #endregion

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
