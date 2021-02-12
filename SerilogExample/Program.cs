using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerilogExample
{
    // https://github.com/serilog/serilog-settings-configuration

    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    Thread.CurrentThread.Name = "Main thread";

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogTrace("Hello, {name}!", "World");
                    logger.LogDebug("Hello, {name}!", "World");
                    logger.LogInformation("Hello, {name}!", "World");
                    logger.LogWarning("Hello, {name}!", "World");
                    logger.LogError("Hello, {name}!", "World");

                    logger.LogInformation("Hello, {@object}", new Person { Name = "Bill" });

                    logger.LogInformation("Destructure with max object nesting depth: {@NestedObject}",
                        new { FiveDeep = new { Two = new { Three = new { Four = new { Five = "the end" } } } } });

                    logger.LogInformation("Destructure with max string length: {@LongString}",
                        new { TwentyChars = "0123456789abcdefghij" });

                    logger.LogInformation("Destructure with max collection count: {@BigData}",
                        new { TenItems = new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" } });

                    logger.LogInformation("Destructure with policy to strip password: {@LoginData}",
                        new LoginData { Username = "BGates", Password = "isityearoflinuxyet" });
                }
                catch (Exception)
                {

                }
            }

            await host.RunAsync();
        }
   
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostBuilderContext, services) =>
            {
            })
            .UseSerilog((hostBuilderContext, loggerConfiguration) =>
            {
                loggerConfiguration
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({Application}/{SourceContext}/{MachineName}/{ThreadId}/{ThreadName}) {Message:lj}{NewLine}{Exception}",
                        theme: SystemConsoleTheme.Literate)
                    .MinimumLevel.ControlledBy(new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Verbose })
                    .MinimumLevel.Override("SerilogExample.Program", LogEventLevel.Verbose)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "Serilog Sample")
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName();

                //var configuration = LoadConfiguration();
                //loggerConfiguration.ReadFrom.Configuration(configuration);

            });

        private static IConfiguration LoadConfiguration()
        {
            //return new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            //    .Build();

            //var json =
            //@"
            //{
            //    ""Serilog"": {
            //    ""Using"":  [ ""Serilog.Sinks.Console"", ""Serilog.Sinks.File"" ],
            //    ""MinimumLevel"": ""Debug"",
            //    ""WriteTo"": [
            //        { ""Name"": ""Console"", ""Args"": { ""outputTemplate"" : ""{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}"" } },
            //        //{ ""Name"": ""File"", ""Args"": { ""path"": ""Logs/log.txt"" } }
            //    ],
            //    ""Enrich"": [ ""FromLogContext"", ""WithMachineName"", ""WithThreadId"" ],
            //    ""Destructure"": [
            //        { ""Name"": ""With"", ""Args"": { ""policy"": ""SerilogExample.CustomPolicy, SerilogExample"" } },
            //        { ""Name"": ""ToMaximumDepth"", ""Args"": { ""maximumDestructuringDepth"": 4 } },
            //        { ""Name"": ""ToMaximumStringLength"", ""Args"": { ""maximumStringLength"": 100 } },
            //        { ""Name"": ""ToMaximumCollectionCount"", ""Args"": { ""maximumCollectionCount"": 10 } }
            //    ],
            //    ""Properties"": {
            //    ""Application"": ""SerilogExample""
            //    }
            //    }
            //}
            //";

            var json =
            @"
            {
              ""Serilog"": {
                ""Using"": [ ""Serilog.Sinks.Console"" ],
                ""LevelSwitches"": { ""$controlSwitch"": ""Verbose"" },
                ""FilterSwitches"": { ""$filterSwitch"": ""Application = 'Serilog Example'"" },
                ""MinimumLevel"": {
                  ""Default"": ""Verbose"",
                  ""Override"": {
                    ""Microsoft"": ""Warning"",
                    ""MyApp.Something.Tricky"": ""Verbose""
                  }
                },
                //""WriteTo"" : [
                //  { ""Name"": ""Console"", ""Args"": { ""outputTemplate"" : ""{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}"" } },
                //],
                ""WriteTo:Sublogger"": {
                  ""Name"": ""Logger"",
                  ""Args"": {
                    ""configureLogger"": {
                      ""MinimumLevel"": ""Verbose"",
                      ""WriteTo"": [
                        {
                          ""Name"": ""Console"",
                          ""Args"": {
                            ""outputTemplate"": ""{Timestamp:o} [{Level:u3}] ({Application}/{SourceContext}/{MachineName}/{ThreadId}/{ThreadName}) {Message:lj}{NewLine}{Exception}"",
                            ""theme"": ""Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme::Literate, Serilog.Sinks.Console""
                          }
                        }
                      ]
                    },
                    ""restrictedToMinimumLevel"": ""Verbose"",
                    ""levelSwitch"": ""$controlSwitch""
                  }
                },
                //""WriteTo:Async"": {
                //  ""Name"": ""Async"",
                //  ""Args"": {
                //    ""configure"": [
                //      {
                //        ""Name"": ""File"",
                //        ""Args"": {
                //          ""path"": ""Logs/serilog-configuration-sample.txt"",
                //          ""outputTemplate"": ""{Timestamp:o} [{Level:u3}] ({Application}/{MachineName}/{ThreadId}/{ThreadName}) {Message}{NewLine}{Exception}""
                //        }
                //      }
                //    ]
                //  }
                //},
                //""WriteTo:ConditionalSink"": {
                //  ""Name"": ""Conditional"",
                //  ""Args"": {
                //    ""expression"": ""@Level in ['Error', 'Fatal']"",
                //    ""configureSink"": [
                //      {
                //        ""Name"": ""File"",
                //        ""Args"": {
                //          ""path"": ""Logs/serilog-configuration-sample-errors.txt""
                //        }
                //      }
                //    ]
                //  }
                //},
                ""Enrich"": [
                  ""FromLogContext"",
                  ""WithThreadId"",
                  ""WithThreadName"",
                  ""WithMachineName"",
                  //{
                  //  ""Name"": ""AtLevel"",
                  //  ""Args"": {
                  //    ""enrichFromLevel"": ""Error"",
                  //    ""configureEnricher"": [ ""WithThreadName"" ]
                  //  }
                  //},
                  //{
                  //  ""Name"": ""When"",
                  //  ""Args"": {
                  //    ""expression"": ""Application = 'SerilogExample'"",
                  //    ""configureEnricher"": [ ""WithMachineName"" ]
                  //  }
                  //}
                ],
                ""Properties"": {
                  ""Application"": ""Serilog Example"",
                },
                ""Destructure"": [
                  {
                    ""Name"": ""With"",
                    ""Args"": { ""policy"": ""SerilogExample.CustomPolicy, SerilogExample"" }
                  },
                  {
                    ""Name"": ""ToMaximumDepth"",
                    ""Args"": { ""maximumDestructuringDepth"": 3 }
                  },
                  {
                    ""Name"": ""ToMaximumStringLength"",
                    ""Args"": { ""maximumStringLength"": 10 }
                  },
                  {
                    ""Name"": ""ToMaximumCollectionCount"",
                    ""Args"": { ""maximumCollectionCount"": 5 }
                  }
                ],
                ""Filter"": [
                  //{
                  //  ""Name"": ""ControlledBy"",
                  //  ""Args"": {
                  //    ""switch"": ""$filterSwitch""
                  //  }
                  //},
                  {
                    ""Name"": ""With"",
                    ""Args"": {
                      ""filter"": ""SerilogExample.CustomFilter, SerilogExample""
                    }
                  }
                ]
              }
            }
            ";

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
                .Build();

            return configuration;
        }
    }
}
