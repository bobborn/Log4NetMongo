using log4net.Layout;
using System.Collections;

namespace Log4NetMongo
{
    public class MongoDbAppenderField
    {
        public string Name { get; set; }
        public IRawLayout Layout { get; set; }
    }
}