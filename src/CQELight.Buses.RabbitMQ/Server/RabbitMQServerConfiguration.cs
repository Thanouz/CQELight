﻿using CQELight.Events.Serializers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Configuration class to setup RabbitMQ server behavior.
    /// </summary>
    [Obsolete("Use ")]
    public class RabbitMQServerConfiguration : AbstractBaseConfiguration
    {
        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQServerConfiguration Default
            => new RabbitMQServerConfiguration("default",
                new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                }, QueueConfiguration.Empty);

        #endregion

        #region Properties

        /// <summary>
        /// Specific configuration of the queue.
        /// </summary>
        public QueueConfiguration QueueConfiguration { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new server configuration on a rabbitMQ server.
        /// </summary>
        /// <param name="emiter">Id/Name of application that is using the bus</param>
        /// <param name="connectionFactory">Configured connection factory</param>
        /// <param name="queueConfiguration">Queue configuration.</param>
        public RabbitMQServerConfiguration(string emiter,
                                           ConnectionFactory connectionFactory,
                                           QueueConfiguration queueConfiguration)
            : base(emiter, connectionFactory, null, null)
        {
            QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
        }

        #endregion

    }
}
