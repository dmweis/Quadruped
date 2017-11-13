#! /bin/bash
rm -rf quadruped/*
cp downloadServer.sh quadruped/download.sh
cd quadruped
./download.sh
cd ..

