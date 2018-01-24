using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Senparc.Weixin;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;

namespace FGWX.Common
{
    public class WXUtils
    {
        public static readonly string StrCon = System.Configuration.ConfigurationManager.ConnectionStrings["fgwx.connection"].ConnectionString;
        public static readonly string AppId = System.Web.Configuration.WebConfigurationManager.AppSettings["WeixinAppId"];//与微信公众账号后台的AppId设置保持一致，区分大小写。
        public static readonly string AppSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["WeixinAppSecret"];//与微信公众账号后台的AppId设置保持一致，区分大小写。

        public static string GetAccessToken()
        {
            //using (System.Net.WebClient webclient = new System.Net.WebClient())
            //{
            //    try
            //    {
            //        return webclient.DownloadString("http://brand.fugumobile.cn/wx/token.ashx");
            //    }
            //    catch (Exception e)
            //    {
            //        Common.Logger.LogUtil.LogError("WXUtils.GetAccessToken: " + AppId, e);
            //        return "";
            //    }
            //}

            //return Senparc.Weixin.MP.Containers.AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            string token = null;
            try
            {
                token = Utils.NullFilter<string>(SQLHelper.ExecuteScalar(StrCon, System.Data.CommandType.Text, "select top 1 access_token from wx_access_token where appid = '" + AppId + "' and expires_time > '" + DateTime.UtcNow.AddHours(8).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss") + "' order by id desc", null), null);

                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        string result = CacheHelper.Get("IP" + AppId) as string;
                        if (result != "true")
                        {
                            using (System.Net.WebClient client = new System.Net.WebClient())
                            {
                                try
                                {
                                    client.Encoding = System.Text.Encoding.UTF8;
                                    result = client.DownloadString("https://api.weixin.qq.com/cgi-bin/getcallbackip?access_token=" + token);
                                }
                                catch
                                {
                                    result = "";
                                }
                            }
                            if (result == null || !result.Contains("ip_list"))
                            {
                                token = "";
                            }
                            else
                            {
                                CacheHelper.Insert("IP" + AppId, result, 120);
                            }
                        }
                    }
                    catch (Exception ipex)
                    {
                        Common.Logger.LogUtil.LogError("WXUtils.GetCallbackIP: " + AppId, ipex);
                    }
                }
                if (string.IsNullOrEmpty(token))
                {
                    AccessTokenResult result = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetToken(AppId, AppSecret);
                    if (result != null && !string.IsNullOrEmpty(result.access_token))
                    {
                        token = result.access_token;
                        try
                        {
                            SQLHelper.ExecuteNonQuery(StrCon, System.Data.CommandType.Text, "insert into wx_access_token (appid,access_token,expires_in,create_time,expires_time) values ('" + AppId + "','" + token + "','" + result.expires_in + "','" + DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + DateTime.UtcNow.AddHours(8).AddSeconds(result.expires_in).ToString("yyyy-MM-dd HH:mm:ss.fff") + "')", null);
                        }
                        catch (Exception e)
                        {
                            Common.Logger.LogUtil.LogError("WXUtils.GetAccessToken: " + AppId, e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.Logger.LogUtil.LogError("WXUtils.GetAccessToken: " + AppId, e);
            }
            return token;
        }
        public static string GetJSSDKTicket()
        {
            //return Senparc.Weixin.MP.Containers.JsApiTicketContainer.TryGetJsApiTicket(AppId, AppSecret);
            string ticket = null;
            try
            {
                ticket = Utils.NullFilter<string>(SQLHelper.ExecuteScalar(StrCon, System.Data.CommandType.Text, "select top 1 ticket from wx_jsapi_ticket where appid = '" + AppId + "' and expires_time > '" + DateTime.UtcNow.AddHours(8).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss") + "' order by id desc", null), null);
                if (string.IsNullOrEmpty(ticket))
                {
                    string token = GetAccessToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        JsApiTicketResult result = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetTicketByAccessToken(token);
                        if (result != null && result.errcode == ReturnCode.请求成功 && !string.IsNullOrEmpty(result.ticket))
                        {
                            ticket = result.ticket;
                            try
                            {
                                SQLHelper.ExecuteNonQuery(StrCon, System.Data.CommandType.Text, "insert into wx_jsapi_ticket (appid,ticket,expires_in,create_time,expires_time) values ('" + AppId + "','" + ticket + "','" + result.expires_in + "','" + DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + DateTime.UtcNow.AddHours(8).AddSeconds(result.expires_in).ToString("yyyy-MM-dd HH:mm:ss.fff") + "')", null);
                            }
                            catch (Exception e)
                            {
                                Common.Logger.LogUtil.LogError("WXUtils.GetJSSDKTicket: Token: " + token, e);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.Logger.LogUtil.LogError("WXUtils.GetJSSDKTicket: " + AppId, e);
            }
            return ticket;
        }

        public static string GetSignature(string ticket, string noncestr, string timestamp, string url)
        {
            return Senparc.Weixin.MP.Helpers.JSSDKHelper.GetSignature(ticket, noncestr, timestamp, url);
        }

        public static bool ChangeUserGroup(string openid, int groupid)
        {
            if (string.IsNullOrWhiteSpace(openid) || groupid < 0)
            {
                return false;
            }
            string accessToken = GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            //Senparc.Weixin.Entities.WxJsonResult result = Senparc.Weixin.MP.AdvancedAPIs.GroupsApi.MemberUpdate(accessToken, openid, groupid, 4000);
            Senparc.Weixin.Entities.WxJsonResult result = Senparc.Weixin.MP.AdvancedAPIs.UserTagApi.BatchTagging(accessToken, groupid, new List<string>() { openid });
            if (result.errmsg != "ok")
            {
                return false;
            }
            return true;
        }

        public static bool IsSubscribe(string openid)
        {
            if (string.IsNullOrWhiteSpace(openid))
            {
                return false;
            }
            string accessToken = GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken)) return true;

            string subscribe = CacheHelper.Get(openid) as string;
            if (subscribe == null)
            {
                Senparc.Weixin.MP.AdvancedAPIs.User.UserInfoJson result = Senparc.Weixin.MP.AdvancedAPIs.UserApi.Info(accessToken, openid);

                if (result == null || result.errcode != ReturnCode.请求成功)
                {
                    subscribe = "1";
                    Common.Logger.LogUtil.LogError("IsSubscribe: " + openid + "\r\n" + result.ToString());
                }
                else
                {
                    subscribe = result.subscribe + "";
                    if (subscribe == "1")
                        CacheHelper.Insert(openid, result.subscribe, 600);
                }
            }
            if (subscribe == "1")
                return true;
            else
                return false;
        }
    }
}