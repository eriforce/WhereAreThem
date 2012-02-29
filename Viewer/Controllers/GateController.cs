using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WhereAreThem.Viewer.Models;

namespace WhereAreThem.Viewer.Controllers
{
    public class GateController : Controller
    {
        public ActionResult Login()
        {
            if ((Request.ClientCertificate != null) && Request.ClientCertificate.IsValid
                    && (Request.ClientCertificate.Issuer == Request.ClientCertificate.ServerIssuer)) {
                FormsAuthentication.SetAuthCookie(Request.ClientCertificate.Subject.Substring(3), true);
                return RedirectToAction(Extensions.ActionIndex, Extensions.ControllerHome);
            }
            return new HttpStatusCodeResult(401);
        }
    }
}
