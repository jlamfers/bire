using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bire.Console
{
   public class ArgValues
   {
      public string Source { get; set; }
      public string Target { get; set; }
      public IList<string> SkipExtensions { get; set; }
      public string IgnoreExpression { get; set; }
      public bool ClearTarget { get; set; }
      public bool Overwrite { get; set; }
      public IList<FieldValue> Replacements { get; set; }
   }
}
