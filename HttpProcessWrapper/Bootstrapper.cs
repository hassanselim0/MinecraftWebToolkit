using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Nancy.TinyIoc;

namespace HttpProcessWrapper
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.OnError += (NancyContext ctx, Exception ex) =>
            {
                if (ctx.Request.Headers.Accept.Any(t => t.Item1.Contains("html"))) return null;

                return new TextResponse(HttpStatusCode.InternalServerError, ex.ToString());
            };

            base.ApplicationStartup(container, pipelines);
        }
    }
}