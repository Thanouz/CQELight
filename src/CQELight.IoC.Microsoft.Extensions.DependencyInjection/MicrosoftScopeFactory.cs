﻿using CQELight.Abstractions.IoC.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC.Microsoft.Extensions.DependencyInjection
{
    class MicrosoftScopeFactory : IScopeFactory
    {

        #region Members

        private readonly ServiceProvider serviceProvider;
        private readonly IServiceCollection services;

        #endregion

        #region Ctor

        public MicrosoftScopeFactory(IServiceCollection services)
        {
            serviceProvider = services.BuildServiceProvider();
            this.services = services;
        }

        #endregion

        #region IScopeFactory

        public IScope CreateScope()
            => new MicrosoftScope(serviceProvider.CreateScope(), services);

        #endregion
    }
}
