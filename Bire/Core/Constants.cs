using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bire
{
   public static class Constants
   {
      public const string BoilerplateInfoFileName = "boilerplateinfo.json";
      public const string DefaultIgnoreFilesExpression = @"(.*(\.|\/|\\)(exe|dll|obj|bin|pdb|zip|\.git|\.vs|cache|packages))$";
      public const string IgnoreGitExpression = @".*(\.|\/|\\)git$";
      public static readonly IList<string> DefaultSkipExtensions = new[] { ".exe", ".dll", ".obj", ".pdb", ".zip", ".nupkg" }.ToList().AsReadOnly();
   }
}
