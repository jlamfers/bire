using System;
using System.IO;

namespace Bire
{
   class StreamArray
   {
      const int BUF_COUNT = 2;
      private readonly Stream _stream;
      private readonly byte[][] _buffer;
      private int _bufferIndex = -1;
      private readonly int _bufsize;
      private int _readCount;
      private int _minIndex, _maxIndex;

      public StreamArray(Stream stream, int bufsize = 4096)
      {
         _stream = stream;
         _buffer = new byte[BUF_COUNT][];
         for (var i = 0; i < _buffer.Length; i++)
         {
            _buffer[i] = new byte[bufsize];
         }
         _bufsize = bufsize;
         FetchBytes();
      }

      public byte this[int index]
      {
         get
         {
            if (index < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index >= _readCount)
            {
               FetchBytes();
            }
            if (index < _minIndex || index > _maxIndex)
            {
               if (Eof)
               {
                  throw new ArgumentOutOfRangeException(nameof(index));
               }
               throw new ArgumentOutOfRangeException(nameof(index), $"index {index} is outside the buffered boundaries ({_minIndex}-{_maxIndex}). you need to increase the buffer size. current size: {_bufsize}");
            }
            var x = (index / _bufsize) % BUF_COUNT;
            var y = index % _bufsize;
            return _buffer[x][y];
         }
      }

      private void FetchBytes()
      {
         if (Eof) return;
         if (++_bufferIndex == BUF_COUNT)
         {
            _bufferIndex = 0;
         }
         var count = _stream.Read(_buffer[_bufferIndex], 0, _bufsize);
         Eof = _stream.Position >= _stream.Length - 1;
         _minIndex = Math.Max(_readCount - _bufsize, 0);
         _readCount += count;
         _maxIndex = _readCount - 1;
         FetchedLength = _readCount;
      }

      public bool Eof
      {
         get;private set;
      }

      public int FetchedLength { get; private set; }
   }

}
