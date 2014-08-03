using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Sinject.Runtime
{
    public static class Stubs
    {
        public static bool CallStub(string type, string method, object[] args, out object result)
        {
            Subscription subscription;
            if (!stubs.TryGetValue(type, out subscription))
            {
                result = null;
                return false;
            }

            result = subscription.Stub.GetType().InvokeMember(
                method, BindingFlags.InvokeMethod, null, subscription.Stub, args);
            return true;
        }

        public static IDisposable Inject(Type type, object stub)
        {
            var key = type.FullName;
            var subscription = new Subscription(key, stub);
            while (true)
            {
                var added = stubs.GetOrAdd(key, subscription);
                if (added == subscription) return subscription;
                Monitor.Enter(added.Sync);
            }
        }

        public static IDisposable TryInject(Type type, object stub)
        {
            var key = type.FullName;
            var subscription = new Subscription(key, stub);
            return stubs.TryAdd(key, subscription) ? subscription : null;
        }

        private class Subscription : IDisposable
        {
            public Subscription(string key, object stub)
            {
                this.key = key;
                this.Sync = new object();
                Monitor.Enter(this.Sync);
                this.Stub = stub;
            }

            public void Dispose()
            {
                Subscription value;
                stubs.TryRemove(this.key, out value);
                Monitor.Exit(this.Sync);
            }

            public readonly object Stub;
            public object Sync;

            private string key;
        }

        private static ConcurrentDictionary<string, Subscription> stubs = new ConcurrentDictionary<string, Subscription>();
    }
}
