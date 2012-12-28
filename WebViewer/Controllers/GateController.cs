using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PureLib.Common;
using WhereAreThem.WebViewer.Models;

namespace WhereAreThem.WebViewer.Controllers {
    public class GateController : Controller {
        public ActionResult Login() {
            if (Debugger.IsAttached)
                FormsAuthentication.SetAuthCookie("Debug", false);

            if (!ValidateClientCertificate(Request.ClientCertificate))
                return new HttpStatusCodeResult((int)HttpStatusCode.Unauthorized);
            else
                FormsAuthentication.SetAuthCookie(Request.ClientCertificate.Subject.Substring("CN=".Length), false);

            return RedirectToAction(Extensions.ActionIndex, Extensions.ControllerHome);
        }

        private bool ValidateClientCertificate(HttpClientCertificate cert) {
            if (cert == null || !cert.IsValid)
                return false;

            string validIssuer = ConfigurationManager.AppSettings["issuer"];
            if (!validIssuer.IsNullOrEmpty() && !cert.BinaryIssuer.ToHexString().Equals(validIssuer, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}
