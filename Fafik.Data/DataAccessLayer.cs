using Fafik.Data.Context;
using Fafik.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafik.Data
{
    public class DataAccessLayer
    {
        private readonly IDbContextFactory<FafikDatabaseContext> _contextFactory;

        public DataAccessLayer(IDbContextFactory<FafikDatabaseContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateGuild(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();

            if (context.Guilds.Any(x => x.Id == id)) //sprawdza czy taka gildia (serwer) o takim ID istnieje, jeśli nie to nie robi nic
            {
                return;
            }

            context.Add(new Guild { Id = id });

            await context.SaveChangesAsync();
        }

        public string GetPrefix(ulong id) //id jest brane od ID gildii
        {
            using var context = _contextFactory.CreateDbContext();

            var guild = context.Guilds.Find(id); //szuka gildii na bazie jego ID w bazie danych

            if (guild == null) //jeśli taka gildia nie istnieje
            {
                guild = context.Add(new Guild { Id = id }).Entity; //to wtedy tworzy nową w bazie danych z odpowiednim ID oraz prefiksem, który został w Guild.cs ustawiony domyślnie na "!"
                context.SaveChanges();
            }

            return guild.Prefix;
        }

        public async Task UpdatePrefix(ulong id, string prefix) //id gildii (serwera), string nowego prefiksu
        {
            using var context = _contextFactory.CreateDbContext();

            var guild = await context.Guilds.FindAsync(id); //szuka gildii/serwera na bazie jego ID w bazie danych (po przez klucz główny)

            if (guild != null) //jeśli taka gildia istnieje
            {
                guild.Prefix = prefix; //to wtedy zmień aktualny prefiks na nowy
            }
            else //jeśli nie istnieje
            {
                context.Add(new Guild { Id = id, Prefix = prefix }); //wtedy dodaj ją do bazy danych wraz z ID i przypianym prefiksem
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteGuild(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var guild = await context.Guilds.FindAsync(id);

            if (guild == null) //sprawdza czy taka gildia (serwer) o takim ID nie istnieje, jeśli tak jest to nie robi nic
            {
                return;
            }

            context.Remove(guild); //jeśli gildia o danym ID istnieje to wtedy usuń ją z bazy danych

            await context.SaveChangesAsync(); //zapisywanie zmian
        }

        public async Task ModifyLog(ulong id, ulong channelId) //zadanie umiżliwiające modyfikowanie wartości logów serwera
        {
            using var context = _contextFactory.CreateDbContext(); //szuka i wybiera pierwszy serwer który ma odpowiednie ID z bazy danych
            var guild = await context.Guilds.FindAsync(id);

            if (guild == null) //jeśli taki serwer  go nie istnieje to wtedy tworzy nowy w bazie danych
            {
                context.Add(new Guild { Id = id, Logs = channelId });
            }
            else //natomiast jeśli istnieje to wtedy zmienia dane domyślnego ID kanału na ten który zawiera channelID
            {
                guild.Logs = channelId;
            }

            await context.SaveChangesAsync(); //zapis zmian
        }

        public async Task ClearLog(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var guild = await context.Guilds.FindAsync(id); //szuka i wybiera pierwszy serwer który ma odpowiednie ID z bazy danych

            guild.Logs = 0; //zmienia wartość Logs padany z channelID na 0 -> czyli czyści do wartości domyślnej

            await context.SaveChangesAsync();
        }

        public async Task<ulong> GetLog(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var guild = await context.Guilds.FindAsync(id); //szuka i wybiera pierwszy serwer który ma odpowiednie ID z bazy danych

            return await Task.FromResult(guild.Logs); //zwraca ulong (ID) kanału z logami
        }
    }
}
