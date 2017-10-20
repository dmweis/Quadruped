#! /bin/bash

FILE_NAME=arm_web_deploy.tar.gz
LATEST_RELEASE=$(curl -L -s -H 'Accept: application/json' https://github.com/dmweis/DynamixelServo/releases/latest)
# The releases are returned in the format {"id":3622206,"tag_name":"hello-1.0.0.11",...}, we have to extract the tag_name.
LATEST_VERSION=$(echo $LATEST_RELEASE | sed -e 's/.*"tag_name":"\([^"]*\)".*/\1/')
ARTIFACT_URL="https://github.com/dmweis/DynamixelServo/releases/download/$LATEST_VERSION/$FILE_NAME"
echo Downloading...
wget $ARTIFACT_URL -q --show-progress
tar -xzf $FILE_NAME
rm $FILE_NAME
chmod +x run_linux.sh
echo script run_linux.sh will run the program
