using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using cons = System.Console;

namespace Bire.Console
{
   class Program
   {

      private static ConsoleColor
         _headerColor = ConsoleColor.DarkCyan,
         _titleColor = ConsoleColor.DarkCyan,
         _errorColor = ConsoleColor.Red,
         _completedColor = ConsoleColor.DarkGreen,
         _infoColor = ConsoleColor.Gray,
         _warningColor = ConsoleColor.DarkRed;

      static int Main(string[] consoleArgs)
      {

         WriteLine($" *** Bire v{Version} - Binary replace utility", _headerColor);
         cons.WriteLine();

         try
         {
            #region Initialize
            var usage = new Usage();
            var args = new Args(consoleArgs);
            if (!usage.ValidateArgs(args,s => cons.WriteLine(s)))
            {
               return -1;
            }
            var v = usage.GetArgValues(args);
            #endregion

            #region Validate arg values

            if (!Directory.Exists(v.Source) && !File.Exists(v.Source))
            {
               WriteLine($"source '{v.Source}' does not exists",_errorColor);
               return -1;
            }

            if (IsZip(v.Source) && IsZip(v.Target))
            {
               if (PathHelper.EqualPaths(v.Source, v.Target))
               {
                  WriteLine($"source and target are the same zip archive: {v.Source}",_errorColor);
                  return -1;
               }
            }

            if (v.ClearTarget)
            {

               if (Directory.Exists(v.Target))
               {

                  if (PathHelper.EqualPaths(v.Source, v.Target))
                  {
                     WriteLine($"you cannot clear the target folder {v.Target}, because it is the same folder as the source folder",_errorColor);
                     return -1;
                  }
                  Directory.Delete(v.Target, true);
               }
               else if (File.Exists(v.Target))
               {
                  File.Delete(v.Target);
               }
            }
            #endregion

            var boilerplate = IsZip(v.Source)
               ? (IBoilerplate)new ZipBoilerplate(v.Source, v.SkipExtensions)
               : new DirectoryBoilerplate(v.Source, v.IgnoreExpression, v.SkipExtensions);

            

            #region Manage replacements

            // try to intialize from fields in boilerplate (package-info.json/fields)
            var replacements = boilerplate.Info?.GetScaffoldReplacements()?.ToList();

            if (replacements == null)
            {
               // no boilerplateinfo.json
               replacements = v.Replacements?.Select(f => new FieldValuePrompt(f.Field, f.Value, null,null)).ToList() ?? new List<FieldValuePrompt>();
            }
            else if (v.Replacements != null)
            {
               // merge fields from command line, disable prompt for all occurances
               replacements.Merge(v.Replacements, true);
            }

            // replace any field placeholders, in all field values, with the corresponding preceding field values
            replacements.ResolvePlaceholders();

            var info = RemoveHtmlTags(boilerplate.Info?.ToString());
            if(info != null)
            {
               WriteLine(" Boilerplate info:",_titleColor);
               cons.WriteLine();
               cons.WriteLine("  "+info.Replace(Environment.NewLine, Environment.NewLine+"  "));
               cons.WriteLine();
            }


            foreach(var replacement in replacements.Where(f => f.Prompt != null || f.Value == null))
            {
               replacement.Prompt = replacement.Prompt?.TrimEnd(':',' ') ?? $"Enter value for {replacement.Field}";
            }

            var maxPrompLength = replacements
               .Where(field => field.Prompt != null)
               .Select(f => f.Prompt.Length)
               .OrderByDescending(x => x)
               .FirstOrDefault();

            var promptWritten = false;

            foreach (var replacement in replacements)
            {
               if (replacement.Prompt != null || replacement.Value == null)
               {
                  if (!promptWritten)
                  {
                     promptWritten = true;
                     WriteLine(" Please enter required scaffold fields:",_titleColor);
                     cons.WriteLine();
                  }
                  do
                  {
                     var prompt = $"  {replacement.Prompt}{new string(' ', maxPrompLength - replacement.Prompt.Length)}: ";
                     replacement.Value = LineEditor.ReadLine(prompt, replacement.Value);

                  } while (string.IsNullOrEmpty(replacement.Value));
               }
            }

            // resolve any placeholders left
            replacements.ResolvePlaceholders();

            if (!replacements.Any())
            {
               WriteLine($"Error: no replacements found in either argument -replace nor file {Constants.BoilerplateInfoFileName}",_errorColor);
               cons.WriteLine();
               cons.WriteLine(usage.GetUsage());
               return -1;
            }
            #endregion

            if (promptWritten)
            {
               cons.WriteLine();
            }
            WriteLine(" Scaffolding:",_titleColor);
            cons.WriteLine();

            var process = new Process(v.Source, v.Target, WriteLogItem, replacements, v.SkipExtensions != null && v.SkipExtensions.Any() ? v.SkipExtensions : null, v.IgnoreExpression);

            var sw = new Stopwatch();
            sw.Start();
            var ok = process.Execute();
            sw.Stop();

            if (!ok)
            {
               WriteLine($"{Environment.NewLine} completed with errors",_errorColor);
               return -1;
            }
            WriteLine($"{Environment.NewLine} ** Completed successfully in {sw.ElapsedMilliseconds}ms.", _completedColor);
            return 0;
         }

         catch (Exception ex)
         {
            WriteLine($"{Environment.NewLine} unexpected error: { ex.Message}",_errorColor);
            return -1;
         }
         finally
         {
#if DEBUG
            cons.Write($"{Environment.NewLine}Press any key to continue...");
            cons.ReadKey();
#endif
         }
      }

      static string Version => string.Join(".", Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3));


      private static bool IsZip(string filename)
      {
         return filename != null && Path.GetExtension(filename)?.ToLower() == ".zip";
      }

      private static void WriteLine(string text, ConsoleColor color)
      {
         cons.ForegroundColor = color;
         cons.WriteLine(text);
         cons.ResetColor();
      }

      private static void WriteLogItem(LogItem item)
      {
         switch (item.Type)
         {
            case LogItemType.Info:
               WriteLine(item.Message,_infoColor);
               break;
            case LogItemType.Warning:
               WriteLine(item.Message, _warningColor);
               break;
            case LogItemType.Error:
               WriteLine(item.Message, _errorColor);
               break;
            case LogItemType.Completed:
               break;
         }
      }

      public static string RemoveHtmlTags(string html)
      {
         if (string.IsNullOrWhiteSpace(html))
         {
            return html;
         }
         var text = Regex.Replace(html, "<(.|\n)*?>", String.Empty);
         return HttpUtility.HtmlDecode(text);
      }
   }
}

      

