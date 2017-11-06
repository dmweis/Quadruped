using System;
using System.Collections.Generic;

namespace Quadruped.Driver
{
    public static class ExceptionHelper
    {
        public static T RepeatCatch<T>(Func<T> call, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    return call();
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return default(T);
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T2 RepeatCatch<T1, T2>(Func<T1, T2> call, T1 arg1, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    return call(arg1);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return default(T2);
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T3 RepeatCatch<T1, T2, T3>(Func<T1, T2, T3> call, T1 arg1, T2 arg2, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    return call(arg1, arg2);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return default(T3);
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T4 RepeatCatch<T1, T2, T3, T4>(Func<T1, T2, T3, T4> call, T1 arg1, T2 arg2, T3 arg3, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    return call(arg1, arg2, arg3);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return default(T4);
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static T5 RepeatCatch<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> call, T1 arg1, T2 arg2, T3 arg3, T4 arg4, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    return call(arg1, arg2, arg3, arg4);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return default(T5);
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void RepeatCatch(Action call, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    call();
                    return;
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return;
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void RepeatCatch<T>(Action<T> call, T arg1, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    call(arg1);
                    return;
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return;
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void RepeatCatch<T1, T2>(Action<T1, T2> call, T1 arg1, T2 arg2, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    call(arg1, arg2);
                    return;
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return;
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }

        public static void RepeatCatch<T1, T2, T3>(Action<T1, T2, T3> call, T1 arg1, T2 arg2, T3 arg3, int maxTries = 5, bool swallow = false)
        {
            List<Exception> exceptions = null;
            while (true)
            {
                try
                {
                    call(arg1, arg2, arg3);
                    return;
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (exceptions.Count >= maxTries)
                    {
                        if (swallow)
                        {
                            return;
                        }
                        throw new AggregateException(exceptions.ToArray());
                    }
                }
            }
        }
    }
}
