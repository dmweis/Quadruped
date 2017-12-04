#include <SPI.h>
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_LSM9DS0.h>

Adafruit_LSM9DS0 lsm = Adafruit_LSM9DS0(1000);  // Use I2C, ID #1000

void configureSensor(void)
{
  lsm.setupAccel(lsm.LSM9DS0_ACCELRANGE_2G);
  //lsm.setupAccel(lsm.LSM9DS0_ACCELRANGE_4G);
  //lsm.setupAccel(lsm.LSM9DS0_ACCELRANGE_6G);
  //lsm.setupAccel(lsm.LSM9DS0_ACCELRANGE_8G);
  //lsm.setupAccel(lsm.LSM9DS0_ACCELRANGE_16G);
  
  lsm.setupMag(lsm.LSM9DS0_MAGGAIN_2GAUSS);
  //lsm.setupMag(lsm.LSM9DS0_MAGGAIN_4GAUSS);
  //lsm.setupMag(lsm.LSM9DS0_MAGGAIN_8GAUSS);
  //lsm.setupMag(lsm.LSM9DS0_MAGGAIN_12GAUSS);

  lsm.setupGyro(lsm.LSM9DS0_GYROSCALE_245DPS);
  //lsm.setupGyro(lsm.LSM9DS0_GYROSCALE_500DPS);
  //lsm.setupGyro(lsm.LSM9DS0_GYROSCALE_2000DPS);
}

void setup(void) 
{
    Serial.begin(9600);
#ifndef ESP8266
  while (!Serial);
#endif
  
  if(!lsm.begin())
  {
    Serial.print(F("no LSM9DS0 detected"));
    while(1);
  } 
  configureSensor();
}

void loop(void) 
{  
  sensors_event_t accel, mag, gyro, temp;

  lsm.getEvent(&accel, &mag, &gyro, &temp); 

  Serial.print("{\"acceleration\":{\"x\":"); 
  Serial.print(accel.acceleration.x); 
  Serial.print(",\"y\":");
  Serial.print(accel.acceleration.y);
  Serial.print(",\"z\":");
  Serial.print(accel.acceleration.z);
  // print out magnetometer data
  Serial.print("},\"magnetic\":{\"x\":");
  Serial.print(mag.magnetic.x);
  Serial.print(",\"y\":");
  Serial.print(mag.magnetic.y);
  Serial.print(",\"z\":");
  Serial.print(mag.magnetic.z);
  // print out gyroscopic data
  Serial.print("},\"gyroscopic\":{\"x\":");
  Serial.print(gyro.gyro.x);
  Serial.print(",\"y\":"); 
  Serial.print(gyro.gyro.y);
  Serial.print(",\"z\":");
  Serial.print(gyro.gyro.z);
  // print out temperature data
  Serial.print("},\"temperature\":"); 
  Serial.print(temp.temperature);
  Serial.println("}");

  delay(20);
}