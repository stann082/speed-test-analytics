package dyndbsvc

import "fmt"

const (
	partitionKey = "Id"
	sortKey      = "testDate"
	rcu          = 5
	wcu          = 5
)

func CreateTable(tableName string) {
	return
}

func PutItem(tableName string, machineId string, speedTestOutput string) {
	fmt.Printf("\ntable name: %s\nmachine id: %s\noutput: %s\n", tableName, machineId, speedTestOutput)
	return
}
