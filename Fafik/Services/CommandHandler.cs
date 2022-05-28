using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Fafik.Common;
using Fafik.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fafik.Services
{
    public class CommandHandler : FafikService
    {
        private readonly IServiceProvider _serviceProvider; //zależności jakie CommandHandler będzie mógł zczytywać żeby móc ich użyć w tej klasie
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _configuration;

        public static List<Mute> Mutes = new List<Mute>(); //utworzenie listy gdzie będą wyciszeni użytkownicy

        public CommandHandler(IServiceProvider serviceProvider, DiscordSocketClient client, CommandService service, IConfiguration configuration, ILogger<DiscordClientService> logger, DataAccessLayer dataAccessLayer) //rzeczywiste umożliwienie korzystania z innych zasobów
            : base(client, logger, configuration, dataAccessLayer)
        {
            _serviceProvider = serviceProvider;
            _client = client;
            _service = service;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += MessageRecieved;
            _service.CommandExecuted += CommandExecuted;

            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider); //dodaje wszytskie moduły do command service przez co mogą one zostać użyte
        }

        private async Task CommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            await (commandContext.Channel as ISocketMessageChannel).SendError("Błąd!", result.ErrorReason);
        }

        private async Task MessageRecieved(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage message)) //sprawdza czy wiadomość jjest możliwa do rzutowania na wiadomość wysłaną przez użytkownika, jeśli nie to nie robi nic
            {
                return;
            }

            if (message.Source != MessageSource.User) //sprawdza czy wiadomość została faktycznie wysłana przez użytkownika, a nie np. przez innego bota
            {
                return; //wtedy nie robi nic
            }

            var argPos = 0; //pozycja pierwszego znaku potrzebna do argumentów położenia prefiksa lub znaku wzmianki
            var user = message.Author as SocketGuildUser;
            var prefix = _dataAccessLayer.GetPrefix(user.Guild.Id);

            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) //sprawdza czy wiadomość posiada
                                                                                                                                                      //znak prefiksu (czyli znaku
                                                                                                                                                      //odpowiedzialnego za wykonywanie
                                                                                                                                                      //komend) lub znaku wzmianki @
            {
                return; //wtedy nie robi nic
            }

            var context = new SocketCommandContext(_client, message); //wariacja, która zapisuje użytkownika (klienta), który wysłał wiadomość oraz jego wiadomość

            await _service.ExecuteAsync(context, argPos, _serviceProvider); //dzięki temu można wykonywać polecenia egzekwować, pierwszy jest kontekst, druga pozycja prefiksu
                                                                                    //lub wzmianki oraz serviceProvider, który prowadzi IServiceProvider
        }

        private async Task MuteHandler()
        {
            List<Mute> Remove = new List<Mute>(); //ze względu że nie można modyfikować listy w pętli to jest utworzona nowa lista gdzie będą przenoszeni użytkonicy, u których
                                                  //np. czas wyciszenia się skończył

            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                {
                    continue;
                }

                var guild = _client.GetGuild(mute.Guild.Id); //będzie za każdym razem sprawdzało czy rola "wyciszony" nadal istnieje

                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id); //będzie przechowywało ID roli "wyciszony"

                if (guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id); //będzie sprawdzać czy użytkownik nadal istnieje

                if (role.Position > guild.CurrentUser.Hierarchy) //currentUser to odniesienie do bota
                {
                    Remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);

                Remove.Add(mute);
            }

            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(60000);

            await MuteHandler();
        }
    }
}
