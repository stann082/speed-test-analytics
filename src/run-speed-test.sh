#!/bin/bash

while test $# -gt 0; do
    case "$1" in
        --list-buckets) aws s3api list-buckets --query "Buckets[].Name" --output table
        exit 0
            ;;
        --*) echo "Invalid argument $1"
        exit 0
            ;;
    esac
    shift
done

echo "Retrieving account id..."
ACCOUNT_ID=$(aws sts get-caller-identity --query "Account" --output text)
REGION="us-west-2"

S3_BUCKET_NAME="speed-test-${ACCOUNT_ID}-${REGION}-$(hostname)"
S3_BUCKET_URL="s3://$S3_BUCKET_NAME"
EXISTING_BUCKETS=$(aws s3api list-buckets --query "Buckets[].Name" --output text)
if [[ $EXISTING_BUCKETS != *"${S3_BUCKET_NAME}"* ]]; then
    echo "Bucket $S3_BUCKET_NAME does not exist... Making one"
    aws s3 mb $S3_BUCKET_URL 2> /tmp/make_bucket_err 1> /dev/null
fi

ERROR=$(cat /tmp/make_bucket_err)
if [ "$ERROR" != "" ]; then
    echo $ERROR
    exit 1
fi

FOLDER=$(date +"%Y-%m-%d")
CURRENT_TIME=$(date "+%H_%M_%S")
TEST_RESULTS_JSON=/tmp/speed_test_$CURRENT_TIME.json
echo "Running speed test..."
speedtest --format=json > $TEST_RESULTS_JSON

aws s3api put-object --bucket ${S3_BUCKET_NAME} --key ${FOLDER}/ 2> /tmp/make_bucket_folder_err 1> /dev/null
ERROR=$(cat /tmp/make_bucket_folder_err)
if [ "$ERROR" != "" ]; then
    echo $ERROR
    exit 1
fi

echo "Uploading test results to ${S3_BUCKET_URL}/${FOLDER}..."
aws s3 cp $TEST_RESULTS_JSON ${S3_BUCKET_URL}/${FOLDER}/ 2> /tmp/copy_to_bucket_err 1> /dev/null

ERROR=$(cat /tmp/copy_to_bucket_err)
if [ "$ERROR" != "" ]; then
    echo $ERROR
    exit 1
else
    rm $TEST_RESULTS_JSON
fi

