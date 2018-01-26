using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;

namespace WebServiceIntegration.Fedex
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration() { Password = "password" }; }
        }
    }
}