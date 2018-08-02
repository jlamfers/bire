using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bire
{
   public static class Extensions
   {

      public static void ResolvePlaceholders<T>(this IList<T> fields)
         where T : FieldValue
      {
         for (var i = 0; i < fields.Count; i++)
         {
            var f = fields[i];
            for (var j = i + 1; j < fields.Count; j++)
            {
               var fj = fields[j];
               if (fj.Value != null && f.Value != null)
               {
                  fj.Value = fj.Value.Replace(f.Field, f.Value);
               }
            }
         }
      }

      public static void EvaluatePlaceholderExpressions<T>(this IList<T> fields)
         where T : FieldValue
      {
         foreach (var field in fields)
         {
            var v = field.Value.Trim();
            if (!v.StartsWith("#")) continue;
            if (v.StartsWith("##"))
            {
               v = v.Substring(1);
            }
            else if (v.StartsWith("#lower("))
            {
               v = v.Substring(7, v.Length - 8).Trim().ToLower();
            }
            else if (v.StartsWith("#upper("))
            {
               v = v.Substring(7, v.Length - 8).Trim().ToUpper();
            }
            else if (v.StartsWith("#dashed("))
            {
               v = v.Substring(8, v.Length - 9).Trim().Dashed();
            }
            else
            {
               throw new Exception($"Invalid expressions: {field.Value}");
            }
            field.Value = v;
         }
      }

      public static void Merge<T>(this IList<T> self, IList<FieldValue> fields, bool resetPrompt = false)
         where T : FieldValue
      {
         if(self == null)
         {
            throw new ArgumentNullException(nameof(fields));
         }

         if(fields == null || !fields.Any())
         {
            return;
         }

         foreach(var field in fields)
         {
            var match = self.Where(x => x.Field == field.Field).FirstOrDefault();
            if (match != null)
            {
               match.Value = field.Value;
               if (resetPrompt)
               {
                  match.CastTo<FieldValuePrompt>().Prompt = null;
               }
            }
            else
            {
               if (typeof(FieldValuePrompt).IsAssignableFrom(typeof(T)))
               {
                  self.Add(new FieldValuePrompt(field.Field, field.Value, null,null).CastTo<T>());
               }
               else
               {
                  self.Add(new FieldValue(field.Field, field.Value).CastTo<T>());
               }
            }
         }
      }

      public static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T)self;
      }

      public static bool EqualsOrdinalIgnoreCase(this string self, string other)
      {
         if (self == null || other == null) return self == null && other == null;
         return string.Compare(self, other, StringComparison.OrdinalIgnoreCase) == 0;
      }

      internal static void MatchAllCases(this IList<FieldValuePrompt> self)
      {
         foreach (var f in self.ToList())
         {
            self.Add(new FieldValuePrompt($"#lower({f.Field})", f.Value?.ToLower(), null, null));
            self.Add(new FieldValuePrompt($"#upper({f.Field})", f.Value?.ToUpper(), null, null));
            self.Add(new FieldValuePrompt($"#dashed({f.Field})", f.Value?.Dashed(), null, null));
         }
      }

      public static string Dashed(this string self)
      {
         if (string.IsNullOrWhiteSpace(self)) return self;
         if (self.Contains("."))
         {
            return self.Trim().ToLower().Replace(".", "-");
         }
         return string
            .Concat(self.Trim()
            .Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + char.ToLower(x).ToString() : (i==0 ? char.ToLower(x).ToString() : x.ToString())));
      }

      public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
      {
         if (!overwrite || !Directory.Exists(destinationDirectoryName))
         {
            archive.ExtractToDirectory(destinationDirectoryName);
            return;
         }
         foreach (ZipArchiveEntry file in archive.Entries)
         {
            string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
            if (string.IsNullOrEmpty(file.Name))
            {
               // Assuming Empty for Directory
               var directoryName = Path.GetDirectoryName(completeFileName);
               if (!Directory.Exists(directoryName))
               {
                  Directory.CreateDirectory(directoryName);
               }
               continue;
            }
            file.ExtractToFile(completeFileName, true);
         }
      }

   }
}
