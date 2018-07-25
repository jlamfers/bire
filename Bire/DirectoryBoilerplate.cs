using Bire;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bire
{
   public class DirectoryBoilerplate : IBoilerplate
   {
     

      private string 
         _sourceFolder;
      private HashSet<string> 
         _skipExtensions;
      private readonly Regex 
         _ignoreExpression;

      public DirectoryBoilerplate(string sourceFolder, string ignoreExpression, IList<string> skipExtensions = null)
      {
         if (sourceFolder == null) throw new ArgumentNullException(nameof(sourceFolder));

         _sourceFolder = new DirectoryInfo(sourceFolder).FullName;

         if (!Directory.Exists(_sourceFolder))
         {
            throw new Exception($"source directory {sourceFolder} not found.");
         }

         var boilerplateInfoFile = Directory.GetFiles(sourceFolder, Constants.BoilerplateInfoFileName, SearchOption.AllDirectories).FirstOrDefault();
         if(boilerplateInfoFile != null)
         {
            using(var stream = new FileStream(boilerplateInfoFile,FileMode.Open))
            {
               Info = BoilerplateInfo.FromStream(stream);
            }
         }

         _skipExtensions = new HashSet<string>(skipExtensions ?? Info?.SkipExtensions ?? Constants.DefaultSkipExtensions, StringComparer.OrdinalIgnoreCase);
         _ignoreExpression = new Regex(ignoreExpression ?? Info?.IgnoreExpression ?? Constants.IgnoreGitExpression, RegexOptions.IgnoreCase | RegexOptions.Compiled);

      }

      public BoilerplateInfo Info { get; }
 
      public bool BuildScaffold(string targetFolder, IEnumerable<FieldValue> replacements, Action<LogItem> logger = null)
      {

         if (targetFolder == null) throw new ArgumentNullException(nameof(targetFolder));

         var loggerEx = logger == null
                        ? new Action<Exception, LogItem>((e, i) => { if (e != null) throw e; })
                        : (e, i) => { logger(i); };

         logger = logger ?? (s => { });


         targetFolder = new DirectoryInfo(targetFolder).FullName;

         try
         {
            var fields = Info?.GetScaffoldReplacements() ?? new List<FieldValuePrompt>();
            fields.Merge(replacements?.ToList(), true);
            fields.ResolvePlaceholders();

            fields.MatchAllCases();

            var workingFolder = _sourceFolder;
            if (!PathHelper.EqualPaths(workingFolder, targetFolder))
            {
               PathHelper.CopyDirectory(new DirectoryInfo(workingFolder), new DirectoryInfo(targetFolder), s => !Ignore(s));
               var boilerplateInfoFile = Directory.GetFiles(targetFolder, Constants.BoilerplateInfoFileName, SearchOption.AllDirectories).FirstOrDefault();
               if(boilerplateInfoFile != null)
               {
                  File.Delete(boilerplateInfoFile);
               }
               workingFolder = targetFolder;
            }

            MoveFiles(workingFolder, fields, logger);
            MoveDirectories(workingFolder, fields, logger);
            ProcessContent(workingFolder, fields, logger);

            loggerEx(null, LogItem.Completed("build completed"));
            return true;

         }
         catch(Exception ex)
         {
            var sourceDirInfo = new DirectoryInfo(_sourceFolder);
            var targetDirInfo = new DirectoryInfo(targetFolder);
            if (targetDirInfo.Exists && !PathHelper.EqualPaths(_sourceFolder,targetFolder))
            {
               try
               {
                  targetDirInfo.Delete(true);
               }
               catch { }
            }
            loggerEx(ex,LogItem.Error(ex.Message));
            return false;
         }
      }
 
      private void MoveFiles(string baseFolder, IEnumerable<FieldValue> replacements, Action<LogItem> logger)
      {
         foreach (var replacement in replacements)
         {
            var oldText = replacement.Field;
            var newText = replacement.Value;
            //var files = Directory.GetFiles(baseFolder, "*.*", SearchOption.AllDirectories);
            var files = baseFolder.GetFilesByFilter(s => !Ignore(s));
            foreach (var file in files.OrderByDescending(f => f.Count(c => c == '/' || c == '\\')))
            {
               if (Path.GetFileName(file).Contains(oldText))
               {
                  var newFile = Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(file).Replace(oldText, newText));
                  try
                  {
                     File.Move(file, newFile);
                     logger(LogItem.Info($" renamed {Path.GetFileName(file)} => {Path.GetFileName(newFile)}"));
                  }
                  catch 
                  {
                     logger(LogItem.Error($" could not rename {"." + Path.GetFileName(file)} to {"." + Path.GetFileName(newFile)}"));
                     throw;
                  }
               }
            }
         }
      }
      private void MoveDirectories(string baseFolder, IEnumerable<FieldValue> replacements, Action<LogItem> logger, HashSet<string> handled = null, string startFolder = null)
      {
         handled = handled ?? new HashSet<string>();
         startFolder = startFolder ?? baseFolder;
         if (handled.Contains(baseFolder))
         {
            return;
         }
         handled.Add(baseFolder);

         if (Ignore(baseFolder.Substring(startFolder.Length)))
         {
            logger(LogItem.Info($" ignored {Path.GetFileName(baseFolder)}"));
            return;
         }

         foreach (var replacement in replacements)
         {
            var oldText = replacement.Field;
            var newText = replacement.Value;
            foreach (var directory in Directory.GetDirectories(baseFolder, "*.*", SearchOption.TopDirectoryOnly))
            {

               if (Path.GetFileName(directory).Contains(oldText))
               {
                  var newDirectory = Path.Combine(Path.GetDirectoryName(directory), Path.GetFileName(directory).Replace(oldText, newText));
                  try
                  {
                     Directory.Move(directory, newDirectory);
                     logger(LogItem.Info($" renamed {Path.GetFileName(directory)}/ => {Path.GetFileName(newDirectory)}/"));
                     MoveDirectories(newDirectory, replacements, logger, handled, startFolder);
                  }
                  catch 
                  {
                     logger(LogItem.Error($" renamed {directory.Substring(baseFolder.Length)}/ => {newDirectory.Substring(baseFolder.Length)}/"));
                     throw;
                  }
               }
               else
               {
                  MoveDirectories(directory, replacements, logger, handled, startFolder);
               }
            }
         }
      }
      private void ProcessContent(string baseFolder, IEnumerable<FieldValue> replacements, Action<LogItem> logger)
      {
         var files = baseFolder.GetFilesByFilter(s => !Ignore(s));

         foreach (var inputfileName in files)
         {
            var extension = Path.GetExtension(inputfileName);
            if (_skipExtensions.Contains(extension))
            {
               continue;
            }

            var segements = inputfileName.Substring(baseFolder.Length).Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (segements.Any(s => Ignore(s)))
            {
               logger(LogItem.Info($" ignored {Path.GetFileName(inputfileName)}"));
               continue;
            }


            var encoding = GetEncoding(inputfileName);

            var tempFileName = PathHelper.GetTempName();
            var output = new FileStream(tempFileName, FileMode.CreateNew);
            try
            {
               using (var input = new FileStream(inputfileName, FileMode.Open))
               {
                  if (ByteReplacer.TryReplace(input, output, replacements.Select(r => Tuple.Create(encoding.GetBytes(r.Field), encoding.GetBytes(r.Value)))))
                  {
                     input.Close();
                     output.Close();
                     File.Delete(inputfileName);
                     File.Move(tempFileName, inputfileName);
                     tempFileName = PathHelper.GetTempName();
                     output = new FileStream(tempFileName, FileMode.CreateNew);
                     logger(LogItem.Info($"   modified {Path.GetFileName(inputfileName)}"));

                  }
                  else {
                     //output.Flush();//??
                     output.Seek(0L, SeekOrigin.Begin);
                  }

               }
            }
            finally
            {
               if(output != null)
               {
                  output.Close();
                  File.Delete(tempFileName);
                  output = null;
               }
            }

         }
      }

      private static Encoding GetEncoding(string file)
      {
         using (var reader = new StreamReader(file, Encoding.Default, true))
         {
            reader.Peek();
            return reader.CurrentEncoding;
         }
      }

      private bool Ignore(string path)
      {
         return path == null || (_ignoreExpression != null && _ignoreExpression.IsMatch(path));
      }


   }


}
