package configure

import (
	"github.com/kelseyhightower/envconfig"
	"github.com/newrelic/go-agent/v3/integrations/nrlogrus"
	"github.com/newrelic/go-agent/v3/newrelic"
	"github.com/sirupsen/logrus"
	"gopkg.in/yaml.v2"
	"log"
	"os"
	m "speedtest/configure/model"
)

//region Public Functions

func Initialize(cfg *m.Config) {
	readFile(cfg)
	readEnv(cfg)
	configureNewRelic(cfg.NewRelic.License, cfg.NewRelic.Name, cfg.Log.Level)
}

//endregion

//region Private Functions

func configureNewRelic(apiKey string, appName string, logLevel string) {
	_, err := newrelic.NewApplication(
		newrelic.ConfigAppName(appName),
		newrelic.ConfigLicense(apiKey),
		func(config *newrelic.Config) {
			logrus.SetLevel(getLevel(logLevel))
			config.Logger = nrlogrus.StandardLogger()
		},
	)
	if err != nil {
		log.Printf("failed to configure New Relic: %v\n", err)
	}
}

func getLevel(logLevel string) logrus.Level {
	switch logLevel {
	case "debug":
		return logrus.DebugLevel
	case "error":
		return logrus.ErrorLevel
	case "info":
		return logrus.InfoLevel
	case "warn":
		return logrus.WarnLevel
	default:
		return logrus.InfoLevel
	}
}

func readEnv(cfg *m.Config) {
	err := envconfig.Process("", cfg)
	if err != nil {
		log.Fatal(err)
	}
}

func readFile(cfg *m.Config) {
	f, err := os.Open("configure/config.yml")
	if err != nil {
		log.Fatal(err)
	}
	defer f.Close()

	decoder := yaml.NewDecoder(f)
	err = decoder.Decode(&cfg)
	if err != nil {
		log.Fatal(err)
	}
}

//endregion
