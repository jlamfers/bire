using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bire.Console
{
   public class Args
   {
      public Args(string[] args)
      {
         AllArgs = args.ToList().AsReadOnly();

         var unnamedArgs = new List<string>();
         var namedArgs = new Dictionary<string, List<string>>();
         string key = null;
         foreach(var arg in args)
         {
            if (arg.StartsWith("-"))
            {
               key = arg;
               if (!namedArgs.ContainsKey(key))
               {
                  namedArgs.Add(key, new List<string>());
               }
               continue;
            }
            if(key != null)
            {
               namedArgs[key].Add(arg);
            }
            else
            {
               unnamedArgs.Add(arg);
            }
         }

         UnNamedArgs = unnamedArgs.AsReadOnly();
         NamedArgs = new ReadOnlyDictionary<string, IList<string>>(namedArgs.ToDictionary(i => i.Key, i => (IList<string>)i.Value.AsReadOnly()));

      }

      public IList<string> AllArgs { get; }
      public IList<string> UnNamedArgs { get; }
      public IDictionary<string,IList<string>> NamedArgs { get; }

      public IList<string> GetNamedArgs(string name)
      {
         IList<string> args;
         NamedArgs.TryGetValue(name, out args);
         return args;
      }

      public string this[string name]
      {
         get { return GetNamedArgs(name)?.FirstOrDefault(); }
      }

   }
}
