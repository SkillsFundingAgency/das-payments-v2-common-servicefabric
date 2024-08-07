﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.ServiceFabric.Core.Messaging;

namespace SFA.DAS.Payments.ServiceFabric.Core.UnitTests.Messaging
{
    [TestFixture]
    public class DuplicateEarningEventServiceTests
    {
        private AutoMock mocker;

        [SetUp]
        public void SetUp()
        {
            mocker = AutoMock.GetLoose();
            mocker.Mock<IActorDataCache<EarningEventKey>>()
                .Setup(cache => cache.Contains(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private T CreateEvent<T>() where T : PaymentsEvent, new()
        {
            return new T
            {
                EventId = Guid.NewGuid(),
                JobId = 123456,
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2021, Period = 1 },
                Ukprn = 1234,
                EventTime = DateTimeOffset.UtcNow,
                IlrFileName = "test-filename1.xml",
                IlrSubmissionDateTime = DateTime.Now,
                Learner = new Learner
                {
                    Uln = 12345678,
                    ReferenceNumber = "learn-ref"
                },
                LearningAim = new LearningAim
                {
                    StartDate = DateTime.Now.AddYears(-1),
                    FrameworkCode = 1,
                    FundingLineType = "funding-line",
                    PathwayCode = 2,
                    ProgrammeType = 3,
                    Reference = "aim-ref",
                    SequenceNumber = 4,
                    StandardCode = 5,
                }
            };
        }

        private ApprenticeshipContractType1EarningEvent CreateDefaultEarningEvent() => CreateEvent<ApprenticeshipContractType1EarningEvent>();

        [Test]
        public async Task IsDuplicate_Should_Return_False_For_New_Earning_Events()
        {
            var service = mocker.Create<DuplicateEarningEventService>();
            var isDuplicate = await service.IsDuplicate(CreateDefaultEarningEvent(), CancellationToken.None).ConfigureAwait(false);
            isDuplicate.Should().BeFalse();
        }

        [Test]
        public async Task IsDuplicate_Should_Return_True_For_Duplicate_Earning_Events()
        {
            mocker.Mock<IActorDataCache<EarningEventKey>>()
                .Setup(cache => cache.Contains(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var earningEvent = CreateDefaultEarningEvent();
            var service = mocker.Create<DuplicateEarningEventService>();
            var isDuplicate = await service.IsDuplicate(CreateDefaultEarningEvent(), CancellationToken.None).ConfigureAwait(false);
            isDuplicate.Should().BeTrue();
        }
    }
}