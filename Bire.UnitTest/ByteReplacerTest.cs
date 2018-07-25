using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Bire.UnitTest
{
   [TestClass]
   public class ByteReplacerTest
   {
      [TestMethod]
      public void MonkeyTest()
      {
         var bytes = new byte[0];
         var replacements = new[]
         {
            Tuple.Create(new byte[]{ }, new byte[]{ })
         };
         var r = ByteReplacer.TryReplace(ref bytes, replacements);
      }

      [TestMethod]
      public void NoMatchWorks()
      {
         var bytes = new byte[] { 1,2,3};
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1,2,3,4 }, new byte[]{ 1 })
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsFalse(r);
         Assert.AreSame(bytes, bytes2);
      }

      [TestMethod]
      public void ExactBufferSizeWithNoMatchResultoIntoNoReplacements()
      {
         var bytes = new byte[4096]; // 4096 is buffer size
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1,2,3,4 }, new byte[]{ 1 })
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsFalse(r);
         Assert.AreSame(bytes, bytes2);
      }

      [TestMethod]
      public void NoMatchWorks2()
      {
         var bytes = new byte[] { 1, 2, 3 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1,3 }, new byte[]{ 1 })
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsFalse(r);
         Assert.AreSame(bytes, bytes2);
      }

      [TestMethod]
      public void NoMatchWorks3()
      {
         var bytes = new byte[] { 1, 2, 3 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1,3 }, new byte[]{ 1 }),
            Tuple.Create(new byte[]{2,1 }, new byte[]{ 1 }),
            Tuple.Create(new byte[]{3,1 }, new byte[]{ 1 }),
            Tuple.Create(new byte[]{1,1 }, new byte[]{ 1 })
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsFalse(r);
         Assert.AreSame(bytes, bytes2);
      }

      [TestMethod]
      public void SingleMatchWorks()
      {
         var bytes = new byte[] { 1 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1 }, new byte[]{ 2 }),
            Tuple.Create(new byte[]{2 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 2 }));
      }

      [TestMethod]
      public void SingleMatchWorks2()
      {
         var bytes = new byte[] { 1, 1, 1 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1 }, new byte[]{ 2 }),
            Tuple.Create(new byte[]{2 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 2, 2, 2 }));
      }

      [TestMethod]
      public void DoubleMatchWorks()
      {
         var bytes = new byte[] { 1, 1, 2, 3 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 2 }),
            Tuple.Create(new byte[]{2, 3 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 3 }));
      }
      [TestMethod]
      public void DoubleMatchWorks2()
      {
         var bytes = new byte[] { 1, 1, 2, 3 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 2 }),
            Tuple.Create(new byte[]{2, 3 }, new byte[]{ 3 }),
            Tuple.Create(new byte[]{3 }, new byte[]{ 4 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 4 }));
      }

      [TestMethod]
      public void DoubleMatchWorks3()
      {
         var bytes = new byte[] { 1, 1, 2, 1,2,1 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 1 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 1, 1,1 }));
      }

      [TestMethod]
      public void TripleMatchWorks()
      {
         var bytes = new byte[] { 1, 2, 1, 2, 3 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 3 }));
      }

      [TestMethod]
      public void TripleMatchWorks2()
      {
         var bytes = new byte[] { 1, 2, 1, 2, 3, 1, 2, 3 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 3, 3 }));
      }
      [TestMethod]
      public void TripleMatchWorks3()
      {
         var bytes = new byte[] { 1, 2, 1, 2, 3, 1, 2, 3, 1 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 3, 3, 1 }));
      }
      [TestMethod]
      public void TripleMatchWorks4()
      {
         var bytes = new byte[] { 1, 2, 1, 2, 3, 1, 2, 3, 0 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 3, 3, 0 }));
      }
      [TestMethod]
      public void TripleMatchWorks5()
      {
         var bytes = new byte[] { 1, 2, 1, 2, 3, 1, 2, 3, 1,2 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 1, 2, 3, 3,1,2 }));
      }
      [TestMethod]
      public void LongestMatchWorks()
      {
         var bytes = new byte[] { 1, 2, 3,4,5 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 1 }),
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 2 }),
            Tuple.Create(new byte[]{1, 2, 3, 4 }, new byte[]{ 3 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 3, 5 }));
      }
      [TestMethod]
      public void LongestMatchWorks2()
      {
         var bytes = new byte[] { 1, 2, 3, 4, 5 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 6 }),
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 7 }),
            Tuple.Create(new byte[]{1, 2, 3, 4 }, new byte[]{ 8 }),
            Tuple.Create(new byte[]{5 }, new byte[]{ 9 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 8, 9 }));
      }
      [TestMethod]
      public void LongestMatchWorks3()
      {
         var bytes = new byte[] { 1,2, 1, 2, 3, 4, 5 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 6 }),
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 7 }),
            Tuple.Create(new byte[]{1, 2, 3, 4 }, new byte[]{ 8 }),
            Tuple.Create(new byte[]{5 }, new byte[]{ 9 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 6, 8, 9 }));
      }

      [TestMethod]
      public void LongestMatchWorks4()
      {
         var bytes = new byte[] { 1, 2, 3, 4 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 6 }),
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 7 }),
            Tuple.Create(new byte[]{1, 2, 3, 4 }, new byte[]{ 8 }),
            Tuple.Create(new byte[]{5 }, new byte[]{ 9 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 8 }));
      }

      [TestMethod]
      public void LongestMatchWorks5()
      {
         var bytes = new byte[] { 5,5,6,1, 1,2, 1,2,3, 1, 2, 3, 4,5 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1 }, new byte[]{ 5}),
            Tuple.Create(new byte[]{5,6,7 }, new byte[]{ 9}),
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 6 }),
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 7 }),
            Tuple.Create(new byte[]{1, 2, 3, 4 }, new byte[]{ 8 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 5,5,6,5,6,7,8,5 }));
      }
      [TestMethod]
      public void LongestMatchWorks6()
      {
         var bytes = new byte[] { 5, 5, 6, 1, 1, 2,5,6, 1, 2, 3, 1, 2, 3, 4, 5 };
         var bytes2 = bytes;
         var replacements = new[]
         {
            Tuple.Create(new byte[]{1 }, new byte[]{ 5}),
            Tuple.Create(new byte[]{5,6,7 }, new byte[]{ 9}),
            Tuple.Create(new byte[]{1, 2 }, new byte[]{ 6 }),
            Tuple.Create(new byte[]{1, 2, 3 }, new byte[]{ 7 }),
            Tuple.Create(new byte[]{1, 2, 3, 4 }, new byte[]{ 8 }),
         };
         var r = ByteReplacer.TryReplace(ref bytes2, replacements);
         Assert.IsTrue(r);
         Assert.AreNotSame(bytes, bytes2);
         Assert.IsTrue(bytes2.SequenceEqual(new byte[] { 5, 5, 6, 5, 6,5,6, 7, 8, 5 }));
      }


   }
}
