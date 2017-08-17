namespace Http.Gateway
{
    public class Service
    {
        public string id
        { get
            {
                return $"{Name?.ToLowerInvariant()}:{Version?.ToLowerInvariant()}";
            }
        }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}