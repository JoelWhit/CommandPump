using CommandPump.Contract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CommandPump.Dispatch
{
    public class CommandDispatch : ICommandDispatch
    {
        private ConcurrentDictionary<Type, ICommandHandler> _commandHandlers;

        public CommandDispatch()
        {
            _commandHandlers = new ConcurrentDictionary<Type, ICommandHandler>();
        }

        /// <summary>
        /// Registers the specified command handler.
        /// </summary>
        public void RegisterHandler(ICommandHandler commandHandler)
        {
            var genericHandler = typeof(ICommandHandler<>);

            // find all the command handlers in the supplied class
            var supportedCommandTypes = commandHandler.GetType()
                .GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericHandler)
                .Select(iface => iface.GetGenericArguments()[0])
                .ToList();

            if (_commandHandlers.Keys.Any(registeredType => supportedCommandTypes.Contains(registeredType)))
            {
                throw new ArgumentException("The command handler supplied has already been registered.");
            }

            // Register this handler for each of he handled types.
            foreach (var commandType in supportedCommandTypes)
            {
                _commandHandlers.TryAdd(commandType, commandHandler);
            }
        }

        /// <summary>
        /// Attempts locate unregistered handlers via reflection and add them to the cache
        /// </summary>
        /// <param name="command"></param>
        private void LocateUnregisteredHandler(ICommand command)
        {
            // this is will be slow

            // get base generic type
            Type handlerGenericType = typeof(ICommandHandler<>);

            // inject the command type into the generic type
            Type commandHandlerType = handlerGenericType.MakeGenericType(command.GetType());

            // find the type that has the interface generic type<injected command>
            // looks for constructorless handlers
            Type commandHandler = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(p => commandHandlerType.IsAssignableFrom(p) && !p.IsInterface && p.GetConstructor(Type.EmptyTypes) != null).SingleOrDefault();

            // gets all the commands that the handler class can handle
            // by looking through the interfaces based on the ICommandHandler<> (generic) type
            List<Type> supportedCommandTypes = commandHandler.GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == handlerGenericType)
                .Select(iface => iface.GetGenericArguments()[0])
                .ToList();

            // construct the class and for each handler type add to cache - pointing to the same reference
            if (supportedCommandTypes.Count > 0)
            {
                ICommandHandler handler = (ICommandHandler)Activator.CreateInstance(commandHandler);

                foreach (Type supportedCommand in supportedCommandTypes)
                {
                    _commandHandlers.TryAdd(supportedCommand, handler);
                }
            }
        }

        public void Dispatch<T>(T command) where T : ICommand
        {
            if (!_commandHandlers.ContainsKey(command.GetType()))
            {
                LocateUnregisteredHandler(command);
            }

            ICommandHandler handler = null;

            if (_commandHandlers.TryGetValue(command.GetType(), out handler))
            {
                ((dynamic)handler).Handle((dynamic)command);
            }
            else
            {
                throw new Exception("No handler avaliable for command type: " + command.GetType().ToString());
            }
        }
    }
}
