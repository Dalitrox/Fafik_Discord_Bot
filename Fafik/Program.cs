using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Fafik.Data;
using Fafik.Data.Context;
using Fafik.Modules;
using Fafik.Services;
using Fafik.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Fafik
{
    /// <summary>
    /// Generalny dokument startowy bota, zawiera jego konfigurację, zależności oraz jego hosting
    /// </summary>
    internal class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x => //tworzenie konfiguracji bota
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory()) //przypisanie ścieżki gdzie znajduje się cały projekt
                        .AddJsonFile("appsettings.json", false, true) //miejsce pliu JSON z konfiguracją, pierwsza opcja to czy to ma być opcjonalne, a druga opcja czy ma
                                                                      //załadowywać na nowo po dokonaniu zmian
                        .Build();

                    x.AddConfiguration(configuration);
                })

                .ConfigureLogging(x =>
                {
                    x.AddConsole(); //to oznacza, że konsola będzie użyta do wysyałania logów
                    x.SetMinimumLevel(LogLevel.Debug); //to pokazuje jakie minimalny level musi być logów żeby pokazało się w konsoli
                })

                .ConfigureDiscordHost((context, config) => //konfigurowanie hosta Discord bota
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Debug, //jaki loglevel Discord ma używać
                        AlwaysDownloadUsers = false,
                        MessageCacheSize = 2000, //tutaj ustala się ile wiadomości ma być przechowanych w cashu
                    };

                    config.Token = context.Configuration["token"]; //pobranie tokenu bota z pliku json
                })

                .UseCommandService((context, config) => //konfigurowanie serwisu komend
                {
                    config.CaseSensitiveCommands = false; //to jest "wrażliwość" na sposób zapisu komendy np. jeśli zamiast !info napisze się !Info to przy CaseSensitiveCommands
                                                          //równym false będzie przyjmowało obie formu, przy wartości true tylko taką formę jaka została w pliku zapisana

                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Sync;
                })

                .ConfigureServices((context, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>()
                        .AddSingleton<Images>()
                        .AddHttpClient()
                        .AddDbContextFactory<FafikDatabaseContext>(options => options.UseMySql(
                            context.Configuration.GetConnectionString("Default"),
                            new MySqlServerVersion(new Version(8, 0, 27))))
                        .AddSingleton<DataAccessLayer>();
                })

                .UseConsoleLifetime();

            var host = builder.Build();

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}