using System;
using System.Collections.Generic;
using System.IO;

namespace DynamixelServo.Driver
{
    public static class ExceptionHelper
    {
        public static T TryThrow<T>(Func<T> call, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    return call();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T2 TryThrow<T1, T2>(Func<T1, T2> call, T1 arg1, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    return call(arg1);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T3 TryThrow<T1, T2, T3>(Func<T1, T2, T3> call, T1 arg1, T2 arg2, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    return call(arg1, arg2);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T4 TryThrow<T1, T2, T3, T4>(Func<T1, T2, T3, T4> call, T1 arg1, T2 arg2, T3 arg3, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    return call(arg1, arg2, arg3);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T5 TryThrow<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> call, T1 arg1, T2 arg2, T3 arg3, T4 arg4, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    return call(arg1, arg2, arg3, arg4);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void TryThrow(Action call, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    call();
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void TryThrow<T>(Action<T> call, T arg1, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    call(arg1);
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void TryThrow<T1, T2>(Action<T1, T2> call, T1 arg1, T2 arg2, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    call(arg1, arg2);
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void TryThrow<T1, T2, T3>(Action<T1, T2, T3> call, T1 arg1, T2 arg2, T3 arg3, int maxTries = 5)
        {
            List<Exception> exceptions = new List<Exception>();
            while (true)
            {
                try
                {
                    call(arg1, arg2, arg3);
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }
    }
}
