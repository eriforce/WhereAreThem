using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;
using Xunit;

namespace Model.Tests {
    public class PersistenceTest {
        [Fact]
        public void TestJsonNetPersistence() {
            Folder folder = new Folder() { Name = "dir1", CreatedDateUtc = DateTime.UtcNow };
            JsonNetPersistence p = new JsonNetPersistence();
            using (MemoryStream stream = new MemoryStream()) {
                p.Save(folder, stream);

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(stream)) {
                    string json = sr.ReadToEnd();

                    Assert.NotNull(json);
                }
            }
        }
    }
}
