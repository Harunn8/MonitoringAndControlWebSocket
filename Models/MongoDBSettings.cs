using System;

namespace Models
{
    public class MongoDBSettings
    {
        public string DatabaseName { get; set; }
        public Collections Collections { get; set; }
    }

    public class Collections
    {
        public string User { get; set; }
        public string Device { get; set; }
    }
}
