package model

type Config struct {
	Output struct {
		Format string `yaml:"format"`
	}
	Log struct {
		Level string `yaml:"level"`
	}
	NewRelic struct {
		Name    string `yaml:"name"`
		License string `envconfig:"NEW_RELIC_API_KEY"`
	}
}
