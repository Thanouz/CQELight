using CQELight.TestFramework;
using System;
using Xunit;
using CQELight.IoC.Autofac;
using Autofac;
using FluentAssertions;
using CQELight.Bootstrapping.Notifications;
using System.Collections.Generic;
using CQELight.Abstractions.IoC.Interfaces;

namespace CQELight.IoC.Autofac.Integration.Tests
{
    public class BootstrapperExtTests : BaseUnitTestClass
    {
        #region Ctor & members

        private interface ITest
        {
            string Data { get; }
        }
        private class Test : ITest
        {
            public Test(string data)
            {
                Data = data;
            }
            public Test()
            {
                Data = "ctor_test";
            }

            public string Data { get; private set; }
        }

        private readonly ContainerBuilder _builder;

        public BootstrapperExtTests()
        {
            _builder = new ContainerBuilder();
        }

        #endregion

        #region InstanceTypeRegistration

        [Fact]
        public void BootstrapperExt_CustomRegistration_InstanceTypeRegistration_AsExpected()
        {
            new Bootstrapper()
                .AddIoCRegistration(new InstanceTypeRegistration(new Test("test"), typeof(ITest)))
                .UseAutofacAsIoC(_builder)
                .Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("test");
            }
        }

        #endregion

        #region TypeRegistration

        [Fact]
        public void BootstrapperExt_CustomRegistration_TypeRegistration_AsExpected()
        {
            new Bootstrapper()
                .AddIoCRegistration(new TypeRegistration(typeof(Test), typeof(ITest)))
                .UseAutofacAsIoC(_builder).
                Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("ctor_test");
            }
        }

        #endregion

        #region FactoryRegistration

        [Fact]
        public void BootstrapperExt_CustomRegistration_FactoryRegistration_AsExpected()
        {
            new Bootstrapper()
                .AddIoCRegistration(new FactoryRegistration(() => new Test("fact_test"), typeof(ITest)))
                .UseAutofacAsIoC(_builder)
                .Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("fact_test");
            }
        }

        #endregion

        #region Use scope

        [Fact]
        public void BootstrapperExt_Scope_CustomRegistration_InstanceTypeRegistration_AsExpected()
        {
            new Bootstrapper()
                .AddIoCRegistration(new InstanceTypeRegistration(new Test("test"), typeof(ITest)))
                .UseAutofacAsIoC(_builder.Build())
                .Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("test");
            }
        }

        [Fact]
        public void BootstrapperExt_Scope_CustomRegistration_TypeRegistration_AsExpected()
        {
            new Bootstrapper()
                .AddIoCRegistration(new TypeRegistration(typeof(Test), typeof(ITest)))
                .UseAutofacAsIoC(_builder.Build()).
                Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("ctor_test");
            }
        }

        [Fact]
        public void BootstrapperExt_Scope_CustomRegistration_FactoryRegistration_AsExpected()
        {
            new Bootstrapper()
                .AddIoCRegistration(new FactoryRegistration(() => new Test("fact_test"), typeof(ITest)))
                .UseAutofacAsIoC(_builder.Build())
                .Bootstrapp();

            using (var s = DIManager.BeginScope())
            {
                var i = s.Resolve<ITest>();
                i.Data.Should().Be("fact_test");
            }
        }

        #endregion

        #region Autoload

        private interface IAutoLoadTest { }
        private class AutoLoadTest : IAutoLoadTest, IAutoRegisterType { }

        [Fact]
        public void AutoLoad_Should_CreateContainer_And_UserAutoRegisterTypes()
        {
            new Bootstrapper(new BootstrapperOptions { AutoLoad = true }).Bootstrapp();
            DIManager.IsInit.Should().BeTrue();
            using(var scope = DIManager.BeginScope())
            {
                scope.Resolve<IAutoLoadTest>().Should().NotBeNull();
            }
        }

        #endregion

    }
}
