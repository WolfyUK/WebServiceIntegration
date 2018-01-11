namespace WebServiceIntegration.Shipping
{
    using System;
    using System.Threading.Tasks;
    using Messages.Commands;
    using Messages.Response;
    using NServiceBus;

    internal class CreateOrderShippingSaga : Saga<CreateOrderShippingSagaData>,
        IAmStartedByMessages<CreateOrderShipping>, 
        IHandleMessages<DispatchOrderToDhlFailure>,
        IHandleMessages<DispatchOrderToDhlSucsess>,
        IHandleMessages<DispatchOrderToFedexSucsess>,
        IHandleMessages<DispatchOrderToFedexFailure>,
        IHandleMessages<FedexAndDhlFailed>
    { 
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CreateOrderShippingSagaData> mapper)
        {
            mapper.ConfigureMapping<CreateOrderShipping>(m => m.OrderId)
               .ToSaga(s => s.OrderId);
        }

        public Task Handle(CreateOrderShipping message, IMessageHandlerContext context)
        {
            var customerNumber = new Guid("f64bb7b3-fb1c-486e-b745-8062bf30e4d3");

            Console.WriteLine("Handling message CreateOrderShipping orderId: {0} OrderNumber: {1}", message.OrderId,
                message.OrderNumber);

            Data.OrderId = message.OrderId;
            Data.CountryCode = message.OrderCountryCode;
            Data.CustomerNumber = customerNumber;
            Data.ThrowException = message.ThrowException;
            Data.OrderNumber = message.OrderNumber;

            // do some shipping related logic
            var dispatchOrderToDhl = new DispatchOrderToDhl
            {
                CountryCode = message.OrderCountryCode,
                OrderId = message.OrderId,
                DhlCustomerNumber = customerNumber,
                DispatchId = Guid.NewGuid(),
                ThrowException = message.ThrowException
            };

            //Dispatch the order to DHL
            return context.Send(dispatchOrderToDhl);
        }

        public Task Handle(DispatchOrderToDhlFailure message, IMessageHandlerContext context)
        {
            // depending on notify web service, we can retry
            // notify on failure
            // timeout to retry later
            // and so on

            Console.WriteLine("Dispatch Order: {0} and DispatchId: {1} failed with DHL", message.OrderId, message.DispatchId);
            
            // do some shipping related logic
            Console.WriteLine("Dispatch Order: {0} to Fedex ", message.OrderId);

            Data.DhlFailed = true;

            var customerNumber = new Guid("f64bb7b3-fb1c-486e-b745-8062bf30e4d3");

            var dispatchOrderToFedex = new DispatchOrderToFedex
            {
                CountryCode = Data.CountryCode,
                OrderId = message.OrderId,
                FedexCustomerNumber = customerNumber,
                DispatchId = Guid.NewGuid(),
                ThrowException = !Data.ThrowException, //reverse the failure for Fedex
            };

            //Dispatch the order to Fedex
            return context.Send(dispatchOrderToFedex);
        }

        public Task Handle(DispatchOrderToDhlSucsess message, IMessageHandlerContext context)
        {
            // complete of mark complete in state to keep the data or rehydrate the saga
            Console.WriteLine("Dispatch Order: {0} and DispatchId: {1} success with DHL", message.OrderId, message.DispatchId);

            Data.DhlFailed = false;

            return Task.FromResult(0);
        }

        public Task Handle(DispatchOrderToFedexSucsess message, IMessageHandlerContext context)
        {
            // complete of mark complete in state to keep the data or rehydrate the saga
            Console.WriteLine("Dispatch Order: {0} and DispatchId: {1} success with Fedex", message.OrderId, message.DispatchId);

            Data.FedexFailed = false;

            return Task.FromResult(0);
        }

        public Task Handle(DispatchOrderToFedexFailure message, IMessageHandlerContext context)
        {
            // depending on notify web service, we can retry
            // notify on failure
            // timeout to retry later
            // and so on

            Data.FedexFailed = true;

            if (Data.FedexFailed && Data.DhlFailed)
            {
                // timeout for 10 min. and try again
                // reset flags?
                RequestTimeout<FedexAndDhlFailed>(context, new TimeSpan(00, 10, 00));
            }
            Console.WriteLine("Dispatch Order: {0} and DispatchId: {1} failed with Fedex", message.OrderId, message.DispatchId);

            return Task.FromResult(0);
        }

        public async Task Handle(FedexAndDhlFailed message, IMessageHandlerContext context)
        {
            // do stuff?
            var customerNumber = new Guid("f64bb7b3-fb1c-486e-b745-8062bf30e4d3");

            Console.WriteLine("Handling message FedexAndDhlFailed orderId: {0} OrderNumber: {1}", Data.OrderId,
                Data.OrderNumber);

            // do some shipping related logic
            var dispatchOrderToDhl = new DispatchOrderToDhl
            {
                CountryCode = Data.CountryCode,
                OrderId = Data.OrderId,
                DhlCustomerNumber = customerNumber,
                DispatchId = Guid.NewGuid(),
                ThrowException = Data.ThrowException
            };

            //Dispatch the order to DHL
            await context.Send(dispatchOrderToDhl).ConfigureAwait(false);
        }
    }
}