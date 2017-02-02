using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Microsoft.Practices.ServiceLocation;

namespace Wyam.LiveReload
{
    internal class ReloadClientServiceLocator : IServiceLocator
    {
        private readonly ConcurrentBag<IReloadClient> _clients = new ConcurrentBag<IReloadClient>();

        public virtual IEnumerable<IReloadClient> ReloadClients => _clients.ToArray();

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType, string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>()
        {
            if (typeof(TService) == typeof(ReloadClient))
            {
                object client = CreateClient();
                return (TService) client;
            }

            throw new NotImplementedException();
        }

        public TService GetInstance<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            throw new NotImplementedException();
        }

        private object CreateClient()
        {
            ReloadClient client = new ReloadClient();
            _clients.Add(client);
            return client;
        }
    }
}