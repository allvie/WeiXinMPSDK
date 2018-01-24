/*----------------------------------------------------------------
    Copyright (C) 2016 Senparc
    
    文件名：HomeController.cs
    文件功能描述：首页Controller
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.Open.CommonAPIs;

namespace Senparc.Weixin.MP.Sample.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // GET: User
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            string username = collection["username"];
            string password = collection["password"];
            if (!string.IsNullOrWhiteSpace(username) && username.ToLower() == "dyson" && password == "Dyson@2017")
            {
                System.Web.HttpContext.Current.Session["fgwx_loggedin"] = true;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["message"] = "Wrong username or password!";
            }
            return View();
        }
    }
}
