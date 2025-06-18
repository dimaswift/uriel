using System;

namespace Uriel.Utils
{
    public static class Id
    {
        public static string Short => Guid.NewGuid().ToString()[..5].ToUpper();
    }
}