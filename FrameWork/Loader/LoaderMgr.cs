using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace FrameWork
{
    public class LoaderMgr
    {
        #region Static

        public static int LoaderCount;
        public static int MaxThread = 0;

        public static void Start()
        {
            long Start = TCPManager.GetTimeStampMS();

            List<LoadFunction> WaitEnd = new List<LoadFunction>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                HashSet<Type> loaded = new HashSet<Type>();

                foreach (Type type in assembly.GetTypes())
                    Load(WaitEnd, loaded, type);
            }

            Wait();

            foreach (LoadFunction Function in WaitEnd)
                Function.Invoke();

            long End = TCPManager.GetTimeStampMS();

            Log.Success("LoaderMgr", "Loading complete in : " + (End - Start) + "ms");

            GC.Collect(2);

            Log.Success("LoaderMgr", "Collected garbage");
        }

        private static void Load(List<LoadFunction> WaitEnd, HashSet<Type> loaded, Type type)
        {
            // Pick up a class
            if (type.IsClass != true)
                return;

            if (loaded.Contains(type))
                return; // Already loaded

            object[] attrArray = type.GetCustomAttributes(typeof(ServiceAttribute), false);
            ServiceAttribute serviceDecl = attrArray.Length > 0 ? (ServiceAttribute)attrArray[0] : null;

            // Ensure all dependencies have been initialized
            if (serviceDecl != null)
            {
                foreach (Type dependency in serviceDecl.Dependencies)
                    if (!loaded.Contains(dependency))
                        Load(WaitEnd, loaded, dependency);
            }

            // Iterate over all annotated methods
            foreach (MethodInfo m in type.GetMethods())
                foreach (LoadingFunctionAttribute at in m.GetCustomAttributes(typeof(LoadingFunctionAttribute), false))
                {
                    LoadFunction loadfunction = (LoadFunction)Delegate.CreateDelegate(typeof(LoadFunction), m);

                    if (at.Immediate)
                        InitLoad(loadfunction);
                    else
                        WaitEnd.Add(loadfunction);
                }

            // Register the service as loaded
            if (serviceDecl != null)
                loaded.Add(type);
        }

        public static void InitLoad(LoadFunction Func)
        {
            Func.Invoke();
        }
        public static void InitMultiLoad(MultiLoadFunction Func,int Count)
        {
            for (int i = 0; i < Count; ++i)
                new LoaderMgr(Func, Count,i);
        }

        public static void Wait()
        {
            while (LoaderCount > 0)
                Thread.Sleep(50);
        }

        #endregion

        public delegate void LoadFunction();
        public delegate void MultiLoadFunction(int ThreadCount, int Id);

        private int Id;
        private int Count;
        private LoadFunction _Function;
        private MultiLoadFunction _MultiFunction;

        public LoaderMgr(LoadFunction Function)
        {
            _Function = Function;
            ThreadStart Start = new ThreadStart(Load);
            Thread LoadThread = new Thread(Start);
            LoadThread.Start();
        }

        public LoaderMgr(MultiLoadFunction Function, int Count, int Id)
        {
            _MultiFunction = Function;
            this.Count = Count;
            this.Id = Id;
            ThreadStart Start = new ThreadStart(MultiLoad);
            Thread LoadThread = new Thread(Start);
            LoadThread.Start();
        }

        public void Load()
        {
            Interlocked.Increment(ref LoaderCount);
            try
            {
                if (_Function != null)
                {
                    Log.Debug("Load", "Loading : " + _Function.Method.Name);
                    _Function.Invoke();
                }
            }
            catch (Exception e)
            {
                Log.Error(_Function.Method.Name, e.ToString());
            }
            finally
            {
                Interlocked.Decrement(ref LoaderCount);
            }
        }

        public void MultiLoad()
        {
            Interlocked.Increment(ref LoaderCount);
            try
            {
                if (_MultiFunction != null)
                {
                    Log.Debug("Load", "Loading : " + _MultiFunction.Method.Name +", Id="+Id);
                    _MultiFunction.Invoke(Count,Id);
                }
            }
            catch (Exception e)
            {
                Log.Error(_MultiFunction.Method.Name, e.ToString());
            }
            finally
            {
                Interlocked.Decrement(ref LoaderCount);
            }
        }
    }
}
