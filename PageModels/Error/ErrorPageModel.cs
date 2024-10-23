using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.PageModels.Error
{
    /// <summary>
    /// Error Page Model
    /// </summary>
    public class ErrorPageModel
    {
        public string Redirect { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
        public Exception InnerException { get; set; }

        public ErrorPageModel(int errorCode, string errorTitle, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            Redirect = null;
        }

        public ErrorPageModel(int errorCode, string errorTitle, string errorMessage, string redirect)
        {
            ErrorCode = errorCode;
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            Redirect = redirect;
        }

        public ErrorPageModel(string errorTitle, string errorMessage)
        {
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            Redirect = null;
        }

        public ErrorPageModel(string errorTitle, string errorMessage, string redirect)
        {
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            Redirect = redirect;
        }

        public ErrorPageModel(int errorCode, string errorTitle, string errorMessage, Exception innerException)
        {
            ErrorCode = errorCode;
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            InnerException = innerException;
        }
        public ErrorPageModel(int errorCode, string errorTitle, string errorMessage, Exception innerException, string redirect)
        {
            ErrorCode = errorCode;
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            InnerException = innerException;
            Redirect = redirect;
        }
    }
}
