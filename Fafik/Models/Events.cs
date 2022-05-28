using Newtonsoft.Json;
using System;

/// <summary>
/// Dokument gdzie są pobierane i/lub zmieniane dane dotyczące różnych aktywności
/// </summary>

namespace Fafik.Models
{
    public partial class Events
    {
        /// <summary>
        /// Pobiera lub nadpisuje opis badanej czynności
        /// </summary>
        [JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
        public string Activity { get; set; }

        /// <summary>
        /// Czynnik opisujący, w jaki sposób zdarzenie ma związek z najbardziej dostępnym zerem
        /// </summary>
        [JsonProperty("accessibility", NullValueHandling = NullValueHandling.Ignore)]
        public double? Accessibility { get; set; }


        /// <summary>
        /// Pobiera lub zmienia rodzaj działalności
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }


        /// <summary>
        /// Pobiera lub zmienia liczbę osób, w które może zaangażować się to działanie
        /// </summary>
        [JsonProperty("participants", NullValueHandling = NullValueHandling.Ignore)]
        public long? Participants { get; set; }


        /// <summary>
        /// Pobiera lub zmienia czynnik opisujący koszt zdarzenia, gdzie zero jest wolne
        /// </summary>
        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public double? Price { get; set; }

        /// <summary>
        /// Pobiera lub zmienia odnośnik (link) danej aktywności
        /// </summary>
        [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Link { get; set; }

        /// <summary>
        /// Pobiera lub zmienia unikalny identyfikator numeryczny
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Key { get; set; }
    }

    public partial class Events
    {

        public static Events FromJson(string json) => JsonConvert.DeserializeObject<Events>(json, Converter.Settings); //tutaj zachodzi przekonwertowanie pliku JSON
    }
}
