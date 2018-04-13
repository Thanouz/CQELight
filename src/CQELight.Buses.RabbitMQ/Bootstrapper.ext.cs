﻿using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Buses.RabbitMQ.Server;
using CQELight.IoC;
using System.Linq;

namespace CQELight.Buses.RabbitMQ
{
    public static class BootstrapperExtensions
    {

        #region Public static methods

        /// <summary>
        /// Use RabbitMQ client to publish events and commands to a rabbitMQ instance.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configuration">Configuration to use RabbitMQ.</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseRabbitMQClientBus(this Bootstrapper bootstrapper, RabbitMQClientBusConfiguration configuration = null)
        {
            var service = RabbitMQBootstrappService.Instance;

            service.BootstrappAction += () =>
            {
                bootstrapper.AddIoCRegistrations(
                    new TypeRegistration(typeof(RabbitMQClientBus), typeof(IDomainEventBus)),
                    new TypeRegistration(typeof(RabbitMQClientBus), typeof(ICommandBus)),
                    new InstanceTypeRegistration(configuration ?? RabbitMQClientBusConfiguration.Default,
                        typeof(RabbitMQClientBusConfiguration)));
            };

            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        public static Bootstrapper StartRabbitMQServer(this Bootstrapper bootstrapper, RabbitMQServerConfiguration configuration = null)
        {
            var service = RabbitMQBootstrappService.Instance;

            service.BootstrappAction += () =>
            {
                var server = new RabbitMQServer(null, configuration);
                server.Start();
            };

            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }
        
        #endregion

    }
}
