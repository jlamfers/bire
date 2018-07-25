namespace Bire
{

   public class LogItem
   {
      public LogItem(string message, LogItemType type)
      {
         Message = message;
         Type = type;
      }
      public string Message { get; }
      public LogItemType Type { get; }

      public static LogItem Info(string message) => new LogItem(message, LogItemType.Info);
      public static LogItem Warning(string message) => new LogItem(message, LogItemType.Warning);
      public static LogItem Error(string message) => new LogItem(message, LogItemType.Error);
      public static LogItem Completed(string message) => new LogItem(message, LogItemType.Completed);

      public override string ToString()
      {
         return Type == LogItemType.Info || Type==LogItemType.Completed ? Message : $"{Type}: {Message}";
      }
   }
}
