using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeRegistrationApp.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }
        [Route("/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode) 
            {
                case 404:
                    {
                        ViewBag.ErrorMesage = "Sorry, Requested resource cannot be found!!!!";
                        //ViewBag.Path = statusCodeResult.OriginalPath;
                        //ViewBag.QueryString = statusCodeResult.OriginalQueryString;
                        logger.LogWarning($"404 Error occured.The path is {statusCodeResult.OriginalPath}"
                                          + $"The query string is {statusCodeResult.OriginalQueryString}"
                                          );
                        break;
                    }
            }
            return View("NotFound");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            //ViewBag.ExceptionPath = exceptionDetails.Path;
            //ViewBag.ExceptionMessage = exceptionDetails.Error.Message;
            //ViewBag.StackTrace = exceptionDetails.Error.StackTrace;
            logger.LogError($"The path {exceptionDetails.Path} threw an exception."+
                            $" {exceptionDetails.Error.Message}"
                            );

            return View("Error");
        }
    }
}