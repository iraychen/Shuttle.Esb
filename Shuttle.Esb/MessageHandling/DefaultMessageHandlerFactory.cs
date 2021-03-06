using System;
using System.Collections.Generic;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DefaultMessageHandlerFactory : MessageHandlerFactory
	{
		private readonly Dictionary<Type, Type> _messageHandlerTypes = new Dictionary<Type, Type>();

		private readonly ILog _log;

		public DefaultMessageHandlerFactory()
		{
			_log = Log.For(this);
		}

		private void AddMessageHandlerType(Type messageHandlerType)
		{
			var messageHandler = typeof (IMessageHandler<>);

			foreach (var type in messageHandlerType.GetInterfaces())
			{
				if (!type.IsGenericType || type.GetGenericTypeDefinition() != messageHandler)
				{
					continue;
				}

				AddMessageTypeHandler(type.GetGenericArguments()[0], messageHandlerType);
			}
		}

		private void AddMessageTypeHandler(Type messageType, Type messageHandlerType)
		{
			if (_messageHandlerTypes.ContainsKey(messageType))
			{
				return;
			}

			_messageHandlerTypes.Add(messageType, messageHandlerType);

			_log.Information(string.Format(EsbResources.MessageHandlerFactoryHandlerRegistered, messageType.FullName,
				messageHandlerType.FullName));
		}

		public override object CreateHandler(object message)
		{
			var messageType = message.GetType();

			if (_messageHandlerTypes.ContainsKey(messageType))
			{
				return Activator.CreateInstance(_messageHandlerTypes[messageType]);
			}

			return null;
		}

		public override IEnumerable<Type> MessageTypesHandled
		{
			get { return _messageHandlerTypes.Keys; }
		}

	    public override IMessageHandlerFactory RegisterHandlers(Assembly assembly)
	    {
            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes(typeof(IMessageHandler<>), assembly))
            {
                if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    RegisterHandler(type);
                }
                else
                {
                    _log.Warning(string.Format(EsbResources.DefaultMessageHandlerFactoryNoDefaultConstructor, type.FullName));
                }
            }

	        return this;
	    }

	    public override IMessageHandlerFactory RegisterHandler(Type type)
	    {
            Guard.AgainstNull(type, "type");

            AddMessageHandlerType(type);

	        return this;
	    }
	}
}