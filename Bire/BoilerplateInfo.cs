using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;

namespace Bire
{
   public class BoilerplateInfo
   {
      private IDictionary<string, object> _json;
      private IList<FieldValuePrompt> _replacements;

      public static BoilerplateInfo FromFile(string filename)
      {
         return new BoilerplateInfo((IDictionary<string, object>)new JsonParser().ParseFile(filename));
      }
      public static BoilerplateInfo FromJson(IDictionary<string, object> json)
      {
         return new BoilerplateInfo(json);
      }
      public static BoilerplateInfo FromJson(string json)
      {
         return new BoilerplateInfo((IDictionary<string, object>)new JsonParser().ParseContent(json));
      }
      public static BoilerplateInfo FromStream(Stream stream, bool leaveOpen = false)
      {
         return new BoilerplateInfo((IDictionary<string, object>)new JsonParser().Parse(stream,leaveOpen: leaveOpen));
      }

      public BoilerplateInfo(IDictionary<string, object> json)
      {
         _json = json;
         Id = this["id"]?.ToString();
         Author = this["author"]?.ToString();
         Title = this["title"]?.ToString();
         Description = this["description"]?.ToString();

         var ignore = this["ignore"]?.ToString();
         if(ignore == "default")
         {
            ignore = Constants.DefaultIgnoreFilesExpression;
         }
         IgnoreExpression = ignore;

         SkipExtensions = this["skipExtensions"]
            .CastTo<IList<object>>()
            ?.Cast<string>()
            .ToList()
            .AsReadOnly();

         CreatedAt = this["createdAt"]?.ToString();

         Type = this["type"]?.ToString();


         Comment = _json.TryGetValue("comment", out object value) && value is IList<object> array
               ? string.Join(Environment.NewLine, array.Select(i => i?.ToString() ?? ""))
               : null;

         if(!_json.TryGetValue("fields", out object value2))
         {
            // allow "fields","replace" and "scaffold" for the same target
            if (!_json.TryGetValue("replace", out value2)) {
               _json.TryGetValue("scaffold", out value2);
            }
         }
         _replacements = value2 is IDictionary<string, object> dict
               ? dict.Select(ToFieldPromptValue).ToList().AsReadOnly()
               : new ReadOnlyCollection<FieldValuePrompt>(new FieldValuePrompt[0]);

 
         if (string.IsNullOrWhiteSpace(Comment)) Comment = null;

      }

      public string Id { get; }
      public string Author { get; }
      public string Title { get; }
      public string Description { get; }
      public string Comment { get; }
      public string Type { get; }

      public IList<string> SkipExtensions { get; }
      public string CreatedAt { get; }
      public string IgnoreExpression { get; }

      public IList<FieldValuePrompt> GetScaffoldReplacements()
      {
         return _replacements.Select(i => new FieldValuePrompt(i.Field, SubstituteApiPlaceholders(i.Value),i.Prompt, i.Description)).ToList();
      }

      public object this[string name]
      {
         get
         {
            return _json.TryGetValue(name, out object value) ? value : null;
         }
      }

      private FieldValuePrompt ToFieldPromptValue(KeyValuePair<string, object> pair)
      {
         if (pair.Value is string)
         {
            return new FieldValuePrompt(pair.Key, (string)pair.Value, null,null);
         }
         var dict = (IDictionary<string, object>)pair.Value;
         dict.TryGetValue("value", out object value);
         dict.TryGetValue("prompt", out object prompt);
         dict.TryGetValue("description", out object description);
         return new FieldValuePrompt(pair.Key, value?.ToString(), prompt?.ToString(), description?.ToString());
      }

      private string SubstituteApiPlaceholders(string value)
      {
         if (string.IsNullOrEmpty(value) || value[0] != '#') return value;
         if (value.StartsWith("##")) return value.Substring(1);
         switch (value.TrimEnd())
         {

            case "#username()":
               return  HttpContext.Current?.User?.Identity?.Name ?? Environment.UserName;
            case "#today()":
               return DateTime.Today.ToString("dd-MM-yyyy");
            case "#now()":
               return DateTime.Now.ToString("dd-MM-yyyy HH:mm");
            case "#timestamp()":
               return DateTime.Now.ToString("o");
            case "#year()":
               return DateTime.Today.Year.ToString();
            case "#month()":
               return DateTime.Today.Month.ToString("D2");
            case "#guid()":
               return Guid.NewGuid().ToString("D");
            default:
               
               return value;
         }
      }

      public override string ToString()
      {
         var nl = Environment.NewLine;

         return $"{Title} - (c) {Author}{nl}{Description}" + (Comment != null ? $"{nl}{nl}{Comment}":"");
      }
   }


}

