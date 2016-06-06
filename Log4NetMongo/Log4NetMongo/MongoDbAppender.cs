using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using log4net.Util;
using MongoDB.Driver;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Bson;
using System.Collections;

namespace Log4NetMongo
{
    public class MongoDbAppender : AppenderSkeleton
    {
        private const string _defaultCollectionName = "logs";
        private const string _defaultDatabaseName = "log4net";
        private const string _defaultConnectionString = "mongodb://localhost/" + _defaultDatabaseName;


        private string _connectionString;
        private string _collectionName;


        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    return _defaultConnectionString;
                }

                if (!_connectionString.StartsWith("mongodb://"))
                {
                    var connectionStringSetting = ConfigurationManager.ConnectionStrings[_connectionString];
                    if (connectionStringSetting != null)
                    {
                        return connectionStringSetting.ToString();
                    }
                    return _defaultConnectionString;
                }

                return _connectionString;

            }
            set { _connectionString = value; }
        }

        public string CollectionName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_collectionName))
                {
                    return _defaultCollectionName;
                }
                return _collectionName;
            }
            set
            {
                _collectionName = value;
            }
        }

        public string CertificateFriendlyName { get; set; }


        private readonly Dictionary<string, MongoDbAppenderField> _fields = new Dictionary<string, MongoDbAppenderField>();

        public void AddField(MongoDbAppenderField field)
        {
            _fields.Add(field.Name, field);
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var factory = new BuildBsonDocumentFactory(_fields);
            IBuildBsonDocument buildBsonDocument = factory.GetBuildBsonDocument();
            var document = buildBsonDocument.Build(loggingEvent);

            var collection = GetCollection();
            collection.InsertOne(document);
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            var factory = new BuildBsonDocumentFactory(_fields);
            IBuildBsonDocument buildBsonDocument = factory.GetBuildBsonDocument();

            var collection = GetCollection();
            collection.InsertManyAsync(loggingEvents.Select(t => buildBsonDocument.Build(t)));
        }

        protected virtual IMongoDatabase GetDatabase()
        {
            MongoUrl url = MongoUrl.Create(ConnectionString);
            MongoClientSettings settings = MongoClientSettings.FromUrl(url);

            settings.SslSettings = url.UseSsl ? GetSslSettings() : null;
            MongoClient client = new MongoClient(settings);

            IMongoDatabase db = client.GetDatabase(url.DatabaseName ?? _defaultDatabaseName);
            return db;
        }

        protected virtual IMongoCollection<BsonDocument> GetCollection()
        {
            return GetDatabase().GetCollection<BsonDocument>(CollectionName); ;
        }

        private SslSettings GetSslSettings()
        {
            SslSettings sslSettings = null;

            if (!string.IsNullOrEmpty(CertificateFriendlyName))
            {
                X509Certificate2 certificate = GetCertificate(CertificateFriendlyName);

                if (null != certificate)
                {
                    sslSettings = new SslSettings();
                    sslSettings.ClientCertificates = new List<X509Certificate2>() { certificate };
                }
            }
            return sslSettings;
        }

        private X509Certificate2 GetCertificate(string certificateFriendlyName)
        {
            X509Certificate2 certificateToReturn = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates;

            foreach (X509Certificate2 certificate in certificates)
            {
                if (certificate.FriendlyName.Equals(certificateFriendlyName))
                {
                    certificateToReturn = certificate;
                    break;
                }
            }

            store.Close();

            return certificateToReturn;
        }

    }
}
