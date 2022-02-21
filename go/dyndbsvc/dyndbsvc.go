package dyndbsvc

import (
	"fmt"
	"log"
	"time"

	"github.com/aws/aws-sdk-go/aws/awserr"
	"github.com/aws/aws-sdk-go/aws/session"
	ddb "github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
	"github.com/google/uuid"
)

const (
	partitionKey = "Id"
	sortKey      = "TestDate"
	rcu          = 5
	wcu          = 5
)

type Item struct {
	Id         string
	TestDate   string
	MachineId  string
	TestResult string
}

var svc *ddb.DynamoDB

func CreateTable(tableName string) {
	if tableExists(tableName) {
		return
	}

	log.Printf("Table %s does not exist. Creating one...\n", tableName)
	// TODO: create a table
}

func InitializeAwsClient() {
	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	// Create DynamoDB client
	svc = ddb.New(sess)
	if svc == nil {
		log.Fatalf("Could not open aws session\n")
	}
}

func PutItem(tableName string, machineId string, speedTestOutput string) {
	item := Item{
		Id:         fmt.Sprintf("%s", uuid.New()),
		TestDate:   time.Now().UTC().Format(time.RFC3339),
		MachineId:  machineId,
		TestResult: speedTestOutput,
	}

	av, err := dynamodbattribute.MarshalMap(item)
	if err != nil {
		log.Fatalf("Error marshalling new speed test item: %v", err)
	}

	input := &ddb.PutItemInput{
		Item:      av,
		TableName: &tableName,
	}

	_, err = svc.PutItem(input)
	if err != nil {
		log.Fatalf("Error calling PutItem: %v", err)
	}

	log.Printf("Successfully added item to table %s\n", tableName)
}

//region helper functions

func tableExists(tableName string) bool {
	input := &ddb.ListTablesInput{}
	for {
		result, err := svc.ListTables(input)
		if err != nil {
			if aerr, ok := err.(awserr.Error); ok {
				switch aerr.Code() {
				case ddb.ErrCodeInternalServerError:
					log.Println(ddb.ErrCodeInternalServerError, aerr.Error())
				default:
					log.Println(aerr.Error())
				}
			} else {
				// Print the error, cast err to awserr.Error to get the Code and
				// Message from an error.
				log.Println(err.Error())
			}
			return false
		}

		for _, n := range result.TableNames {
			if *n == tableName {
				return true
			}
		}

		input.ExclusiveStartTableName = result.LastEvaluatedTableName
		if result.LastEvaluatedTableName == nil {
			break
		}
	}
	return false
}

//endregion
