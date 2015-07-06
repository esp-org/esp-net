﻿using System;
using Esp.Net.Concurrency;
using Esp.Net.Model;
#if ESP_EXPERIMENTAL

namespace Esp.Net.Examples
{
    public class FxOption
    {
        public string CurrencyPair { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CurrentQuoteId { get; set; }
    }

    public class AcceptQuoteEvent
    {
        public string QuoteId { get; set; }
    }

    public class BookingPipelineContext : IPipelineInstanceContext
    {
        private bool _isCanceled;

        public BookingPipelineContext(AcceptQuoteEvent initialEvent)
        {
            Event = initialEvent;
        }

        public AcceptQuoteEvent Event { get; private set; }

        public bool IsCanceled
        {
            get { return _isCanceled; }
        }

        public void Cancel()
        {
            _isCanceled = true;
        }
    }

    public class ReferenceDatesEventProcessor  : DisposableBase, IEventProcessor
    {
        private readonly IRouter<FxOption> _router;
        private readonly IBookingService _bookingService;
       // private readonly IPipeline<FxOption> _getReferenceDatesPipeline;
        private readonly EspSerialDisposable _inflightWorkItem = new EspSerialDisposable();

        public ReferenceDatesEventProcessor(IRouter<FxOption> router, IBookingService bookingService)
        {
            _router = router;
            _bookingService = bookingService;
//            _getReferenceDatesPipeline = _router
//               .ConfigurePipeline()
//               .SelectMany(m => _bookingService.AcceptQuote(m.QuoteId), OnQuoteAccepted)
//               .SelectMany(m => _bookingService.AcceptQuote(m.QuoteId), OnTermsheetReceived)
//               .Create();
            AddDisposable(_inflightWorkItem);
        }

//        public void Start()
//        {
//            AddDisposable(_router
//                .GetEventObservable<AcceptQuoteEvent>(ObservationStage.Committed)
//                .Observe((model, userChangedCCyPairEvent, context) =>
//                {
//                    IPipelineInstance<FxOption> instance = _getReferenceDatesPipeline.CreateInstance();
//                    _inflightWorkItem.Disposable = instance;
//                    instance.Run(model, ex => { });
//                })
//            );
//        }


        public void Start()
        {
            // By adding a pipeline context we can flow that right through the stack and provide it 
            // anytime we invoke a deletage, for example on each step or on a pipeline instance exception.
            //
            // We can solve the problem of 'should run' not by returning empty observables (which the consumer may not own)
            // but rather with a simple where filter that comes before that step. The pipeline context enables this.
            IDisposable disposable = _router
                .ConfigurePipeline<FxOption, BookingPipelineContext, AcceptQuoteEvent>((m, e, c) => new BookingPipelineContext(e))
                // we don't need where, a do with a context that can be canceled is more inline with the 
                // existing api 
                //.Where((model, pipeLineContext) => pipeLineContext.Event.QuoteId == model.CurrentQuoteId)
                // select many functions much the same as select many in Rx, we stay subscribed to the 
                // response stream and invoke the next step for each yield\
                .SelectMany((model, pipelineContext) => _bookingService.AcceptQuote(model.CurrentQuoteId), OnQuoteAccepted)
                .SelectMany((model, pipelineContext) => _bookingService.GenerateTermsheet(model.CurrentQuoteId), OnTermsheetReceived)
                .Do(OnBookingComlete)
                // Run wraps Create and for eacn event creates a  new instance (via CreateInstance).
                // so efictively each instance acts on it's own right, however all instances can be 
                // disposed usng the disposable returned from Run().
                .Run((pipelinContext, exception) => { });
        }

//        public void Start2()
//        {
//            AddDisposable(_router
//                .GetEventObservable<UserChangedCCyPairEvent>()
//                .
//                .OnEvent<UserChangedCCyPairEvent>((m, e, c) => { }, ObservationStage.Committed)
//                .ThenSubscribeTo(m => _referenceDataService.GetFixingDates(m.CurrencyPair), OnFixingDatesReceived)
//                .Run()
//            );
//
//            AddDisposable(_router
//                .BeginConfigureAsyncEventStream()
//                .OnEvent<UserChangedCCyPairEvent>((m, e, c) => { }, ObservationStage.Committed)
//                .ThenSubscribeTo(m => _referenceDataService.GetFixingDates(m.CurrencyPair), OnFixingDatesReceived)
//                .Run()
//            );
//
//            AddDisposable(_router
//                .Pipeline()
//                .OnEvent<UserChangedCCyPairEvent>((m, e, c) => { }, ObservationStage.Committed)
//                .Do((m) => { })
//                .SelectMany(m => _referenceDataService.GetFixingDates(m.CurrencyPair), OnFixingDatesReceived)
//                .Do((m) => { })
//                .Run()
//            );
//        }

        private void OnQuoteAccepted(FxOption model, string response)
        {
            // apply dates to model
        }

        private void OnTermsheetReceived(FxOption model, string response)
        {
            // apply dates to model
        }

        private void OnBookingComlete(FxOption model, BookingPipelineContext context)
        {
        }

    }
}
#endif