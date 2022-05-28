using Discord;
using Discord.WebSocket;
using System;

namespace Fafik.Common
{
    public class Mute
    {
        public SocketGuild Guild; //serwer, na którym dana osoba została zmutowana
        public SocketGuildUser User; //użytkownik, który został zmutowany
        public IRole Role; //rola, która została użyta do zmutowania użytkownika
        public DateTime End;
    }
}
