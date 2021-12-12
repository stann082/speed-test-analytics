# Speed Test Analytics

This tool is to provide stats for your internet speed.

## Installation
1. Install [Speed Test CLI](https://www.speedtest.net/apps/cli)
2. Make sure you do Steps 1 and 2 in this [guide](https://aws.amazon.com/getting-started/hands-on/backup-to-s3-cli/)

## Usage
Simply copy the shell script to one of the cron directories (depending on how often you want it to run)
```bash
sudo cp ./src/run-speed-test.sh /etc/cron.hourly/
```
In the above example the script will execute every hour.

