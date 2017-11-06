#! /bin/bash

echo "Starting spider with port /dev/ttyACM0"
echo '{ "SerialPortName": "/dev/ttyACM0" }' > ./QuadrupedConfig.json
dotnet ./Quadruped.WebInterface.dll
