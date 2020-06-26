﻿namespace MassTransit.Azure.ServiceBus.Core.Tests
{
    using System;
    using NUnit.Framework;


    static class Configuration
    {
        public static string KeyName =>
            TestContext.Parameters.Exists(nameof(KeyName))
                ? TestContext.Parameters.Get(nameof(KeyName))
                : Environment.GetEnvironmentVariable("MT_ASB_KEYNAME") ?? "MassTransitBuild";

        public static string ServiceNamespace =>
            TestContext.Parameters.Exists(nameof(ServiceNamespace))
                ? TestContext.Parameters.Get(nameof(ServiceNamespace))
                : Environment.GetEnvironmentVariable("MT_ASB_NAMESPACE") ?? "masstransit-build";

        public static string SharedAccessKey =>
            TestContext.Parameters.Exists(nameof(SharedAccessKey))
                ? TestContext.Parameters.Get(nameof(SharedAccessKey))
                : Environment.GetEnvironmentVariable("MT_ASB_KEYVALUE") ?? "YfN2b8jT84759bZy5sMhd0P+3K/qHqO81I5VrNrJYkI=";

        public static string StorageAccount =>
            TestContext.Parameters.Exists(nameof(StorageAccount))
                ? TestContext.Parameters.Get(nameof(StorageAccount))
                : Environment.GetEnvironmentVariable("MT_AZURE_STORAGE_ACCOUNT") ?? "";
    }
}
