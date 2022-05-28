using Discord.Addons.Hosting;
using Discord.WebSocket;
using Fafik.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fafik.Services
{
    public abstract class FafikService : DiscordClientService
    {
        public readonly DiscordSocketClient _client;
        public readonly ILogger<DiscordClientService> _logger;
        public readonly IConfiguration _configuration;
        public readonly DataAccessLayer _dataAccessLayer;

        public FafikService(DiscordSocketClient client, ILogger<DiscordClientService> logger, IConfiguration configuration, DataAccessLayer dataAccessLayer)
            : base(client, logger)
        {
            _client = client;
            _logger = logger;
            _configuration = configuration;
            _dataAccessLayer = dataAccessLayer;
        }
    }
}
