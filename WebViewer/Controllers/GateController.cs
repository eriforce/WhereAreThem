using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WhereAreThem.WebViewer.Models;

namespace WhereAreThem.WebViewer.Controllers {
    public class GateController : Controller {
        public ActionResult Login() {
            if (Request.CertAuth(ConfigurationManager.AppSettings["issuer"]))
                return RedirectToAction(Extensions.ActionIndex, Extensions.ControllerHome);
            else
                return new HttpStatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
    }
}
