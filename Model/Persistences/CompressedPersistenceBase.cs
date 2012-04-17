using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace WhereAreThem.Model {
    public abstract class CompressedPersistenceBase {
        public void Compress(Stream inStream, Stream outStream) {
            using (GZipStream gzipStream = new GZipStream(outStream, CompressionMode.Compress)) {
                inStream.CopyTo(gzipStream);
            }
        }

        public void Decompress(Stream inStream, Stream outStream) {
            using (GZipStream gzipStream = new GZipStream(inStream, CompressionMode.Decompress)) {
                gzipStream.CopyTo(outStream);
            }
        }
    }
}
