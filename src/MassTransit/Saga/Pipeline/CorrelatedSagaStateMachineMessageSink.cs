// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Saga.Pipeline
{
	using System;
	using System.Linq.Expressions;
	using log4net;
	using Magnum.StateMachine;
	using MassTransit.Pipeline;

	public class CorrelatedSagaStateMachineMessageSink<TSaga, TMessage> :
		SagaMessageSinkBase<TSaga, TMessage>
		where TMessage : class, CorrelatedBy<Guid>
		where TSaga : SagaStateMachine<TSaga>, ISaga
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(CorrelatedSagaStateMachineMessageSink<TSaga, TMessage>).ToFriendlyName());

		private readonly DataEvent<TSaga, TMessage> _dataEvent;
		private readonly Expression<Func<TSaga, TMessage, bool>> _selector;

		public CorrelatedSagaStateMachineMessageSink(ISubscriberContext context,
		                                             IServiceBus bus,
		                                             ISagaRepository<TSaga> repository,
		                                             ISagaPolicy<TSaga, TMessage> policy,
		                                             DataEvent<TSaga, TMessage> dataEvent)
			: base(context, bus, repository, policy)
		{
			_dataEvent = dataEvent;

			_selector = CreateCorrelatedSelector();
		}

		protected override Expression<Func<TSaga, TMessage, bool>> FilterExpression
		{
			get { return _selector; }
		}

		protected override void ConsumerAction(TSaga saga, TMessage message)
		{
			if(_log.IsDebugEnabled)
				_log.DebugFormat("RaiseEvent: {0} {1} {2}", typeof(TSaga).Name, _dataEvent.Name, saga.CorrelationId);

			saga.RaiseEvent(_dataEvent, message);
		}
	}
}