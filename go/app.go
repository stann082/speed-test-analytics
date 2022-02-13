package main

//region imports
import (
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

//region main function

func main() {
	var cfg m.Config
	configure.Initialize(&cfg)

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
		log.Println("speed test failed")
		return
	}

	dyndbsvc.PutItem(tableName, machineId, stdout)
}

//endregion

//region helper functions

func getMachineId() string {
	hostname, err := os.Hostname()
	if err != nil {
		log.Printf("error getting a hostname: %v", err)
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
