using System;

namespace FrameWork
{
    public class DatabaseException : ApplicationException
    {
        // Exception raised by the database
        public DatabaseException(Exception e)
            : base("", e)
        {
        }

        // Lip an exception with the error message
        public DatabaseException(string str, Exception e)
            : base(str, e)
        {
        }

        // Reasons for the exeption
        public DatabaseException(string str)
            : base(str)
        {
        }
    }
}