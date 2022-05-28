using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Fafik.Common;
using Fafik.Data;
using Fafik.Models;
using Fafik.Services;
using Fafik.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


/// <summary>
/// Dokument z bazowymi komendami jakie są normalnie dostępne
/// </summary>

namespace Fafik.Modules
{
    public class GeneralCommands : FafikModuleBase
    {
        private readonly Images _images;
        private readonly IHttpClientFactory _httpClientFactory;

        public GeneralCommands(Images images, IHttpClientFactory httpClientFactory, DataAccessLayer dataAccessLayer)
            :base(dataAccessLayer)
        {
            _images = images;
            _httpClientFactory = httpClientFactory;
        }

        [Command("ping")] //utowrzenie przykładowej komendy
        public async Task Ping()
        {                                   //context to generalnie odwołanie się do tego na jakim kanale dana komenda została wywołana, przez jakiego użytkownika, jakie serwis
                                            //komend musi zostać wykonany
            await Context.Channel.TriggerTypingAsync(); //to daje informację użytkowinkowi, że bot odpowiada na jego komendę pokazując pod paskiem wpisywania wiadomości oznaczenie,
                                                        //że bot odpowiada
            await Context.Channel.SendMessageAsync("Pong!", true); //co system (bot) ma odpowiedzieć jeśli taka komenda zostanie wykonana
        }

        [Command("info")]
        public async Task Info(SocketGuildUser user = null)
        {
            await Context.Channel.TriggerTypingAsync();
            if (user == null)
            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()) //jeśli użytkownik ma własny avatar to go wyświetli a jeśli nie to pokaże
                                                                                                     //domyślny
                .WithDescription("Kilka informacji jakie mamy o tobie!")
                .WithColor(new Color(15, 240, 247))
                .AddField("User ID", Context.User.Id, true)
                .AddField("Discriminator", Context.User.Discriminator, true)
                .AddField("Username", Context.User.Username, true)
                .AddField("Created at", Context.User.CreatedAt.ToString("dd/MM/yyyy, HH:mm:ss"), true) //zostaje przekonwertowane na string żeby napis został wyświetlony czytelny
                                                                                                       //napis
                .AddField("Joined at", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy, HH:mm:ss"), true)
                .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp();

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);

            }
            else
            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(user.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()) //jeśli użytkownik ma własny avatar to go wyświetli a jeśli nie to pokaże domyślny
                .WithDescription($"Kilka informacji jakie mamy o użytkowniku {user.Username}!")
                .WithColor(new Color(15, 240, 247))
                .AddField("User ID", user.Id, true)
                .AddField("Discriminator", user.Discriminator, true)
                .AddField("Username", user.Username, true)
                .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy, HH:mm:ss"), true) //zostaje przekonwertowane na string żeby napis został wyświetlony czytelny
                                                                                                       //napis
                .AddField("Joined at", user.JoinedAt.Value.ToString("dd/MM/yyyy, HH:mm:ss"), true)
                .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp();

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);

            }
        }

        [Command("server")]
        public async Task Server()
        {
            await Context.Channel.TriggerTypingAsync();
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Kilka informacji o naszym serwerze!")
                .WithTitle($"Informacje o serwerze {Context.Guild.Name}")
                .WithColor(new Color(21, 40, 163))
                .AddField("Created at", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Memebercount", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true)
                .AddField("Owner Name", Context.Guild.Owner.Username, true)
                .AddField("Owner Discriminator", Context.Guild.Owner.Discriminator, true)
                .AddField("Owner ID", Context.Guild.Owner.Id, true);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);

        }

        [Command("remove")]
        [Alias("delete", "clear")] //aliasy pozwalają na użycie tej samej komendy przy pomocy innych słów
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Remove(int amount)
        {
            await Context.Channel.TriggerTypingAsync();
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} wiadmości zostało pomyślnie usuniętych!");

            await Task.Delay(2500);
            await message.DeleteAsync();
            await SendLog(Context.Guild, "Usuwanie wiadomości", $"Użytkownik {Context.User.Username} dokonał usunięcia {messages.Count()} wiadomości");

        }

        [Command("image", RunMode = RunMode.Async)] //jeśli ta komenda będzie wykonywana to będzie ona wykonywana na zewnątrz żeby nie blokowało to możliwości korzystania
                                                    //z innych komend
        public async Task ImageAsync(SocketGuildUser user = null) //podanie/wzmianka nazwy użytkownika jest opcjonalne
        {
            if (user == null) //jeśli jest on nullem to wtedy wyświetli banner użytkownika, który napisał komendę
            {
                var path = await _images.CreateImageAsync(Context.User as SocketGuildUser); //utorzenie zdjęcia (banneru) dla użytkownika który użył komendy
                await Context.Channel.SendFileAsync(path); //wysłanie pliku

                File.Delete(path); //usunięcie pliku
            }
            else //jeśli jest podana nazwa użytkownika to wtedy wykona banner dla użytkonika o podanym/wspomnianej nazwie
            {
                var path = await _images.CreateImageAsync(user); //utorzenie zdjęcia (banneru) dla użytkownika który użył komendy
                await Context.Channel.SendFileAsync(path); //wysłanie pliku

                File.Delete(path); //usunięcie pliku
            }
        }

        [Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subreddit = null)
        {
            await Context.Channel.TriggerTypingAsync();
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("Taki subreddit nie istnieje!");
                return;
            }

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(post["url"].ToString())
                .WithColor(new Color(255, 111, 0))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}");

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);

        }

        [Command("activity")]
        public async Task Activity()
        {
            await Context.Channel.TriggerTypingAsync();
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetStringAsync("https://www.boredapi.com/api/activity/");

            var activity = Events.FromJson(response);

            if (activity == null)
            {
                await Context.Channel.SendError("Problem", "Wystąpił błąd. Proszę spróbować później.");
                return;
            }

            await ReplyAsync($"**Aktywność:** {activity.Activity}\n**Uczestnicy:** {activity.Participants}\n**Rodzaj:** {activity.Type}\n**Cena:** {activity.Price}");
        }

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var currentPrefix = _dataAccessLayer.GetPrefix(Context.Guild.Id);
                await ReplyAsync($"Aktualny prefiks tego serwera to: '{currentPrefix}'.");
                return;
            }

            if (prefix.Length > 1)
            {
                await Context.Channel.SendError("Błąd", "Nowy prefix jest zbyt długi!");
                return;
            }

            await _dataAccessLayer.UpdatePrefix(Context.Guild.Id, prefix);

            await Context.Channel.SendSucces("Sukces", $"Prefiks został zmieniony na '{prefix}'.");
            await SendLog(Context.Guild, "Zmiana prefixu", $"Użytkownik {Context.User.Username} dokonał zmiany prefixu na '{prefix}'");
        }

        [Command("echo")]
        public async Task Echo([Remainder] string text)
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(text, true);
        }

        [Command("log")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Log(string value = null) //zadanie, które sprawdza czy kanał do logów jest utworzony
        {
            if (value == null)
            {
                var fetchChannelID = await _dataAccessLayer.GetLog(Context.Guild.Id);

                if (fetchChannelID == 0) //jeśli nie ma znajdzie przypisanego kanału
                {
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync("Nie został ustawiony żaden kanał do logów!"); //wyświetli ten komunikat
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchChannelID); //pobierze ID odpowiedniego kanału

                if (fetchedChannel == null) //jeśli kanał został usunięty lub bot go znaleźć nie może
                {
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync("Nie został ustawiony żaden kanał do logów!"); //wyświetli komunikat, że kanał do logów nie został ustawiony
                    await _dataAccessLayer.ClearLog(Context.Guild.Id); //i wyczyści dane (zmieni na wartość Logs = 0) i automatycznie wyłączy moduł powitalny
                    return;
                }

                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync($"Kanał użyty jako kanał do logów to: {fetchedChannel.Mention}.");

                return;
            }

            if (value != "clear") //jeśli value nie jest "clear" to modyfikuje kanał jako nowy kanał do logów
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedID)) //spróbuje przekonwertować value na ulong uzyskując ID kanału, po przez wzmiankę odpowiedniego kanału
                {
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync("Proszę podać poprawny kanał!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedID); //zmienna, która przechowuje podany ID

                if (parsedChannel == null) //jeśli kanał o takim ID nie istnieje to wyświetli stosowny komunikat
                {
                    await Context.Channel.TriggerTypingAsync();
                    await ReplyAsync("Proszę podać poprawny kanał!");
                    return;
                }

                await _dataAccessLayer.ModifyLog(Context.Guild.Id, parsedID); //modyfikuje ID kanału na ten jaki został podany w parsedID

                await Context.Channel.TriggerTypingAsync();
                await Context.Channel.SendSucces("Sukces!", $"Pomyślnie zmieniono kanał do logów na {parsedChannel.Mention}!");
                await SendLog(Context.Guild, "Zmiana logu", $"Użytkownik: {Context.User.Username} dokonał zmiany kanału do logów na {parsedChannel.Mention}");
                return;
            }

            if (value == "clear") //jeśli value jest "clear" to wyświetli komunikat o pomyślnym usunięciu kanału
            {
                await SendLog(Context.Guild, "Usunięcie logu", $"Użytkownik: {Context.User.Username} usunął kanał do logów");
                await Context.Channel.TriggerTypingAsync();
                await _dataAccessLayer.ClearLog(Context.Guild.Id);
                await Context.Channel.SendSucces("Sukces!", "Pomyślnie usunięto kanał do logow!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync("Nie użyto polecenia poprawnie.");
            await Context.Channel.SendError("Błąd!", "Nie użyto polecenia poprawnie.");
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Mute(SocketGuildUser user, double minutes, [Remainder] string reason = null)
        {
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy) //najpierw sprawdza czy pozycja danego użytkownika jest większa lub mniejsza od pozycji bota w hierarchi ról
            {
                await Context.Channel.SendError("Błąd!", "Pozycja tego użytkownika jest większa od bota!");
                return;
            }

            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Wyciszony");

            if (role == null) //jeśli taka rola nie istnieje to wtedy bot utworzy taką rolę i uniemożliwi posanie wiadomości przez takiego użytkownika
            {
                role = await Context.Guild.CreateRoleAsync("Wyciszony", new GuildPermissions(sendMessages: false), null, false, null);
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy) //sprawdza czy pozycja "wyciszonej" roli jest wyższa od pozycji bota
            {
                await Context.Channel.SendError("Błąd uprawnień!", "Pozycja roli 'wyciszony' ma wyższą pozycję od bota!");
                return;
            }

            if (user.Roles.Contains(role)) //jeśli u użytkownika jest już rola "wyciszony" dodana lub utworzona to wyświetli komunikat, że użytkownika został już zmutowany
            {
                await Context.Channel.SendError("Błąd!", "Użytkownik jest już zmutowany!");
                return;
            }

            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy); //modyfikuje "przenosi" role wyciszonych użytkowników na poziom pozycję tuż pod botem
            foreach (var channel in Context.Guild.TextChannels) //pętla, w której będzie blokowało pisanie wyciszonemu użytkownikowi na innych kanałach tekstowych
            {
                if (!channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
                }
            }

            CommandHandler.Mutes.Add(new Mute { Guild = Context.Guild, User = user, End = DateTime.Now + TimeSpan.FromMinutes(minutes), Role = role });

            await user.AddRoleAsync(role);

            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSucces("Wyciszono", $"Pomyślnie wyciszono użytkonika {user.Username} na {minutes} minut. \n Powód: {reason ?? "Żaden"}.");
            await SendLog(Context.Guild, "Wyciszenie", $"Pomyślnie wyciszono użytkonika {user.Username} na {minutes} minut. \n Powód: {reason ?? "Żaden"}.");
        }

        [Command("unmute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Unmute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Wyciszony");

            if (role == null)
            {
                await Context.Channel.SendError("Błąd!", "Ten użytkonik nie został jeszcze wyciszony!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy) //sprawdza czy pozycja "wyciszonej" roli jest wyższa od pozycji bota
            {
                await Context.Channel.SendError("Błąd uprawnień!", "Pozycja roli 'wyciszony' ma wyższą pozycję od bota!");
                return;
            }

            if (!user.Roles.Contains(role)) //jeśli u użytkownika nie ma roli "wyciszony" dodana lub utworzona to wyświetli komunikat, że użytkownika został już zmutowany
            {
                await Context.Channel.SendError("Błąd!", "Użytkownik nie jest wyciszony!");
                return;
            }

            await user.RemoveRoleAsync(role);

            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendSucces("Zdjęto wyciszenie", $"Pomyślnie zdjęto wyciszenie z użytkownika {user.Username}.");
            await SendLog(Context.Guild, "Zdjęcie wyciszenia", $"Pomyślnie zdjęto wyciszenie z użytkownika {user.Username}.");
        }
    }
}
