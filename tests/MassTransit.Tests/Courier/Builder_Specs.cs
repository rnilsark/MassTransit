﻿namespace MassTransit.Tests.Courier
{
    using System;
    using MassTransit.Courier;
    using MassTransit.Testing;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using TestFramework;


    [TestFixture]
    public class Using_a_class_on_a_routing_slip :
        InMemoryActivityTestFixture
    {
        [Test]
        public void Should_properly_map_the_types()
        {
            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            var cmd3 = new ActivityMessageThreeCmd {Data = "Msg Three in Routing Slip."};
            builder.AddActivity("ActivityMessageThreeCmd", new Uri("loopback://localhost/exec_ActivityMessageThreeCmd"), cmd3);
        }


        public interface ITestResult
        {
            string MyResult { get; }
        }


        public interface IActivityMessageThreeCmd
        {
            ITestResult TestResult { get; }
            string Data { get; }
        }


        public class ActivityMessageThreeCmd : IActivityMessageThreeCmd
        {
            public ITestResult TestResult { get; set; }
            public string Data { get; set; }
        }


        protected override void SetupActivities(BusTestHarness testHarness)
        {
        }
    }


    [TestFixture]
    public class Cyclical_references_in_an_argument :
        InMemoryActivityTestFixture
    {
        [Test]
        public void Should_not_crater_the_planet()
        {
            var outer = new Outer();
            outer.Inners = new Inner[2]
            {
                new Inner
                {
                    Parent = outer,
                    Value = "First"
                },
                new Inner
                {
                    Parent = outer,
                    Value = "Second"
                }
            };

            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            Assert.Throws<JsonSerializationException>(() =>
                builder.AddActivity("Activity", new Uri("loopback://localhost/execute_activity"), new {Content = outer}));
        }


        public class Outer
        {
            public Inner[] Inners { get; set; }
        }


        public class Inner
        {
            public Outer Parent { get; set; }
            public string Value { get; set; }
        }


        protected override void SetupActivities(BusTestHarness testHarness)
        {
        }
    }
}
