using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Security;

namespace WhereAreThem.WebViewer.Models {
    public static class ClientCertificateExtensions {
        public static bool CertAuth(this HttpRequestBase request, string issuer) {
            HttpClientCertificate cert = request.ClientCertificate;
            if (cert == null || !cert.IsValid)
                return false;

            X509Certificate2 x509 = new X509Certificate2(cert.Certificate);
            X509Chain chain = new X509Chain(true);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Offline;
            chain.Build(x509);

            if (!chain.ChainElements.Cast<X509ChainElement>().Any(e =>
                    e.Certificate.Thumbprint.Equals(issuer, StringComparison.OrdinalIgnoreCase)))
                return false;

            string userName = x509.GetNameInfo(X509NameType.SimpleName, false);
            FormsAuthentication.SetAuthCookie(userName, false, request.ApplicationPath);
            return true;
        }
    }
}
