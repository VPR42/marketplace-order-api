namespace MarketPlace.Services
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = null!;
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "marketplace.orders";
    }
}
