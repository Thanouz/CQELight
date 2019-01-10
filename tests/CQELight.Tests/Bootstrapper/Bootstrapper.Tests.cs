﻿using CQELight.Bootstrapping.Notifications;
using CQELight.IoC;
using CQELight.TestFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.Tests
{
    public class BootstrapperTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region AddIoCRegistration

        [Fact]
        public void Bootstrapper_AddIoCRegistration_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().AddIoCRegistration(null));

            var b = new Bootstrapper();
            b.AddIoCRegistration(new TypeRegistration(typeof(DateTime), typeof(DateTime)));
            b.IoCRegistrations.Should().HaveCount(1);
            b.IoCRegistrations.First().Should().BeOfType<TypeRegistration>();
        }

        #endregion

        #region ConfigureDispatcher

        [Fact]
        public void Bootstrapper_ConfigureDispatcher_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().ConfigureDispatcher(null));
        }

        #endregion

        #region AddService

        [Fact]
        public void Bootstrapper_AddService_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new Bootstrapper().AddService(null));
        }

        [Fact]
        public void Bootstrapper_AddService_StrictMode_AlreadExists()
        {
            var m1 = new Mock<IBootstrapperService>();
            m1.Setup(m => m.ServiceType).Returns(BootstrapperServiceType.IoC);
            var m2 = new Mock<IBootstrapperService>();
            m2.Setup(m => m.ServiceType).Returns(BootstrapperServiceType.IoC);

            Assert.Throws<InvalidOperationException>(() => new Bootstrapper(true).AddService(m1.Object).AddService(m2.Object));
        }

        #endregion

        #region Bootstrapp

        [Fact]
        public void Bootstrapper_Bootstrapp_Non_Optimal_Mode()
        {
            var b = new Bootstrapper(false, false);
            var notifs = b.Bootstrapp();
            notifs.Should().BeEmpty();
        }

        [Fact]
        public void Bootstrapper_Bootstrapp_Optimal_Mode()
        {
            var b = new Bootstrapper(false, true);
            var notifs = b.Bootstrapp();
            notifs.Should().HaveCount(4);
            notifs.All(n => n.Type == BootstrapperNotificationType.Warning).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.BusServiceMissing).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.DALServiceMissing).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.EventStoreServiceMissing).Should().BeTrue();
            notifs.Any(s => s.ContentType == BootstapperNotificationContentType.IoCServiceMissing).Should().BeTrue();
        }

        [Fact]
        public void Bootstrapper_Bootstrapp_BootstrappingContext_Services()
        {
            BootstrappingContext bootstrappContext = null;
            var iocServiceMock = new Mock<IBootstrapperService>();
            iocServiceMock
                .Setup(m => m.ServiceType).Returns(BootstrapperServiceType.IoC);
            iocServiceMock
                .Setup(m => m.BootstrappAction)
                .Returns((c) => bootstrappContext = c);

            var b = new Bootstrapper();
            b.AddService(iocServiceMock.Object);
            b.Bootstrapp();

            bootstrappContext.Should().NotBeNull();
            bootstrappContext.IsServiceRegistered(BootstrapperServiceType.IoC).Should().BeTrue();
            bootstrappContext.IsServiceRegistered(BootstrapperServiceType.Bus).Should().BeFalse();
            bootstrappContext.IsServiceRegistered(BootstrapperServiceType.DAL).Should().BeFalse();
            bootstrappContext.IsServiceRegistered(BootstrapperServiceType.EventStore).Should().BeFalse();
        }

        [Fact]
        public void Bootstrapper_Bootstrapp_BootstrappingContext_CheckIoCRegistrations()
        {
            BootstrappingContext bootstrappContext = null;
            var iocServiceMock = new Mock<IBootstrapperService>();
            iocServiceMock
                .Setup(m => m.ServiceType).Returns(BootstrapperServiceType.IoC);
            iocServiceMock
                .Setup(m => m.BootstrappAction)
                .Returns((c) => bootstrappContext = c);

            var b = new Bootstrapper();
            b.AddService(iocServiceMock.Object);
            b.AddIoCRegistration(new TypeRegistration(typeof(object), typeof(object), typeof(DateTime)));
            b.Bootstrapp();

            bootstrappContext.Should().NotBeNull();
            bootstrappContext.IsAbstractionRegisteredInIoC(typeof(object)).Should().BeTrue();
            bootstrappContext.IsAbstractionRegisteredInIoC(typeof(DateTime)).Should().BeTrue();
        }

        [Fact]
        public void Bootstrapper_Bootstrapp_IoCRegistrations_Tests()
        {
            var bootstrapperStrict = new Bootstrapper(strict: true);
            bootstrapperStrict.AddIoCRegistration(new TypeRegistration(typeof(object), typeof(object)));

            Assert.Throws<InvalidOperationException>(() => bootstrapperStrict.Bootstrapp());

            var bootstrapperLazy = new Bootstrapper();
            bootstrapperLazy.AddIoCRegistration(new TypeRegistration(typeof(object), typeof(object)));

            var notifs = bootstrapperLazy.Bootstrapp().ToList();
            notifs.Should().HaveCount(1);
            notifs[0].Type.Should().Be(BootstrapperNotificationType.Error);
            notifs[0].ContentType.Should().Be(BootstapperNotificationContentType.IoCRegistrationsHasBeenMadeButNoIoCService);
        }

        [Fact]
        public void Bootstrapp_Should_Returns_CustomNotification_To_System_Ones()
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(object), typeof(object)));

            bootstrapper.AddNotification(new BootstrapperNotification(BootstrapperNotificationType.Error, "error message"));

            var notifs = bootstrapper.Bootstrapp().OrderBy(n => n.ContentType).ToList();
            notifs.Should().HaveCount(2);
            notifs[0].Type.Should().Be(BootstrapperNotificationType.Error);
            notifs[0].ContentType.Should().Be(BootstapperNotificationContentType.IoCRegistrationsHasBeenMadeButNoIoCService);
            notifs[0].Message.Should().BeNullOrWhiteSpace();

            notifs[1].Type.Should().Be(BootstrapperNotificationType.Error);
            notifs[1].ContentType.Should().Be(BootstapperNotificationContentType.CustomServiceNotification);
            notifs[1].Message.Should().Be("error message");
        }

        [Fact]
        public void Bootstrapp_Should_Pass_Optimal_And_Strict_Flag_ToExtensions()
        {
            var bootsrapper = new Bootstrapper(true, true);
            var extMock = new Mock<IBootstrapperService>();

            bool strictPassed = false;
            bool optimalPassed = false;

            Action<BootstrappingContext> act = (BootstrappingContext c) =>
            {
                strictPassed = c.Strict;
                optimalPassed = c.CheckOptimal;
            };

            extMock.SetupGet(m => m.BootstrappAction).Returns(act);

            bootsrapper.AddService(extMock.Object);

            bootsrapper.Bootstrapp();

            strictPassed.Should().BeTrue();
            optimalPassed.Should().BeTrue();

        }

        #endregion


    }
}
