using System;
using System.Collections.Generic;
using System.IO;

namespace Bire
{

   class ByteReplacer
   {

      public static bool TryReplace(ref byte[] bytes, IEnumerable<Tuple<byte[], byte[]>> replacements)
      {
         using (var inputStream = new MemoryStream(bytes))
         {
            using (var outputStream = new MemoryStream())
            {
               if (TryReplace(inputStream, outputStream, replacements))
               {
                  bytes = new byte[outputStream.Length];
                  Buffer.BlockCopy(outputStream.GetBuffer(), 0, bytes, 0, bytes.Length);
                  return true;
               }
               return false;
            }
         }
      }

      public static bool TryReplace(Stream input, Stream output, IEnumerable<Tuple<byte[], byte[]>> replacements)
      {

         // tuple.Item1 => pattern
         // tuple.Item2 => value (replacement)
         var modified = false;
         var matcher = new Matcher(replacements);
         var writer = new BinaryWriter(output);
         var reader = new StreamArray(input);
         var matchIndex = 0;
         for (var i = 0; !reader.Eof || i < reader.FetchedLength; i++)
         {
            if (matcher.IsMatching(reader[i], matchIndex))
            {
               if (matcher.IsMatch())
               {
                  // match! => add replacement to output
                  modified = true;
                  writer.Write(matcher.Replacement.Item2);
                  var fieldLength = matcher.Replacement.Item1.Length;
                  i -= ((matchIndex + 1) - fieldLength);
                  matchIndex = 0;
                  matcher.Reset();
               }
               else
               {
                  if (reader.Eof && i == reader.FetchedLength - 1)
                  {
                     for (var j = reader.FetchedLength - matchIndex - 1; j < reader.FetchedLength; j++)
                     {
                        writer.Write(reader[j]);
                     }
                     continue;
                  }
                  // possible match => continue
                  matchIndex++;
               }
               continue;
            }
            if (matchIndex == 0)
            {

               //matcher.Reset();

               // no match => add byte to output
               writer.Write(reader[i]);
               continue;
            }

            // there was a partial match, but here it fails
            while (matchIndex != 0)
            {
               writer.Write(reader[i - matchIndex]);
               matchIndex--;
               var match = false;
               matcher.Reset();
               for (var j = 0; j < matchIndex; j++)
               {
                  if (!matcher.IsMatching(reader[i - matchIndex + j], j))
                  {
                     match = false;
                     // still no partial match
                     break;
                  }
                  match = true;
               }
               if (match) break;
            }

            // put it one back, se we can start again to try to match from the last byte
            i--;

         }

         if (modified)
         {
            writer.Flush();
         }


         return modified;
      }

   }
}
