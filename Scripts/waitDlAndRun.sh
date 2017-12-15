#! /bin/bash

rm -rf quadruped/*
cp downloadServer.sh quadruped/download.sh
CURRENT_VERSION=$(./checkNewestVersion.sh)
echo $CURRENT_VERSION
echo waiting for new
while [ "$CURRENT_VERSION" = "$(./checkNewestVersion.sh)" ]
do
        sleep 8
done
echo new version found
echo $CURRENT_VERSION
cd quadruped
./download.sh
echo running new version
./run_linux.sh
echo server exited
echo process ended
cd ..
