using log4net.Core;
using log4net.Util;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetMongo
{
    public interface IBuildBsonDocument
    {
        BsonDocument Build(LoggingEvent loggingEvent);
    }

    public class BuildBsonDocument : IBuildBsonDocument
    {
        private Dictionary<string, MongoDbAppenderField> _fields;

        public BuildBsonDocument(Dictionary<string, MongoDbAppenderField> fields)
        {
            _fields = fields;
        }

        public BsonDocument Build(LoggingEvent loggingEvent)
        {
            var bsonDocument = new BsonDocument();

            foreach (var field in _fields)
            {
                object value = field.Value.Layout.Format(loggingEvent);
                if (value != null)
                {
                    var bsonValue = value as BsonValue ?? BsonValue.Create(value);
                    bsonDocument.Add(field.Key, bsonValue);
                }
            }

            return bsonDocument;
        }
    }


    public class BuildLog : IBuildBsonDocument
    {
        public BsonDocument Build(LoggingEvent loggingEvent)
        {
            var log = Create(loggingEvent);
            return log.ToBsonDocument();
        }

        private Log Create(LoggingEvent loggingEvent)
        {
            var log = new Log
            {
                TimeStamp = loggingEvent.TimeStamp,
                Level = loggingEvent.Level.ToString(),
                ThreadName = loggingEvent.ThreadName,
                UserName = loggingEvent.UserName,
                Message = loggingEvent.RenderedMessage,
                LoggerName = loggingEvent.LoggerName,
                Domain = loggingEvent.Domain,
                MachineName = Environment.MachineName,
                Exception = loggingEvent.ExceptionObject
            };

            if (loggingEvent.LocationInformation != null)
            {
                log.LocationInfo = new LocationInfo(className: loggingEvent.LocationInformation.ClassName,
                    methodName: loggingEvent.LocationInformation.MethodName,
                     fileName: loggingEvent.LocationInformation.FileName,
                     lineNumber: loggingEvent.LocationInformation.LineNumber);
            }

            PropertiesDictionary compositeProperties = loggingEvent.GetProperties();
            if (compositeProperties != null && compositeProperties.Count > 0)
            {
                log.Properties = new Dictionary<string, string>();
                foreach (DictionaryEntry entry in compositeProperties)
                {
                    log.Properties.Add(entry.Key.ToString(), entry.Value == null ? null : entry.Value.ToString());
                }
            }

            return log;
        }
    }


    public class BuildBsonDocumentFactory
    {
        private Dictionary<string, MongoDbAppenderField> _fields;

        public BuildBsonDocumentFactory(Dictionary<string, MongoDbAppenderField> fields)
        {
            _fields = fields;
        }

        public IBuildBsonDocument GetBuildBsonDocument()
        {
            if (_fields.Any())
            {
                return new BuildBsonDocument(_fields);
            }
            return new BuildLog();
        }
    }
}
