using System;
using System.Linq;

namespace Bire.Console
{
   using C = System.Console;

   public class LineEditor
   {
      private readonly int _left;
      private readonly int _top;
      private readonly int _width;

      private bool _insert = false;
      private string _text;


      public static string ReadLine(string prompt, string value, out int exitKey)
      {
         C.CursorVisible = false;
         C.Write(prompt ?? "");
         var editor = new LineEditor(value ?? "");
         exitKey = editor.Edit();
         C.WriteLine();
         return editor.Text;
      }
      public static string ReadLine(string prompt = null, string value = null)
      {
         int exitKey;
         return ReadLine(prompt, value, out exitKey);
      }

      public LineEditor(int left, int top, int width)
      {
         _left = left;
         _top = top;
         _width = width;
      }

      public LineEditor(string text, int? width = null)
      {
         _left = C.CursorLeft;
         _top = C.CursorTop;
         _width = width ?? (C.BufferWidth - _left - 1);
         Text = text;
         _insert = true;
      }

      public bool Insert
      {
         get { return _insert; }
         set
         {
            _insert = value;
            C.CursorSize = _insert ? 3 : 50;
         }
      }

      private void WriteControl(string text)
      {
         C.CursorVisible = false;
         var leftSave = C.CursorLeft;
         var topSave = C.CursorTop;
         C.CursorLeft = _left;
         C.CursorTop = _top;
         C.Write((text ?? "").PadRight(_width));
         C.CursorLeft = leftSave;
         C.CursorTop = topSave;
         C.CursorVisible = true;
      }

      public string Text
      {
         get { return _text; }
         set
         {
            _text = value != null ? value.Trim() : null;
            WriteControl(_text);
         }
      }

      public int Edit()
      {
         var value = Text;
         var cursorSize = C.CursorSize;
         var cursorVisible = C.CursorVisible;
         var left = C.CursorLeft;
         var top = C.CursorTop;
         var buf = (value ?? "").PadRight(_width).ToCharArray().ToList();
         var xpos = _text.Length; // Put cursor on the right side of the input field
         xpos = Math.Min(xpos, _width - 1);
         try
         {
            C.CursorTop = _top;
            Insert = _insert;
            while (true)
            {
               xpos = Math.Max(Math.Min(xpos, _width), 0);
               value = new string(buf.ToArray());
               C.CursorLeft = _left + xpos;
               WriteControl(value);
               var ch = C.ReadKey(true);
               switch (ch.Key)
               {
                  case ConsoleKey.Escape:
                     WriteControl(_text);
                     return ch.KeyChar;
                  case ConsoleKey.Enter:
                     WriteControl(value);
                     _text = value.Trim();
                     return ch.KeyChar;
                  case ConsoleKey.LeftArrow:
                     xpos--;
                     break;
                  case ConsoleKey.RightArrow:
                     xpos++;
                     break;
                  case ConsoleKey.Home:
                     xpos = 0;
                     break;
                  case ConsoleKey.End:
                     xpos = value.TrimEnd().Length;
                     break;
                  case ConsoleKey.Insert:
                     Insert = !Insert;
                     break;
                  case ConsoleKey.Backspace:
                     if (xpos > 0)
                     {
                        xpos--;
                        buf.RemoveAt(Math.Max(xpos, 0));
                        buf.Add(' ');
                     }
                     break;
                  case ConsoleKey.Delete:
                     if (xpos < _width)
                     {
                        buf.RemoveAt(xpos);
                        buf.Add(' ');
                     }
                     break;
                  default:
                     if (!char.IsControl(ch.KeyChar))
                     {
                        xpos++;
                        if (xpos > _width)
                        {
                           continue;
                        }
                        if (!_insert)
                        {
                           buf[xpos - 1] = ch.KeyChar;
                        }
                        else
                        {
                           buf.Insert(xpos - 1, ch.KeyChar);
                           buf.RemoveAt(_width);
                        }
                     }
                     break;
               }

            }

         }
         finally
         {
            C.CursorSize = cursorSize;
            C.CursorVisible = cursorVisible;
            C.CursorLeft = left;
            C.CursorTop = top;
         }
      }

   }
}
