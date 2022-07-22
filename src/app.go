package main

//region imports
import (
	"encoding/json"
	"fmt"
	"log"
	"os"
	"os/exec"
	"runtime"
	"speedtest/configure"
	m "speedtest/configure/model"
	"speedtest/dyndbsvc"
)

//endregion

//region constants

const (
	speedTestFileName       = "speedtest.exe"
	speedTestFilePathEnvVar = "SPEED_TEST_FILE_PATH"
	tableName               = "SpeedTestAnalytics"
)

//endregion

//region structs

type Result struct {
	Bandwidth float32 `json:"bandwidth"`
	Bytes     float32 `json:"bytes"`
	Elapsed   int32   `json:"elapsed"`
}

//endregion

//region main function

func main() {
	var cfg m.Config
	configure.Initialize(&cfg)
	dyndbsvc.InitializeAwsClient()

	speedTestFilePath := getSpeedTestFilePath()
	if speedTestFilePath == "" {
		log.Printf("Could not locate %s\n", speedTestFileName)
		return
	}

	dyndbsvc.CreateTable(tableName)

	machineId := getMachineId()
	log.Printf("Running a speed test for %s\n", machineId)

	stdout := runProcess(speedTestFilePath, cfg.Output.Format)
	if stdout == "" {
		log.Println("Speed test failed")
		return
	}

	printTestResult(stdout, "download")
	printTestResult(stdout, "upload")
	dyndbsvc.PutItem(tableName, machineId, stdout)
}

//endregion

//region helper functions

func printTestResult(stdout string, testOperation string) {
	var m map[string]json.RawMessage
	err := json.Unmarshal([]byte(stdout), &m)
	if err != nil {
		log.Fatalf("Failed to parse json output: %v\n", err)
	}

	var result Result
	if eventRaw, ok := m[testOperation]; ok {
		if err := json.Unmarshal(eventRaw, &result); err != nil {
			log.Fatalf("Error parsing JSON output: %v\n", err)
		}
	} else {
		log.Fatalf("Can't find '%s' key in JSON: %v\n", testOperation, err)
	}

	log.Printf("%s size: %s\n", testOperation, formatTestResultData(result.Bytes, false))
	log.Printf("%s speed: %s\n", testOperation, formatTestResultData(result.Bandwidth, true))
	log.Printf("%s time: %s\n", testOperation, fmt.Sprintf("%d secs", result.Elapsed/1000))
}

func formatTestResultData(size float32, useBits bool) string {
	var sizes [4]string
	if useBits {
		sizes = [4]string{"Bit", "KBit", "MBit", "GBit"}
	} else {
		sizes = [4]string{"B", "KB", "MB", "GB"}
	}

	order := 0
	for size >= 1024 && order < len(sizes)-1 {
		order++
		size /= 1024
	}

	if useBits {
		size *= 8
	}

	return fmt.Sprintf("%.2f %s", size, sizes[order])
}

func getMachineId() string {
	hostname, err := os.Hostname()
	if err != nil {
		log.Printf("Error getting a hostname: %v", err)
		return runtime.GOOS
	}
	return fmt.Sprintf("%s (%s)", hostname, runtime.GOOS)
}

func getSpeedTestFilePath() string {
	_, err := os.Stat(speedTestFileName)
	if err == nil {
		return speedTestFileName
	}
	return os.Getenv(speedTestFilePathEnvVar)
}

func runProcess(programPath string, format string) (stdout string) {
	args := fmt.Sprintf("--format=%s", format)
	out, err := exec.Command(programPath, args).Output()
	if err != nil {
		log.Fatal(err)
	}
	return string(out)
}

//endregion
