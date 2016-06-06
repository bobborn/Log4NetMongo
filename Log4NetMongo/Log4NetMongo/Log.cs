using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetMongo
{
    public class Log
    {
        public Log()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public string ThreadName { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string LoggerName { get; set; }
        public string Domain { get; set; }
        public string MachineName { get; set; }
        public LocationInfo LocationInfo { get; set; }
        public Exception Exception { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
