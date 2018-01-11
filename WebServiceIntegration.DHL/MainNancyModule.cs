﻿namespace WebServiceIntegration.DHL
{
    using System;
    using System.Net;
    using Nancy;
    using Nancy.ModelBinding;
    using HttpStatusCode = Nancy.HttpStatusCode;

    public class MainNancyModule : NancyModule
    {
        public MainNancyModule()
        {
            Get["/"] = x => "Hello World";

            Post["/api/DispatchOrder/"] = parameters =>
            {
                var item = this.Bind<DispatchRequest>();

                var response = new Response {StatusCode = HttpStatusCode.OK};

                if (item.ThrowException) throw new WebException("oops , we have a problem in the DHL module");

                if (item.Fail)
                    response.StatusCode = HttpStatusCode.BadRequest;

                return response;
            };
        }
    }

    public class DispatchRequest
    {
        public bool Fail
        {
            get { return new Random().Next(2) == 0; }
        }

        public Guid OrderId { get; set; }
        public Guid DispatchId { get; set; }
        public string CountryCode { get; set; }
        public Guid DhlCustomerNumber { get; set; }
        public bool ThrowException { get; set; }
    }
}