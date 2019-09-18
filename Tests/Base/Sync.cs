using System;
using System.Threading;

public static class Sync
{
    public static bool WaitFor(Func<bool> predicate, int timeout = 5000)
    {
        while (!predicate() && timeout > 0)
        {
            Thread.Sleep(30);
            timeout -= 30;
        }

        return predicate();
    }
}
