using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Bire.UnitTest
{
   [TestClass]
   public class JsonParserTest
   {
      [TestMethod]
      public void MonkeyTest()
      {
         var json = File.ReadAllText("boilerplateinfo.json");
         var parser = new JsonParser();
         var result = parser.ParseContent(json);

      }
      [TestMethod]
      public void MonkeyTest3()
      {
         var result = new JsonParser().ParseFile("boilerplateinfo.json");
      }

      [TestMethod]
      public void MonkeyTest2()
      {
         var json = File.ReadAllText("boilerplateinfo.json");
         var parser = new JsonParser();
         var result = parser.ParseContent(json);

         var info = BoilerplateInfo.FromFile("boilerplateinfo.json");

      }
   }
}
