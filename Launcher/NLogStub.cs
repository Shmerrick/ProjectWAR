namespace NLog
{
    public class Logger
    {
        private readonly string _name;
        public Logger(string name) => _name = name;
        public void Trace(string message) => System.Console.WriteLine($"TRACE [{_name}] {message}");
        public void Debug(string message) => System.Console.WriteLine($"DEBUG [{_name}] {message}");
        public void Info(string message) => System.Console.WriteLine($"INFO [{_name}] {message}");
        public void Warn(string message) => System.Console.WriteLine($"WARN [{_name}] {message}");
        public void Error(string message) => System.Console.WriteLine($"ERROR [{_name}] {message}");
    }

    public static class LogManager
    {
        public static Logger GetCurrentClassLogger()
        {
            var frame = new System.Diagnostics.StackFrame(1, false);
            var type = frame.GetMethod()?.DeclaringType;
            string name = type != null ? type.FullName ?? "Unknown" : "Unknown";
            return new Logger(name);
        }
    }
}
