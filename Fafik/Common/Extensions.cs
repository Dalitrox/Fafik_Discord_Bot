using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafik.Common
{
    public static class Extensions
    {
        public static async Task<IMessage> SendSucces(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(37, 184, 83))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://icons-for-free.com/iconfiles/png/512/complete+done+green+success+valid+icon-1320183462969251652.png")
                    .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);

            return message;
        }

        public static async Task<IMessage> SendError(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(173, 10, 10))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://icons.iconarchive.com/icons/paomedia/small-n-flat/1024/sign-error-icon.png")
                    .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);

            return message;
        }

        public static async Task<IMessage> SendLog(this ITextChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(212, 176, 15))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://i.imgur.com/gLR4k7d.png")
                    .WithName(title);
                })
                .Build();



            var message = await channel.SendMessageAsync(embed: embed);

            return message;
        }
    }
}
