using Castle.DynamicProxy;
using Instrumentation.Interceptors;
using System;
using System.Reflection;

namespace Instrumentation
{
    public static class InstrumentationProxies
    {
        public static T AsLoggable<T>(this T obj) where T: class
        {
            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator
                .CreateInterfaceProxyWithTarget(obj, new Logger());
        }

        public static T AsResilient<T>(this T obj, int repeat = 3)
            where T : class =>
            obj.AsResilient(Retry.Immediately.Limit(repeat));

        public static T AsResilient<T>(this T obj, Retry retry)
            where T : class =>
            obj.AsResilient(retry, (m, ex) => true);
        
        public static T AsResilient<T>(this T obj, Retry retry, Func<MethodInfo, Exception, bool> repeat) 
            where T : class
        {
            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator
                .CreateInterfaceProxyWithTarget(obj, new Repeater(retry, repeat));
        }
    }
}
