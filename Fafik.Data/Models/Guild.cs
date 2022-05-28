using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafik.Data.Models
{
    public class Guild
    {
        public ulong Id { get; set; } //Pobieranie i/lub zmiana ID serwera
        public string Prefix { get; set; } = "!"; //Pobieranie i/lub zmiana aktualnego prefiksu serwera
        public ulong Logs { get; set; } //pobieranie i/lub modyfikacja logów serwera
    }
}
