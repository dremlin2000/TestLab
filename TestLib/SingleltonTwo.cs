using System;
using System.Collections.Generic;
using System.Text;

namespace TestLib
{
    public sealed class SingletonTwo
    {
        private static readonly Lazy<SingletonTwo> lazyInstance = new Lazy<SingletonTwo>(() => new SingletonTwo());
        public static SingletonTwo Instance => lazyInstance.Value;

        private SingletonTwo()
        {

        }
    }
}
