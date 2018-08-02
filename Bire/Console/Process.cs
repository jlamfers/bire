using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;


namespace Bire.Console
{
   public class Process
   {
      private readonly string _source;
      private readonly string _target;
      private readonly Action<LogItem> _logger;
      private readonly IEnumerable<FieldValue> _replacements;
      private readonly IList<string> _skipExtensions;
      private readonly string _ignoreExpression;
      private readonly bool _overwrite;

      public Process(string source, string target, Action<LogItem> logger, IEnumerable<FieldValue> replacements, IList<string> skipExtensions, string ignoreExpression, bool overwrite)
      {
         _source = source;
         _target = target;
         _logger = logger;
         _replacements = replacements;
         _skipExtensions = skipExtensions;
         _ignoreExpression = ignoreExpression;
         _overwrite = overwrite;
      }
      public Process(string source, Action<LogItem> logger, IEnumerable<FieldValue> replacements, IList<string> skipExtensions, string ignoreExpression, bool overwrite)
         : this(source,source,logger,replacements, skipExtensions, ignoreExpression, overwrite)
      {
        
      }

      public bool Execute()
      {
         if (IsZip(_source))
         {
            return IsZip(_target)
               ? FromZipToZip()
               : FromZipToFolder();
         }
         return IsZip(_target)
            ? FromFolderToZip()
            : FromFolderToFolder();
      }

      private bool FromZipToZip()
      {
         var package = new ZipBoilerplate(_source, _skipExtensions);
         return package.BuildScaffold(_target, _replacements, _logger);
      }

      private bool FromFolderToFolder()
      {
         var package = new DirectoryBoilerplate(_source, _ignoreExpression, _skipExtensions);
         return package.BuildScaffold(_target, _replacements, _logger);
      }

      private bool FromFolderToZip()
      {
         var package = new DirectoryBoilerplate(_source, _ignoreExpression, _skipExtensions);
         var tempFolderName = PathHelper.GetTempName();
         Directory.CreateDirectory(tempFolderName);
         if(package.BuildScaffold(tempFolderName, _replacements, _logger))
         {
            try
            {
               _logger(LogItem.Info("packing scaffold..."));
               if (_overwrite && File.Exists(_target))
               {
                  File.Delete(_target);

               }
               ZipFile.CreateFromDirectory(tempFolderName, _target);
               return true;
            }
            finally
            {
               try
               {
                  Directory.Delete(tempFolderName,true);
               }
               catch { }
            }
         }
         return false;
      }

      private bool FromZipToFolder()
      {
         var package = new ZipBoilerplate(_source, _skipExtensions);
         var tempFileName = PathHelper.GetTempName()+".zip";
         if (package.BuildScaffold(tempFileName, _replacements, _logger))
         {
            try
            {
               _logger(LogItem.Info("creating folder..."));
               if (!_overwrite)
               {
                  ZipFile.ExtractToDirectory(tempFileName, _target);
               }
               else
               {
                  using (var archive = ZipFile.OpenRead(tempFileName))
                  {
                     archive.ExtractToDirectory(_target, true);
                  }
               }
               return true;
            }
            finally
            {
               try
               {
                  File.Delete(tempFileName);
               }
               catch { }
            }
         }
         return false;
      }

      private static bool IsZip(string filename)
      {
         return filename != null && Path.GetExtension(filename).ToLower() == ".zip";
      }
   }
}
