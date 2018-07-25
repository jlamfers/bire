using System;
using System.Collections.Generic;

namespace Bire
{
   public interface IBoilerplate
   {
      BoilerplateInfo Info { get; }

      bool BuildScaffold(string target, IEnumerable<FieldValue> replacements, Action<LogItem> logger = null);
   }
}