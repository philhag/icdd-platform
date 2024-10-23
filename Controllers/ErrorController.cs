using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IcddWebApp.PageModels.Error;

namespace IcddWebApp.Controllers
{
    /// <inheritdoc />
    public class ErrorController : Controller
    {
        /// <summary>
        /// Renders an Error Message
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorTitle"></param>
        /// <param name="errorMessage"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public IActionResult Index(int errorCode, string errorTitle, string errorMessage, Exception innerException)
        {
            if (innerException == null || string.IsNullOrEmpty(innerException.Source))
            {
                return View(new ErrorPageModel(errorCode, errorTitle, errorMessage));
            }
            return View(new ErrorPageModel(errorCode, errorTitle, errorMessage, innerException));
        }

    }
}
