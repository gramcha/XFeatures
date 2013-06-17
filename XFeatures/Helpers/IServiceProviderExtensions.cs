using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFeatures.Helpers
{
    internal static class IServiceProviderExtensions
    {
        public static TService GetService<TService>(this System.IServiceProvider serviceProvider)
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }

        public static TInterface GetService<SInterface, TInterface>(this System.IServiceProvider serviceProvider)
        {
            return (TInterface)serviceProvider.GetService(typeof(SInterface));
        }

        public static TService TryGetService<TService>(this System.IServiceProvider serviceProvider)
        {
            object service = serviceProvider.GetService(typeof(TService));
            if (service == null)
                return default(TService);
            else
                return (TService)service;
        }

        public static TInterface TryGetService<SInterface, TInterface>(this System.IServiceProvider serviceProvider)
        {
            object service = serviceProvider.GetService(typeof(SInterface));
            if (service == null)
                return default(TInterface);
            else
                return (TInterface)service;
        }
    }
}
