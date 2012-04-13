using System;
using System.Collections.Generic;
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
                FormsAuthentication.SetAuthCookie("Debug", true);
            else if ((Request.ClientCertificate == null) || !Request.ClientCertificate.IsValid
                    || Request.ClientCertificate.Issuer.IsNullOrEmpty())
                return new HttpStatusCodeResult((int)HttpStatusCode.Unauthorized);
            else {
                string userName = Request.ClientCertificate.Subject.Substring(3);
                FormsAuthentication.SetAuthCookie(userName, false);
            }
            return RedirectToAction(Extensions.ActionIndex, Extensions.ControllerHome);
        }
    }
}
