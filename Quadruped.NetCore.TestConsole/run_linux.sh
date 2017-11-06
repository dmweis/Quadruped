#! /bin/bash

echo "Starting spider with port /dev/ttyACM0"
dotnet ./Quadruped.NetCore.TestConsole.dll /dev/ttyACM0
