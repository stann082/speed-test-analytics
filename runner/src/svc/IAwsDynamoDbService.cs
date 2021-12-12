namespace service
{
    public interface IAwsDynamoDbService
    {

        void CreateTable(string tableName);
        void PutItem(string tableName, string machineId, string speedTestOutput);

    }
}
