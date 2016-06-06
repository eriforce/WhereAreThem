using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;

namespace Model.Tests {
    [TestClass]
    public class PersistenceTest {
        [TestMethod]
        public void TestJsonNetPersistence() {
            Folder folder = new Folder() { Name = "dir1", CreatedDateUtc = DateTime.UtcNow };
            JsonNetPersistence p = new JsonNetPersistence();
            using (MemoryStream stream = new MemoryStream()) {
                p.Save(folder, stream);

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(stream)) {
                    string json = sr.ReadToEnd();

                    Assert.IsNotNull(json);
                }
            }
        }
    }
}
