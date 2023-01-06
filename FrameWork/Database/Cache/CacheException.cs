using System;

namespace FrameWork
{
    public class CacheException : DatabaseException
    {
        // Construit un exeption levée par le cache
        public CacheException(string s)
            : base(s)
        {
        }

        // Description de l'exeption levée
        public CacheException(Exception e)
            : base(e)
        {
        }
    }
}