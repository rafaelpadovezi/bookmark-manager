namespace BookmarkManager.Infrastructure.RabbitMQ
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string VHost { get; set; }
        public bool AutomaticRecoveryEnabled { get; set; }
    }
}
