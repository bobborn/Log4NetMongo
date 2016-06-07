using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net.Config;
using System.IO;
using System.Text;
using log4net;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Log4NetMongo.Tests
{
    [TestClass]
    public class MongoDbAppenderTest
    {
        private IMongoCollection<BsonDocument> _collection;
        private IMongoDatabase _db;
        private const string CollectionName = "logs";

        [TestInitialize]
        public void Init()
        {
            GlobalContext.Properties.Clear();
            ThreadContext.Properties.Clear();
            MongoUrl url = new MongoUrl("mongodb://localhost/log4net");

            MongoClient client = new MongoClient(url);
            _db = client.GetDatabase(url.DatabaseName);
            _db.DropCollectionAsync(CollectionName);
            _collection = _db.GetCollection<BsonDocument>(CollectionName);
        }


        [TestMethod]
        public void TestMethod1()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
            <log4net>
	            <appender name='MongoDbAppender' type='Log4NetMongo.MongoDbAppender, Log4NetMongo'>
		            <connectionString value='mongodb://localhost' />
		            <field>
			            <name value='timestamp' />
			            <layout type='log4net.Layout.RawTimeStampLayout' />
		            </field>
		            <field>
			            <name value='level' />
			            <layout type='log4net.Layout.PatternLayout' value='%level' />
		            </field>
		            <field>
			            <name value='thread' />
			            <layout type='log4net.Layout.PatternLayout' value='%thread' />
		            </field>
                    <field>
			            <name value='message' />
			            <layout type='log4net.Layout.PatternLayout' value='%message' />
		            </field>
		            <field>
			            <name value='exception' />
			            <layout type='log4net.Layout.ExceptionLayout' />
		            </field>
		            <field>
			            <name value='age' />
			            <layout type='Log4NetMongo.CustomPatternLayout,Log4NetMongo'>
                            <conversionPattern value='%property{age}'/> 
                        </layout>
		            </field>
	            </appender>
	            <root>
		            <level value='ALL' />
		            <appender-ref ref='MongoDbAppender' />
	            </root>
            </log4net>
            ")));

            ILog log = LogManager.GetLogger(Guid.NewGuid().ToString());
            log.Debug(new Test
            {
                Id = 1,
                Age = 26
            });
        }
    }


    public class Test
    {
        public int Id { get; set; }

        public int Age { get; set; }
    }
}
