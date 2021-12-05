using Amazon.DynamoDBv2.Model;

namespace speed_test
{
    public interface IAwsDynamoDbClient
    {

        void CreateTable(string tableName);
        TableDescription DescribeTable(string tableName);
        void PutItem(string tableName, string machineId, string speedTestOutput);

    }
}
