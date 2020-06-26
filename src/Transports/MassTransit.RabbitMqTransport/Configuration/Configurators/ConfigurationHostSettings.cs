namespace MassTransit.RabbitMqTransport.Configurators
{
    using System;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using MassTransit.Configuration;
    using Metadata;
    using RabbitMQ.Client;


    class ConfigurationHostSettings :
        RabbitMqHostSettings
    {
        static readonly bool _batchPublishEnabled = !AppContext.TryGetSwitch(AppContextSwitches.RabbitMqBatchPublish, out var isEnabled) || !isEnabled;

        readonly ConfigurationBatchSettings _batchSettings;
        readonly Lazy<Uri> _hostAddress;

        public ConfigurationHostSettings()
        {
            var defaultOptions = new SslOption();
            SslProtocol = SslProtocols.Tls;

            AcceptablePolicyErrors = defaultOptions.AcceptablePolicyErrors | SslPolicyErrors.RemoteCertificateChainErrors;

            PublisherConfirmation = true;

            _batchSettings = new ConfigurationBatchSettings();

            RequestedConnectionTimeout = TimeSpan.FromSeconds(10);

            ClientProvidedName = HostMetadataCache.Host.ProcessName;

            _hostAddress = new Lazy<Uri>(FormatHostAddress);
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public TimeSpan Heartbeat { get; set; }
        public bool Ssl { get; set; }
        public SslProtocols SslProtocol { get; set; }
        public string SslServerName { get; set; }
        public SslPolicyErrors AcceptablePolicyErrors { get; set; }
        public string ClientCertificatePath { get; set; }
        public string ClientCertificatePassphrase { get; set; }
        public X509Certificate ClientCertificate { get; set; }
        public bool UseClientCertificateAsAuthenticationIdentity { get; set; }
        public LocalCertificateSelectionCallback CertificateSelectionCallback { get; set; }
        public RemoteCertificateValidationCallback CertificateValidationCallback { get; set; }
        public IRabbitMqEndpointResolver EndpointResolver { get; set; }
        public string ClientProvidedName { get; set; }
        public bool PublisherConfirmation { get; set; }
        public Uri HostAddress => _hostAddress.Value;
        public ushort RequestedChannelMax { get; set; }
        public TimeSpan RequestedConnectionTimeout { get; set; }

        public BatchSettings BatchSettings => _batchSettings;

        public void ConfigureBatch(Action<ConfigurationBatchSettings> configure)
        {
            configure?.Invoke(_batchSettings);
        }

        Uri FormatHostAddress()
        {
            return new RabbitMqHostAddress(Host, Port, VirtualHost);
        }


        public class ConfigurationBatchSettings :
            BatchSettings
        {
            public ConfigurationBatchSettings()
            {
                Enabled = _batchPublishEnabled;
                MessageLimit = 100;
                SizeLimit = 64 * 1024;
                Timeout = TimeSpan.FromMilliseconds(4);
            }

            public bool Enabled { get; set; }
            public int MessageLimit { get; set; }
            public int SizeLimit { get; set; }
            public TimeSpan Timeout { get; set; }
        }
    }
}
