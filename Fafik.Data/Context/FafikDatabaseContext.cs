using Fafik.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafik.Data.Context
{
    public class FafikDatabaseContext : DbContext
    {
        public FafikDatabaseContext(DbContextOptions options)
            : base(options) //dzięki temu nie będzie trzeba definiować konstruktora z bazą danych tylko robi to za nas Program.cs (program uruchamiający bota)
        {
        }

        public DbSet<Guild> Guilds { get; set; }
    }
}
