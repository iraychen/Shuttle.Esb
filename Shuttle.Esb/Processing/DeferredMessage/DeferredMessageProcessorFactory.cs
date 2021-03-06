using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeferredMessageProcessorFactory : IProcessorFactory
	{
		private static readonly object Padlock = new object();
		private readonly IServiceBus _bus;
		private bool _instanced;

		public DeferredMessageProcessorFactory(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "Bus");

			_bus = bus;
		}

		public IProcessor Create()
		{
			lock (Padlock)
			{
				if (_instanced)
				{
					throw new ProcessorException(EsbResources.DeferredMessageProcessorInstanceException);
				}

				_instanced = true;

				return _bus.Configuration.Inbox.DeferredMessageProcessor;
			}
		}
	}
}