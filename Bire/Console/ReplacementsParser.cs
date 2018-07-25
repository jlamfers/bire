using Bire;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bire.Console
{
   static class ReplacementsParser
   {
      public static IList<FieldValue> Parse(params string[] args)
      {
         if (args == null) return null;

         var result = new List<FieldValue>();
         var field = new StringBuilder();
         var value = new StringBuilder();

         foreach (var arg in args)
         {
            field.Length = 0;
            value.Length = 0;
            var current = field;
            var escape = false;
            foreach (var ch in arg)
            {
               if (escape)
               {
                  current.Append(ch);
                  escape = false;
                  continue;
               }
               switch (ch)
               {
                  case '\\':
                     escape = true;
                     continue;
                  case '=':
                     if (current == value)
                     {
                        throw Error(args);
                     }
                     current = value;
                     continue;
                  case ',':
                     if (current == field)
                     {
                        throw Error(args);
                     }
                     result.Add(new FieldValue(field.ToString(), value.ToString()));
                     field.Length = 0;
                     value.Length = 0;
                     current = field;
                     continue;
                  default:
                     current.Append(ch);
                     continue;
               }

            }
            if (field.Length > 0)
            {
               if (current == field)
               {
                  throw Error(args);
               }
               result.Add(new FieldValue(field.ToString(), value.ToString()));

            }
         }
         return result;
      }

      private static Exception Error(string[] args)
      {
         return new Exception($"Invalid -replace argument: {string.Join(",", args)}");
      }
   }
}
