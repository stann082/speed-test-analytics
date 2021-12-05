using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace speed_test
{
    public class AwsDynamoDbClient : IAwsDynamoDbClient
    {

        #region Constants

        private const string PARTION_KEY = "Id";
        private const int RCU = 5;
        private const string SORT_KEY = "TestDate";
        private const int WCU = 5;

        #endregion

        #region Constructors

        public AwsDynamoDbClient()
        {
            Client = new AmazonDynamoDBClient();
        }

        #endregion

        #region Properties

        private AmazonDynamoDBClient Client { get; }

        #endregion

        #region Public Methods

        public void CreateTable(string tableName)
        {
            if (DoesTableExist(tableName))
            {
                return;
            }

            Console.WriteLine($"Table {tableName} does not exist. Creating one...");
            CreateTableRequest request = CreateTableRequest(tableName);
            Client.CreateTableAsync(request).Wait();
        }

        public TableDescription DescribeTable(string tableName)
        {
            return Client.DescribeTableAsync(tableName).Result.Table;
        }

        public void PutItem(string tableName, string machineId, string speedTestOutput)
        {
            PutItemRequest putItemRequest = new PutItemRequest();
            putItemRequest.TableName = tableName;
            putItemRequest.Item = CreateItem(machineId, speedTestOutput);
            Client.PutItemAsync(putItemRequest).Wait();
        }

        #endregion

        #region Helper Methods

        private AttributeDefinition CreateAttributeDefinition(string name, string type)
        {
            return new AttributeDefinition(name, type);
        }

        private List<AttributeDefinition> CreateAttributeDefinitions()
        {
            List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
            attributeDefinitions.Add(CreateAttributeDefinition(PARTION_KEY, "S"));
            attributeDefinitions.Add(CreateAttributeDefinition(SORT_KEY, "S"));
            return attributeDefinitions;
        }

        private Dictionary<string, AttributeValue> CreateItem(string machineId, string speedTestOutput)
        {
            return new Dictionary<string, AttributeValue>()
            {
                { PARTION_KEY, new AttributeValue { S = Guid.NewGuid().ToString() }},
                { SORT_KEY, new AttributeValue { S = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) }},
                { "MachineId", new AttributeValue { S = machineId }},
                { "TestResult", new AttributeValue { S = speedTestOutput }}
            };
        }

        private List<KeySchemaElement> CreateKeySchema()
        {
            List<KeySchemaElement> keySchemaElements = new List<KeySchemaElement>();
            keySchemaElements.Add(CreateKeySchemaElement(PARTION_KEY, "HASH"));
            keySchemaElements.Add(CreateKeySchemaElement(SORT_KEY, "RANGE"));
            return keySchemaElements;
        }

        private KeySchemaElement CreateKeySchemaElement(string name, string type)
        {
            return new KeySchemaElement(name, type);
        }

        private ProvisionedThroughput CreateProvisionedThroughput()
        {
            ProvisionedThroughput throughput = new ProvisionedThroughput();
            throughput.ReadCapacityUnits = RCU;
            throughput.WriteCapacityUnits = WCU;
            return throughput;
        }

        private CreateTableRequest CreateTableRequest(string tableName)
        {
            CreateTableRequest request = new CreateTableRequest();
            request.TableName = tableName;
            request.AttributeDefinitions = CreateAttributeDefinitions();
            request.KeySchema = CreateKeySchema();
            request.ProvisionedThroughput = CreateProvisionedThroughput();
            return request;
        }

        private bool DoesTableExist(string tableName)
        {
            ListTablesResponse response = Client.ListTablesAsync().Result;
            return response.TableNames.Any(tn => tn == tableName);
        }

        #endregion

    }
}
