using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bire
{
   public class ZipBoilerplate : IBoilerplate
   {
      private readonly string 
         _sourceZipName;

      
      private readonly HashSet<string> 
         _skipExtensions;

      public ZipBoilerplate(string sourceZipName, IList<string> skipExtensions = null)
      {
         if (sourceZipName == null) throw new ArgumentNullException(nameof(sourceZipName));

         _sourceZipName = new FileInfo(sourceZipName).FullName;


         if (!File.Exists(_sourceZipName))
         {
            throw new FileNotFoundException("package not found", _sourceZipName);
         }

         try
         {
            using (var zip = ZipFile.OpenRead(sourceZipName))
            {
               foreach (var entry in zip.Entries)
               {
                  if (entry.Name.EqualsOrdinalIgnoreCase(Constants.BoilerplateInfoFileName))
                  {
                     Info = BoilerplateInfo.FromStream(entry.Open());
                     using (var reader = new StreamReader(entry.Open()))
                     {
                        BoilerplateInfoJson = reader.ReadToEnd();
                     }
                        
                  }

               }
            }
         }
         catch(Exception ex)
         {
#if DEBUG
            throw new Exception($"invalid ZIP package: {ex.ToString()}",ex);
#else
            throw new Exception($"invalid ZIP package: {ex.Message}",ex);
#endif
         }

         _skipExtensions = new HashSet<string>(skipExtensions ?? Info?.SkipExtensions ?? Constants.DefaultSkipExtensions, StringComparer.OrdinalIgnoreCase);
      }

      public BoilerplateInfo Info { get; }

      public string BoilerplateInfoJson { get; }

      public string FileName
      {
         get { return _sourceZipName; }
      }

      public bool BuildScaffold(string targetZipName, IEnumerable<FieldValue> replacements, Action<LogItem> logger = null)
      {


         var loggerEx = logger == null 
            ? new Action<Exception, LogItem>((e, i) => { if(e != null) throw e; }) 
            : (e, i) => { logger(i); };

         var fields = Info?.GetScaffoldReplacements() ?? new List<FieldValuePrompt>();
         fields.Merge(replacements?.ToList(), true);
         fields.ResolvePlaceholders();

         fields.MatchAllCases();

         try
         {
            using (var zipIn = ZipFile.OpenRead(_sourceZipName))
            {
               using (var zipOut = ZipFile.Open(targetZipName, ZipArchiveMode.Create))
               {
                  foreach (var entry in zipIn.Entries)
                  {
                     if(entry.Name.EqualsOrdinalIgnoreCase(Constants.BoilerplateInfoFileName))
                     {
                        // skip package info
                        continue;
                     }

                     var fullname = entry.FullName;

                     foreach (var r in fields)
                     {
                        fullname = fullname.Replace(r.Field, r.Value);
                     }

                     if(GetEntryShortName(entry.FullName) != GetEntryShortName(fullname))
                     {
                        loggerEx(null,LogItem.Info($" renamed {GetEntryShortName(entry.FullName)} => {GetEntryShortName(fullname)}"));
                     }
      

                     if(entry.Length == 0)
                     {
                        // create directory
                        zipOut.CreateEntry(fullname, CompressionLevel.Fastest);
                        continue;
                     }

                     // find encoding
                     Encoding encoding;
                     using (var stream = entry.Open())
                     {
                        using (var reader = new StreamReader(stream))
                        {
                           reader.Peek();
                           encoding = reader.CurrentEncoding;
                        }
                     }

                     // replace bytes
                     using (var inStream = entry.Open())
                     {
                        using (var outStream = zipOut.CreateEntry(fullname, CompressionLevel.Optimal).Open())
                        {
                           if (_skipExtensions.Contains(Path.GetExtension(fullname)))
                           {
                              CopyStream(inStream, outStream);
                           }
                           else if (ByteReplacer.TryReplace(inStream, outStream, fields.Select(x => Tuple.Create(encoding.GetBytes(x.Field), encoding.GetBytes(x.Value)))))
                           {
                              loggerEx(null,LogItem.Info($"   modified {GetEntryShortName(fullname)}"));
                           }
                        }
                     }
                  }
               }
            }
            loggerEx(null,LogItem.Completed("build completed"));
            return true;
         }
         catch(Exception ex)
         {
            try
            {
               File.Delete(targetZipName);
            }
            catch { }
            loggerEx(ex, LogItem.Error(ex.Message));
            return false;
         }
         
      }

      private static string GetEntryShortName(string fullname)
      {
         if(fullname.EndsWith("/") || fullname.EndsWith("\\"))
         {
            return new DirectoryInfo(fullname).Name + "/";
         }
         return Path.GetFileName(fullname);
      }


      byte[] _copyBuf = new byte[4096];
      private void CopyStream(Stream source, Stream target)
      {
         int count;
         while ((count = source.Read(_copyBuf, 0, _copyBuf.Length)) != 0)
         {
            target.Write(_copyBuf, 0, count);
         }
      }

   }
}
