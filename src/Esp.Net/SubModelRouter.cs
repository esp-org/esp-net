#region copyright
// Copyright 2015 Keith Woods
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;

namespace Esp.Net
{
    internal class SubModelRouter<TModel, TSubModel> : IRouter<TSubModel>
    {
        private readonly Func<TModel, TSubModel> _selector;
        private readonly object _modelIid;
        private readonly IRouter _underlying;

        public SubModelRouter(object modelIid, IRouter underlying, Func<TModel, TSubModel> selector)
        {
            _modelIid = modelIid;
            _underlying = underlying;
            _selector = selector;
        }

        public IModelObservable<TSubModel> GetModelObservable()
        {
            return _underlying.GetModelObservable<TModel>(_modelIid).Select(_selector);
        }

        public IEventObservable<TSubModel, TEvent, IEventContext> GetEventObservable<TEvent>(ObservationStage observationStage = ObservationStage.Normal)
        {
            return _underlying.GetEventObservable<TModel, TEvent>(_modelIid, observationStage).Select(_selector);
        }

        public IEventObservable<TSubModel, TBaseEvent, IEventContext> GetEventObservable<TBaseEvent>(Type eventType, ObservationStage observationStage = ObservationStage.Normal)
        {
            return _underlying.GetEventObservable<TModel, TBaseEvent>(_modelIid, eventType, observationStage).Select(_selector);
        }

        public void PublishEvent<TEvent>(TEvent @event)
        {
            _underlying.PublishEvent(_modelIid, @event);
        }

        public void PublishEvent(object @event)
        {
            _underlying.PublishEvent(_modelIid, @event);
        }

        public void ExecuteEvent<TEvent>(TEvent @event)
        {
            _underlying.ExecuteEvent(_modelIid, @event);
        }

        public void ExecuteEvent(object @event)
        {
            _underlying.ExecuteEvent(_modelIid, @event);
        }

        public void BroadcastEvent<TEvent>(TEvent @event)
        {
            _underlying.BroadcastEvent(@event);
        }

        public void BroadcastEvent(object @event)
        {
            _underlying.BroadcastEvent(@event);
        }
    }
}