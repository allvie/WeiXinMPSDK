using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin.MP.Containers;

namespace Senparc.Weixin.MP.Sample.Controllers
{
    /// <summary>
    /// 设备能力测试
    /// </summary>
    public class DeviceController : BaseController
    {
        private string appId = ConfigurationManager.AppSettings["WeixinAppId"];
        private string secret = ConfigurationManager.AppSettings["WeixinAppSecret"];


        public ActionResult Index()
        {
            //var accessToken = AccessTokenContainer.TryGetAccessToken(appId, secret);
            var accessToken = FGWX.Common.WXUtils.GetAccessToken();
            TempData["AccessToken"] = accessToken;
            //var jssdkUiPackage = JSSDKHelper.GetJsSdkUiPackage(appId, secret, Request.Url.AbsoluteUri);

            //获取时间戳
            var timestamp = JSSDKHelper.GetTimestamp();
            //获取随机码
            string nonceStr = JSSDKHelper.GetNoncestr();
            string ticket = FGWX.Common.WXUtils.GetJSSDKTicket();
            //获取签名
            string signature = FGWX.Common.WXUtils.GetSignature(ticket,nonceStr,timestamp, Request.Url.AbsoluteUri);
            var jssdkUiPackage = new JsSdkUiPackage(appId, timestamp, nonceStr, signature);
            return View(jssdkUiPackage);
        }
    }
}