#! /bin/bash
rm -rf quadruped/*
cp downloadServer.sh quadruped/download.sh
cd quadruped
./download.sh
echo running new version
./run_linux.sh
echo server exited
echo process ended
cd ..

