#! /bin/bash

echo "Starting spider with port /dev/ttyACM0"
dotnet ./DynamixelServo.NetCore.TestConsole.dll /dev/ttyACM0
