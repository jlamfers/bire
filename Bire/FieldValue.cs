
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bire
{
   public class FieldValue
   {
      public FieldValue(string field, string value)
      {
         if (field == null) throw new ArgumentNullException(nameof(field));
         Field = field;
         Value = value;
      }
      public string Field { get; }
      public string Value { get; set; }

 


   }
}
