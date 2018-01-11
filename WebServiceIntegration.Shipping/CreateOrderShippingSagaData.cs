namespace WebServiceIntegration.Shipping
{
    using System;
    using NServiceBus;

    internal class CreateOrderShippingSagaData : ContainSagaData
    {
        public virtual Guid OrderId { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual Guid CustomerNumber { get; set; }
        public virtual bool ThrowException { get; set; }
        public virtual bool DhlFailed { get; set; }
        public virtual bool FedexFailed { get; set; }
        public virtual int OrderNumber { get; set; }
    }
}