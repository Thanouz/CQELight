﻿using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Implementations.IoC;
using CQELight.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.IoC.Microsoft.Extensions.DependencyInjection
{
    class MicrosoftScope : DisposableObject, IScope
    {
        #region Members

        private readonly IServiceScope scope;
        private readonly IServiceCollection services;

        #endregion

        #region Ctor

        public MicrosoftScope(IServiceScope scope, IServiceCollection services)
        {
            this.scope = scope;
            this.services = services;
        }

        ~MicrosoftScope() => Dispose(false);

        #endregion

        #region IScope methods

        public bool IsDisposed { get; private set; }

        public IScope CreateChildScope(Action<ITypeRegister> typeRegisterAction = null)
        {
            if (typeRegisterAction != null)
            {
                var childrenCollection = services.Clone();

                var typeRegister = new TypeRegister();
                typeRegisterAction(typeRegister);
                MicrosoftRegistrationHelper.RegisterContextTypes(childrenCollection, typeRegister);

                return new MicrosoftScope(
                    childrenCollection.BuildServiceProvider().CreateScope(),
                    childrenCollection);
            }
            return new MicrosoftScope(
                scope,
                services);
        }

        public T Resolve<T>(params IResolverParameter[] parameters) where T : class
        {
            if (parameters.Length > 0)
            {
                throw new NotSupportedException("Microsoft.Extensions.DependencyInjection doesn't officially supports parameters injection during runtime. You should register parameters retrieving via a factory or change to another IoC container provider that supports parameters injection at runtime.");
            }
            return scope.ServiceProvider.GetService<T>();
        }

        public object Resolve(Type type, params IResolverParameter[] parameters)
        {
            if (parameters.Length > 0)
            {
                throw new NotSupportedException("Microsoft.Extensions.DependencyInjection doesn't officially supports parameters injection during runtime. You should register parameters retrieving via a factory or change to another IoC container provider that supports parameters injection at runtime.");
            }
            return scope.ServiceProvider.GetService(type);
        }

        public IEnumerable<T> ResolveAllInstancesOf<T>() where T : class => scope.ServiceProvider.GetServices<T>();

        public IEnumerable ResolveAllInstancesOf(Type t) => scope.ServiceProvider.GetServices(t);

        #endregion

        #region Overriden methods

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            try
            {
                scope.Dispose();
            }
            catch
            {
                //No throw on disposal
            }
            base.Dispose(disposing);
        }

        #endregion

    }
}
