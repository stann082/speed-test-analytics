using Amazon.DynamoDBv2.Model;

namespace service
{
    public interface IAwsDynamoDbService
    {

        void CreateTable(string tableName);
        TableDescription DescribeTable(string tableName);
        void PutItem(string tableName, string machineId, string speedTestOutput);

    }
}
