#pragma once

#include "Platform.h"

class StopWatch { 
  LARGE_INTEGER frequency_;
  LARGE_INTEGER startTime_; 
  LARGE_INTEGER stopTime_; 

public: 
StopWatch() {
  if (!::QueryPerformanceFrequency(&frequency_)) throw "Error with QueryPerformanceFrequency"; 
} 

void Start() {
   ::QueryPerformanceCounter(&startTime_); 
} 

void Stop() {
   ::QueryPerformanceCounter(&stopTime_); 
} 

float StopWatch::MilliSeconds() const { 
   float v = ((float)stopTime_.QuadPart - (float)startTime_.QuadPart) / ((float)frequency_.QuadPart / 1000.0f);
   return v;
} 

__int64 StopWatch::Ticks() const { 
  return (__int64)(stopTime_.QuadPart - startTime_.QuadPart);// / (float) frequency_.QuadPart; 
} 
}; 