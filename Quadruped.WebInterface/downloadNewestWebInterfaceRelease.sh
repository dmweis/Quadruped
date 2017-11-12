#! /bin/bash

FILE_NAME=arm_web_deploy.tar.gz
LATEST_RELEASE=$(curl -L -s -H 'Accept: application/json' https://github.com/dmweis/Quadruped/releases/latest)
# The releases are returned in the format {"id":3622206,"tag_name":"hello-1.0.0.11",...}, we have to extract the tag_name.
LATEST_VERSION=$(echo $LATEST_RELEASE | sed -e 's/.*"tag_name":"\([^"]*\)".*/\1/')
ARTIFACT_URL="https://github.com/dmweis/Quadruped/releases/download/$LATEST_VERSION/$FILE_NAME"
echo Latest version is $LATEST_VERSION
echo Downloading...
wget $ARTIFACT_URL -q --show-progress
echo unpacking...
tar -xzf $FILE_NAME
echo removing archive
rm $FILE_NAME
chmod +x run_linux.sh
echo script run_linux.sh will run the program
