using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bire
{
   public static class PathHelper
   {

      public static bool EqualPaths(string path1, string path2)
      {
         if (path1 == null || path2 == null) return false;
         return NormalizePath(path1) == NormalizePath(path2);
      }
      public static string NormalizePath(string path)
      {
         path = Path.GetFullPath(path);
         return Path.GetFullPath(new Uri(path).LocalPath)
                    .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
         //TODO: check OS case sensitivity?

      }

      public static string GetTempPath()
      {
         var tempPath = Path.Combine(Path.GetTempPath(), "__bire");
         if (!Directory.Exists(tempPath))
         {
            Directory.CreateDirectory(tempPath);
         }
         return tempPath;
      }

      public static string GetTempName()
      {
         return Path.Combine(GetTempPath(), Guid.NewGuid().ToString());
      }


      public static void CopyDirectory(string source, string target, Func<string, bool> filter)
      {
         CopyDirectory(new DirectoryInfo(source), new DirectoryInfo(target),filter);
      }
      public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, Func<string, bool> filter, string baseFolder = null)
      {
         baseFolder = baseFolder ?? source.FullName;
         foreach (DirectoryInfo dir in source.GetDirectories().Where(d => filter(d.FullName.Substring(baseFolder.Length))))
            CopyDirectory(dir, target.CreateSubdirectory(dir.Name),filter,baseFolder);
         foreach (FileInfo file in source.GetFiles().Where(d => filter(d.FullName.Substring(baseFolder.Length))))
            file.CopyTo(Path.Combine(target.FullName, file.Name));
      }

      public static IList<string> GetFilesByFilter(this string directory, Func<string, bool> filter)
      {
         return GetFilesByFilter(new DirectoryInfo(directory), filter);
      }
      public static IList<string> GetFilesByFilter(this DirectoryInfo self, Func<string,bool> filter, List<string> files = null, string baseFolder = null)
      {
         files = files ?? new List<string>();
         baseFolder = (baseFolder ?? self.FullName).TrimEnd(new[] { '\\', '/' });
         foreach (DirectoryInfo dir in self.GetDirectories().Where(d => filter(d.FullName.Substring(baseFolder.Length))))
            GetFilesByFilter(dir,filter,files,baseFolder);
         foreach (FileInfo file in self.GetFiles().Where(d => filter(d.FullName.Substring(baseFolder.Length))))
            files.Add(file.FullName);
         return files;
      }
   }
}
