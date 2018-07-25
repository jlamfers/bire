using System;
using System.Collections.Generic;
using System.Linq;

namespace Bire
{
   class Matcher
   {
      private class MatchItem
      {

         public readonly Tuple<byte[], byte[]> Tuple;
         public readonly byte[] Field;
         public readonly byte[] Replacement;

         public MatchItem(Tuple<byte[],byte[]> tuple)
         {
            Tuple = tuple;
            Field = tuple.Item1;
            Replacement = tuple.Item2;

         }
         public bool IsMatch(byte b, int index, out bool completed) {
            completed = false;
            if (index >= Field.Length) return true;
            var result = Field[index] == b;
            completed = result && index == Field.Length - 1;
            return result;
            
         }

      }

      private MatchItem[] _matchItems;
      private int _maxFieldLength;
      private MatchItem[] _probes;
      private int _probeCount;
      private int _matchCount;
      private Func<byte,bool> _quickFirstByteMatch;

      public Matcher(IEnumerable<Tuple<byte[], byte[]>> tuples)
      {
         _matchItems = tuples.Select(t => new MatchItem(t)).ToArray();
         _maxFieldLength = _matchItems.Select(x => x.Field.Length).Max();
         _probes = _matchItems.ToArray();
         _probeCount = _probes.Length;
         _matchCount = 0;

         var firstBytes = new HashSet<byte>(_matchItems.Select(i => i.Field.FirstOrDefault()));
         if(firstBytes.Count == 1)
         {
            var first = firstBytes.First();
            _quickFirstByteMatch = b => b == first;
         }
         else
         {
            _quickFirstByteMatch = b => firstBytes.Contains(b);
         }

      }
      public bool IsMatch()
      {
         return _matchCount == _probeCount;
      }
      public bool IsMatching(byte next, int index)
      {
         if(index == 0 && !_quickFirstByteMatch(next))
         {
            return false;
         }

         int i = _matchCount;
         while (i < _probeCount)
         {
            bool completed;
            if (!_probes[i].IsMatch(next, index, out completed))
            {
               _probes[i] = _probes[--_probeCount];
            }
            else
            {
               if (completed)
               {
                  var tmp = _probes[_matchCount];
                  _probes[_matchCount] = _probes[i];
                  _probes[i] = tmp;
                  _matchCount++;
               }
               i++;
            }
         }
         return _probeCount > 0;

      }
      public Tuple<byte[],byte[]> Replacement
      {
         get {
            if (_matchCount == 0 || _matchCount != _probeCount) return null;
            if (_matchCount == 1) return _probes[0].Tuple;
            MatchItem match = _probes[0];
            for(var i = 1; i < _matchCount; i++)
            {
               if(_probes[i].Field.Length > match.Field.Length)
               {
                  match = _probes[i];
               }
            }
            return match.Tuple;
         }
      }
      

      public void Reset()
      {
         if (_matchCount == 0 && _probeCount == _probes.Length) return;

         Array.Copy(_matchItems, _probes, _probes.Length);
         _probeCount = _probes.Length;
         _matchCount = 0;

      }
   }
}
