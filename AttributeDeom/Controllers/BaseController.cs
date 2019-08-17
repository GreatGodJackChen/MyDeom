using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AttributeDeom.Controllers
{
    public class BaseController : Controller
    {
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class UserAuthorizeAttribute : Attribute,IAuthorizationFilter
    {
        /// <summary>
        /// 模块别名，可配置更改
        /// </summary>
        public string ModuleAlias { get; set; }
        /// <summary>
        /// 权限动作
        /// </summary>
        public string OperaAction { get; set; }
        /// <summary>
        /// 权限访问控制器参数
        /// </summary>
        private string Sign { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //一系列操作
            if (string.IsNullOrEmpty(ModuleAlias))
            {
                context.HttpContext.Response.StatusCode = 401;
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.WriteAsync("{\"Code\":401,\"Message\":\"无权访问\"}"); ;
            }
            else
            {
            }
        }
    }
}