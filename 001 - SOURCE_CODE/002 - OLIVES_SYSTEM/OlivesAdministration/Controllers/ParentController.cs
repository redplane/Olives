﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace OlivesAdministration.Controllers
{
    public class ParentController : ApiController
    {
        /// <summary>
        ///     Retrieve validation errors and bind 'em to list.
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        protected IEnumerable<string> RetrieveValidationErrors(ModelStateDictionary modelState)
        {
            // Invalid model state.
            if (modelState == null)
                return new List<string>();

            return modelState.Keys.SelectMany(key => modelState[key].Errors.Select(error => error.ErrorMessage));
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            // By default, english will be used.
            var acceptLanguage = "en-US";

            // Check whether any language has been sent to server.
            var language = controllerContext.Request.Headers.AcceptLanguage.FirstOrDefault();

            if (language != null && !string.IsNullOrEmpty(language.Value))
                acceptLanguage = language.Value;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(acceptLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(acceptLanguage);
            }
            catch (Exception)
            {
                // Suppress exception
            }

            base.Initialize(controllerContext);
        }

        //{

        //protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        //    // By default, english will be used.
        //    var acceptLanguage = "en-US";

        //    // Check whether any language has been sent to server.
        //    var language =
        //        Request.Headers.AllKeys.FirstOrDefault(
        //            x => x.Equals("Accept-Language", StringComparison.InvariantCultureIgnoreCase));

        //    if (!string.IsNullOrEmpty(acceptLanguage))
        //        acceptLanguage = Request.Headers[language];

        //    try
        //    {
        //        Thread.CurrentThread.CurrentCulture = new CultureInfo(acceptLanguage);
        //        Thread.CurrentThread.CurrentUICulture = new CultureInfo(acceptLanguage);
        //    }
        //    catch (Exception)
        //    {
        //        // Suppress exception
        //    }


        //    return base.BeginExecuteCore(callback, state);
        //}

        //protected override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    filterContext.HttpContext.Response.AppendHeader("Access-Control-Allow-Origin", "*");
        //    base.OnActionExecuted(filterContext);
        //}
    }
}