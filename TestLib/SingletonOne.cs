using System;
using System.Collections.Generic;
using System.Text;

namespace TestLib
{
    public sealed class SingletonOne
    {
        private static object syncObject = new object();
        private static volatile SingletonOne instance;
        public static SingletonOne Instance
        {
            get
            {
                lock (syncObject)
                {
                    if (instance != null)
                    {
                        instance = new SingletonOne();
                    }

                    return instance;
                }
            }
        }

        private SingletonOne()
        {
        }
    }
}
