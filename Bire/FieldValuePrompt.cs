namespace Bire
{
   public class FieldValuePrompt : FieldValue
   {
      public FieldValuePrompt(string field, string value, string prompt, string description): base(field,value)
      {
         Prompt = prompt;
         Description = description;
      }

      public string Prompt { get; set; }
      public string Description { get; set; }

   }
}
