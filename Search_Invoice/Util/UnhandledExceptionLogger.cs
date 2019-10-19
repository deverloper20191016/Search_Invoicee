using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace Search_Invoice.Util
{
    public class UnhandledExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            ExceptionUtility.LogException(context.Exception, context.Request.RequestUri.PathAndQuery);
            base.Log(context);
        }
    }
}