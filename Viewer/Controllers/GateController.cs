using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PureLib.Common;
using WhereAreThem.Viewer.Models;

namespace WhereAreThem.Viewer.Controllers {
    public class GateController : Controller {
        public ActionResult Login() {
            if ((Request.ClientCertificate == null) || !Request.ClientCertificate.IsValid
                    || Request.ClientCertificate.Issuer.IsNullOrEmpty())
                return new HttpStatusCodeResult((int)HttpStatusCode.Unauthorized);

            FormsAuthentication.SetAuthCookie(Request.ClientCertificate.Subject.Substring(3), false);
            return RedirectToAction(Extensions.ActionIndex, Extensions.ControllerHome);
        }
    }
}
