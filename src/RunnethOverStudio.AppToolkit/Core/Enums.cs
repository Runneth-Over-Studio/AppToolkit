using System.ComponentModel.DataAnnotations;

namespace RunnethOverStudio.AppToolkit.Core
{
    public static class Enums
    {
        #region StatusCodes
        /// <summary>
        /// Represents general status codes for in-process requests and operations.
        /// These codes are inspired by HTTP status codes but are intended for use in any application context,
        /// not limited to web scenarios.
        /// </summary>
        public enum StatusCodes
        {
            /// <summary>
            /// The operation completed successfully.
            /// </summary>
            OK = 200,

            /// <summary>
            /// The operation resulted in the creation of a new resource or entity.
            /// </summary>
            Created = 201,

            /// <summary>
            /// The request was invalid or could not be understood.
            /// </summary>
            [Display(Name = "Bad Request")]
            BadRequest = 400,

            /// <summary>
            /// The user is unauthenticated.
            /// </summary>
            Unauthorized = 401,

            /// <summary>
            /// The user is authenticated but does not have access rights to perform the operation.
            /// </summary>
            Forbidden = 403,

            /// <summary>
            /// The requested resource or entity was not found.
            /// </summary>
            [Display(Name = "Not Found")]
            NotFound = 404,

            /// <summary>
            /// The requested operation is not allowed for the current resource or context.
            /// </summary>
            [Display(Name = "Method Not Allowed")]
            MethodNotAllowed = 405,

            /// <summary>
            /// The requested operation is not acceptable, possibly due to format or constraints.
            /// </summary>
            [Display(Name = "Not Acceptable")]
            NotAcceptable = 406,

            /// <summary>
            /// The operation timed out before completion.
            /// </summary>
            [Display(Name = "Request Timeout")]
            RequestTimeout = 408,

            /// <summary>
            /// An internal error occurred during the operation.
            /// </summary>
            [Display(Name = "Internal Error")]
            InternalError = 500,

            /// <summary>
            /// Authentication is required to proceed.
            /// </summary>
            [Display(Name = "Authentication Required")]
            AuthenticationRequired = 511
        }
        #endregion
    }
}
