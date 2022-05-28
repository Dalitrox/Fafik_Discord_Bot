using Discord;
using Discord.Commands;
using Discord.Rest;
using Fafik.Common;
using Fafik.Data;
using System.Threading.Tasks;

namespace Fafik.Modules
{
    public abstract class FafikModuleBase : ModuleBase<SocketCommandContext> //klasa jest abstrakcyjna, bo ona nie ma działać samoistnie tylko ma stanowić bazę dla innych komend
    {
        public readonly DataAccessLayer _dataAccessLayer;

        public FafikModuleBase(DataAccessLayer dataAccessLayer)
        {
            _dataAccessLayer = dataAccessLayer;
        }

        /// <summary>
        /// Wsysła embed z tytułem i opisem do odpowiedniego kanału.
        /// </summary>
        /// <param name="title"> Tytuł embedu </param>
        /// <param name="description"> Dodatkowi opis </param>
        /// <returns> <see cref="RestUserMessage"/> zawierający ten embed </returns>
        public async Task<RestUserMessage> SendEmbed(string title, string description)
        {
            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description);

            return await Context.Channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task SendLog(IGuild guild, string title, string description)
        {
            var channelID = await _dataAccessLayer.GetLog(guild.Id); //najpierw musi sprawdzić czy jest kanał odpowiedzialny za logi i przypisuje do channelID

            if (channelID == 0)
            {
                return;
            }

            var fetchedChannel = await guild.GetTextChannelAsync(channelID); //do tej zmiennej przypisujemy ID kanału

            if (fetchedChannel == null) //jeśli jest on nullem
            {
                await _dataAccessLayer.ClearLog(guild.Id); //to wtedy usunie kanał do logów
                return;
            }

            await fetchedChannel.SendLog(title, description);
        }
    }
}
