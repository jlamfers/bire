using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bire.Console
{
   public class Usage
   {
      public string GetUsage()
      {
         using(var w = new StringWriter())
         {
            return $@"
 usage: bire -from <directory-or-zip> [-to <target-directory-or-zip>][-replace<fields>]
             [-clearTarget] [-skip <skip-extensions>] [-ignore <default | <ignore-regex>>]

    or: bire -scaffold <directory> [-to <zip-target>] [-ignore <ignore-regex>]
 
 <directory-or-zip>        : existing directory or zip-file. it may contain a 
                             file named boilerplateinfo.json
 <target-directory-or-zip> : target directory or zip-file. it is newly created
 <fields>                  : space separated field-value pairs like {{<fieldname>=<fieldvalue>}}
                             all of these fields either override or fill up configured fields 
                             in boilerplateinfo.json
 -clearTarget              : target is cleared before build
 <skip-extensions>         : space separated file extensions (dot included like .exe) that do not 
                             need content processing but must be copied. Default is 
                             {string.Join(", ", Constants.DefaultSkipExtensions)}
 <ignore-regex>            : regex that matches all file and directory names that must be ignored, 
                             and not copied
 -ignore default           : equals ""-ignore {Constants.DefaultIgnoreFilesExpression}""

 -scaffold                 : when using -scaffold the source must be a directory and the optional
                             target must be a zip. When omitted then <target> gets the same name
                             as <directory> with extension .zip. <directory> must include the file 
                             boilerplateinfo.json
            ";
         }
      }

      public bool ValidateArgs(Args args, Action<string> errorLineWriter)
      {
         if (args["-scaffold"] != null)
         {
            if (args.NamedArgs.Where(kv => !new[] { "-to", "-scaffold", "-ignore" }.Contains(kv.Key)).Any())
            {
               errorLineWriter("invalid args");
               errorLineWriter(GetUsage());
               return false;
            }
            if (!Directory.Exists(args["-scaffold"]))
            {
               errorLineWriter($"invalid source directory: {args["-scaffold"]}");
               return false;
            }
            return true;
         }

         var allowedArgNames = new HashSet<string>(new[] { "-from", "-to", "-replace", "-clearTarget", "-skip", "-ignore", "-scaffold" });

         var unknownArgs = args.NamedArgs.Where(kv => !allowedArgNames.Contains(kv.Key)).ToList();


         if (args["-from"] == null || unknownArgs.Any() || args.UnNamedArgs.Any())
         {
            if (unknownArgs.Any())
            {
               errorLineWriter($" unknown args: {string.Join(", ", unknownArgs.Select(x => x.Key).ToArray())}");
               errorLineWriter("");
            }
            errorLineWriter(GetUsage());

            return false;
         }
         return true;
      }

      public ArgValues GetArgValues(Args args)
      {
         var values = new ArgValues();

         values.Source = args["-from"];
         values.Target = args["-to"] ?? values.Source;

         values.Replacements = ReplacementsParser.Parse(args.GetNamedArgs("-replace")?.ToArray());
         values.SkipExtensions = args.GetNamedArgs("-skip")?.ToList();
         values.ClearTarget = args.GetNamedArgs("-clearTarget") != null;
         values.IgnoreExpression = args.GetNamedArgs("-ignore")?.FirstOrDefault();
         if (values.IgnoreExpression == "default")
         {
            values.IgnoreExpression = Constants.DefaultIgnoreFilesExpression;
         }

         if (args["-scaffold"] != null)
         {
            values.Source = args["-scaffold"];
            values.Target = values.Target ?? values.Source;
            if (!values.Target.ToLower().EndsWith(".zip"))
            {
               values.Target += ".zip";
            }
            values.ClearTarget = true;
            values.IgnoreExpression = values.IgnoreExpression ?? Constants.DefaultIgnoreFilesExpression;
         }

         return values;
      }
   }
}
