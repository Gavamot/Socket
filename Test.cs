using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace Test
{
    public static class Test
    {
        public static void CheckForEquals(object res, object actual)
        {
            Assert.Equal(JsonConvert.SerializeObject(res), JsonConvert.SerializeObject(actual));
        }

        public static T InvokePrivateFunction<T>(Type obj, string methodName, object[] parameters)
        {
            MethodInfo dynMethod = obj.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (T) dynMethod.Invoke(new object(), parameters);
        }

        public static T InvokePrivateFunction<T>(Type obj, string methodName)
        {
            MethodInfo dynMethod = obj.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)dynMethod.Invoke(new object(), new object[0]);
        }

        public static void InvokePrivateProcedure(Type obj, string methodName, object[] parameters)
        {
            MethodInfo dynMethod = obj.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
           dynMethod.Invoke(new object(), parameters);
        }

        public static void InvokePrivateProcedure(Type obj, string methodName)
        {
            MethodInfo dynMethod = obj.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(new object(), new object[0]);
        }

    }
}
