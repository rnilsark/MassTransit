module.exports = {
  title: 'MassTransit',
  description: 'A free, open-source distributed application framework for .NET.',
  head: [
      ['link', { rel: "apple-touch-icon", sizes: "180x180", href: "/apple-touch-icon.png"}],
      ['link', { rel: "icon", type: "image/png", sizes: "32x32", href: "/favicon-32x32.png"}],
      ['link', { rel: "icon", type: "image/png", sizes: "16x16", href: "/favicon-16x16.png"}],
      ['link', { rel: "manifest", href: "/site.webmanifest"}],
      ['link', { rel: "mask-icon", href: "/safari-pinned-tab.svg", color: "#3a0839"}],
      ['link', { rel: "shortcut icon", href: "/favicon.ico"}],
      ['meta', { name: "msapplication-TileColor", content: "#3a0839"}],
      ['meta', { name: "msapplication-config", content: "/browserconfig.xml"}],
      ['meta', { name: "theme-color", content: "#ffffff"}],
    ],  
  plugins: [
    '@vuepress/active-header-links',
    '@vuepress/back-to-top',
    [
      '@vuepress/google-analytics', {
        'ga': 'UA-156512132-1'
      }      
    ]
  ],
  themeConfig: {
    logo: '/mt-logo-small.png',
    algolia: {
      apiKey: 'e458b7be70837c0e85b6b229c4e26664',
      indexName: 'masstransit'
    },
    nav: [
      { text: "Discord", link: "/discord" },
      { text: 'NuGet', link: 'https://nuget.org/packages/MassTransit' }
    ],
    sidebarDepth: 1,
    sidebar: [
      {
        title: 'Getting Started',
        path: '/getting-started/',
        collapsable: false,
        children: [
          '/getting-started/live-coding',
          '/getting-started/upgrade-v6'
        ]
      },
      {
        title: 'Using MassTransit',
        path: '/usage/',
        collapsable: false,
        children: [
          '/usage/configuration',
          {
            title: 'Transports',
            path: '/usage/transports/',
            collapsable: true,
            children: [
              '/usage/transports/rabbitmq',
              '/usage/transports/azure-sb',
              '/usage/transports/activemq',
              '/usage/transports/amazonsqs',
              '/usage/transports/in-memory'
            ]
          },
          {
            title: 'Riders',
            path: '/usage/riders/',
            collapsable: true,
            children: [
              '/usage/riders/kafka',
              '/usage/riders/eventhub'
            ]
          },
          '/usage/mediator',
          '/usage/messages',
          '/usage/consumers',
          '/usage/producers',
          '/usage/exceptions',
          '/usage/requests',
          {
            title: 'Sagas',
            path: '/usage/sagas/',
            collapsable: true,
            children: [
              '/usage/sagas/automatonymous',
              '/usage/sagas/consumer-saga',
              {
                title: 'Persistence',
                path: '/usage/sagas/persistence',
                collapsable: false,
                children: [
                  '/usage/sagas/efcore',
                  '/usage/sagas/dapper',
                  '/usage/sagas/documentdb',
                  '/usage/sagas/ef',
                  '/usage/sagas/marten',
                  '/usage/sagas/mongodb',
                  '/usage/sagas/nhibernate',
                  '/usage/sagas/redis',
                  '/usage/sagas/session'
                ]
              }
            ]
          },
          {
            title: 'Containers',
            path: '/usage/containers/',
            collapsable: true,
            children: [
              ['/usage/containers/definitions', 'Definitions'],
              ['/usage/containers/msdi', 'Microsoft'],
              '/usage/containers/multibus',
              ['/usage/containers/autofac', 'Autofac'],
              ['/usage/containers/castlewindsor', 'Castle Windsor'],
              ['/usage/containers/simpleinjector', 'Simple Injector'],
              ['/usage/containers/structuremap', 'StructureMap']
            ]
          },
          ['/usage/testing', 'Testing'],
          {
            title: 'Advanced',
            collapsable: true,
            sidebarDepth: 2,
            children: [
              {
                title: 'Scheduling',
                path: '/advanced/scheduling/',
                collapsable: true,
                children: [
                  '/advanced/scheduling/scheduling-api',
                  '/advanced/scheduling/activemq-delayed',
                  '/advanced/scheduling/amazonsqs-scheduler',
                  '/advanced/scheduling/azure-sb-scheduler',
                  '/advanced/scheduling/rabbitmq-delayed',
                  '/advanced/scheduling/hangfire'
                ]
              },
              {
                title: 'Courier',
                path: '/advanced/courier/',
                collapsable: true,
                children: [
                  '/advanced/courier/activities',
                  '/advanced/courier/builder',
                  '/advanced/courier/execute',
                  '/advanced/courier/events',
                  '/advanced/courier/subscriptions'
                ]
              },
              {
                title: 'Middleware',
                path: '/advanced/middleware/',
                collapsable: true,
                children: [
                  '/advanced/middleware/receive',
                  '/advanced/middleware/circuit-breaker',
                  '/advanced/middleware/rate-limiter',
                  '/advanced/middleware/transactions',
                  '/advanced/middleware/custom',
                  '/advanced/middleware/scoped'
                ]
              },
              {
                title: 'Conductor',
                path: '/advanced/conductor/',
                collapsable: true,
                children: [
                  '/advanced/conductor/configuration'
                ]
              },
              '/usage/message-data',
              {
                title: 'Monitoring',
                collapsable: true,
                children: [
                  '/advanced/monitoring/diagnostic-source',
                  '/advanced/monitoring/prometheus',
                  '/advanced/monitoring/applications-insights',
                  '/advanced/monitoring/perfcounters',
                ]
              },
              '/advanced/connect-endpoint',
              '/advanced/observers',
              {
                title: 'Topology',
                path: '/advanced/topology/',
                collapsable: true,
                children: [
                  '/advanced/topology/message',
                  '/advanced/topology/publish',
                  '/advanced/topology/send',
                  '/advanced/topology/consume',
                  '/advanced/topology/conventions',
                  '/advanced/topology/rabbitmq',
                  '/advanced/topology/servicebus',
                ]
              },
              {
                title: 'SignalR',
                path: '/advanced/signalr/',
                collapsable: true,
                children: [
                  '/advanced/signalr/quickstart',
                  '/advanced/signalr/hub_endpoints',
                  '/advanced/signalr/interop',
                  '/advanced/signalr/sample',
                  '/advanced/signalr/considerations'
                ]
              },
              'advanced/audit',
              'advanced/batching',
              'advanced/job-consumers'
            ]
          }
        ]
      },
      {
        title: 'Getting Help',
        path: '/learn/',
        collapsable: true,
        children: [
          '/troubleshooting/common-gotchas',
          '/troubleshooting/show-config',
          '/learn/analyzers',
          '/learn/samples',
          '/learn/videos',
          '/learn/courses',
          '/learn/loving-the-community',
          '/learn/contributing'
        ]
      },
      {
        title: "Platform",
        path: '/platform/',
        collapsable: true,
        children: [
          '/platform/configuration'
        ]
      },
      {
        title: "Reference",
        children: [
          '/architecture/packages',
          '/architecture/interoperability',
          '/architecture/nservicebus',
          '/architecture/versioning',
          '/architecture/newid',
          '/architecture/encrypted-messages',
          '/architecture/green-cache',
          '/architecture/history'
        ]
      }
    ],
    searchPlaceholder: 'Search...',
    lastUpdated: 'Last Updated',
    repo: 'MassTransit/MassTransit',

    docsRepo: 'MassTransit/MassTransit',
    docsDir: 'docs',
    docsBranch: 'develop',
    editLinks: true,
    editLinkText: 'Help us by improving this page!'
  }
}
