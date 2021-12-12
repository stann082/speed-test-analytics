using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Serilog;
using System.Globalization;

namespace service
{
    public class AwsDynamoDbService : IAwsDynamoDbService
    {

        #region Constants

        private const string PARTION_KEY = "Id";
        private const int RCU = 5;
        private const string SORT_KEY = "TestDate";
        private const int WCU = 5;

        #endregion

        #region Constructors

        public AwsDynamoDbService()
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

            Log.Information($"Table {tableName} does not exist. Creating one...");
            CreateTableRequest request = CreateTableRequest(tableName);
            Client.CreateTableAsync(request).Wait();
            WaitForTableToActivate(tableName);
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

        private TableDescription DescribeTable(string tableName)
        {
            return Client.DescribeTableAsync(tableName).Result.Table;
        }

        private bool DoesTableExist(string tableName)
        {
            ListTablesResponse response = Client.ListTablesAsync().Result;
            return response.TableNames.Any(tn => tn == tableName);
        }

        private void WaitForTableToActivate(string tableName)
        {
            while (DescribeTable(tableName).TableStatus != TableStatus.ACTIVE)
            {
                Log.Information($"Waiting for the table {tableName} to activate...");
                Thread.Sleep(5000);
            }
        }

        #endregion

    }
}
