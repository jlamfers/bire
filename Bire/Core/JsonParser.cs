using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bire
{
   // rfc7159 complient

   public class JsonParser
   {
      StringBuilder _sb = new StringBuilder();
      private int _pos,_row,_col;
      private StreamReader _reader;

      public object ParseContent(string content)
      {
         using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content), false))
         {
            return Parse(stream);
         }
      }
      public object ParseFile(string filename)
      {
         using (var stream = new FileStream(filename, FileMode.Open))
         {
            return Parse(stream);
         }
      }
      public object Parse(Stream input, int bufferSize = 4096, bool leaveOpen = false)
      {
         object result;
         using (_reader = new StreamReader(input, Encoding.UTF8, true, bufferSize, leaveOpen))
         {
            _pos = _row = _col = 0;
            SkipInsignificants();
            result = ParseValue();
         }
         _reader = null;
         return result;
      }
      private object ParseValue()
      {
         switch (Peek())
         {
            case '"':
               return ParseString();
            case 'n':
               ExpectAndProceed("null");
               return null;
            case 't':
               ExpectAndProceed("true");
               return true;
            case 'f':
               ExpectAndProceed("false");
               return false;
            case '+':
            case '-':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
               return ParseNumber();
            case '[':
               return ParseArray();
            case '{':
               return ParseObject();
            default:
               throw Error(Peek());
         }
      }
      private Dictionary<string,object> ParseObject()
      {
         Next();//{
         var result = new Dictionary<string, object>();
         while (!Eof())
         {
            SkipInsignificants();
            if (Peek() != '"')
            {
               throw Error(Peek(), '"');
            }
            var name = ParseString();
            SkipInsignificants();
            ExpectAndProceed(":");
            SkipInsignificants();
            result.Add(name, ParseValue());
            SkipInsignificants();
            switch (Peek())
            {
               case ',':
                  Next();
                  continue;
               case '}':
                  Next();
                  return result;
               default:
                  break;
            }
            break;
          }
         throw Error(Peek(),'}');
      }
      private object ParseNumber()
      {
         _sb.Length = 0;
         var hasExp = false;
         var hasDec = false;
         while (!Eof())
         {
            switch (Peek())
            {
               case 'e':
               case 'E':
                  hasExp = true;
                  _sb.Append(Next());
                  continue;
               case '.':
                  hasDec = true;
                  _sb.Append(Next());
                  continue;
               case '-':
               case '+':
               case '0':
               case '1':
               case '2':
               case '3':
               case '4':
               case '5':
               case '6':
               case '7':
               case '8':
               case '9':
                  _sb.Append(Next());
                  continue;
               default:
                  break;
            }
            break;
         }
         try
         {
            if (hasExp)
            {
               return double.Parse(_sb.ToString(), CultureInfo.InvariantCulture);
            }
            if (hasDec)
            {
               return decimal.Parse(_sb.ToString(), CultureInfo.InvariantCulture);
            }
            return long.Parse(_sb.ToString(), CultureInfo.InvariantCulture);
         }
         catch (Exception ex)
         {
            _col -= _sb.Length;
            throw Error($"invalid numeric value: {_sb.ToString()}",ex);
         }
      }
      private string ParseString()
      {
         Next(); //"
         _sb.Length = 0;
         while (!Eof())
         {
            switch (Peek())
            {
               case '"':
                  Next();
                  return _sb.ToString();
               case '\\':
                  Next();
                  var ch = Next();
                  switch (ch)
                  {
                     case 'n':
                        _sb.Append("\n");
                        break;
                     case 'r':
                        _sb.Append("\r");
                        break;
                     case 't':
                        _sb.Append("\t");
                        break;
                     case 'f':
                        _sb.Append("\f");
                        break;
                     case 'b':
                        _sb.Append("\b");
                        break;
                     case 'u':
                        _sb.Append(Regex.Unescape($"\\u{Next()}{Next()}{Next()}{Next()}"));
                        break;
                     case '"':
                     case '/':
                     case '\\':
                        _sb.Append(ch);
                        break;
                     default:
                        throw Error($"invalid escaped character: '\\{ch}'");
                  }
                  break;
               default:
                  _sb.Append(Next());
                  break;
            }
         }
         throw Error("unexpected eof, missing '\"'");
      }
      private List<object> ParseArray()
      {
         Next(); //[
         var result = new List<object>();
         SkipInsignificants();
         if (Peek() == ']')
         {
            // empty array
            return result;
         }
         while (!Eof())
         {
            result.Add(ParseValue());
            SkipInsignificants();
            switch (Peek()){
               case ',':
                  Next();
                  SkipInsignificants();
                  continue;
               case ']':
                  Next();
                  return result;
               default:
                  break;
            }
            break;
         }
         throw Error(Peek());
      }
      private void SkipInsignificants()
      {
         while (!Eof())
         {
            switch (Peek())
            {
               case ' ':
               case '\r':
               case '\n':
               case '\t':
                  Next();
                  continue;
               case '/':
                  Next();
                  if (Peek() == '*')
                  {
                     Next();
                     SkipBlockComment();
                  }
                  else if (Peek() == '/')
                  {
                     Next();
                     SkipLineComment();
                  }
                  else
                  {
                     throw Error('/');
                  }
                  continue;
               default:
                  return;
            }
         }

      }
      private void SkipBlockComment()
      {
         while (!Eof() )
         {
            var ch = Next();
            if(ch == '*' && Peek()=='/')
            {
               Next(); //'*'
               return;
            }
            if (ch == '/' && Peek() == '*')
            {
               Next();//'*'
               SkipBlockComment();
            }
         }
         throw Error("unterminated comment, missing \"*/\"");
      }
      private void SkipLineComment()
      {
         while (!Eof() && Peek() != '\r' && Peek() != '\n')
         {
            Next();
         }
         while (Peek() == '\r' || Peek() == '\n')
         {
            Next();
         }
      }
      private char Peek()
      {
         return _reader.EndOfStream ? '\0' : (char)_reader.Peek();
      }
      private char Next()
      {
         var ch = Eof() ? '\0' : (char)_reader.Read();
         switch (ch)
         {
            case '\n':
               _row++;
               _col = 0;
               break;
            case '\r':
               _col = 0;
               break;
            default:
               _col++;
               break;
         }
         return ch;
      }
      private bool Eof()
      {
         return _reader.EndOfStream;
      }
      private void ExpectAndProceed(string expectedChars)
      {
         foreach(var expectedChar in expectedChars)
         {
            var next = Next();
            if(expectedChar != next)
            {
               _col--;
               throw Error(next,expectedChar);
            }
         }
      }
      private Exception Error(char ch)
      {
         return Error($"unexpected char '{ch}'");
      }
      private Exception Error(char ch, char expectedChar)
      {
         return Error($"unexpected char '{ch}', expected '{expectedChar}'");
      }
      private Exception Error(string msg, Exception inner = null)
      {
         return new Exception($"JSON parse error at line {_row+1}, col {_col+1}: {msg}", inner);
      }

   }
}
