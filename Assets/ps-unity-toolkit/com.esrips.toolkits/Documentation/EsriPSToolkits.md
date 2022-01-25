# Esri PS Toolkits

# Overview
This package contains useful tooling that we have developed in PS while learning Unity. 

# Package Contents
You will find the following resources under Runtime that we expect will help in your projects:
* FeatureService
* GeometryHelper
* MainThreadDispatcher
* RouteService

# Samples
Explore the sample scenes to see how to use the Esri PS Toolkit functionality.

* URP\GDELTScene
  * This scene uses a simple Particle System Prefab and the Baseline.csv script under /Scripts to fetch data from GDELT and dispay it on a globe. More importantly, it demonstrates how the RequestFeaturesCR method on FeatureService can be used with callback and a Coroutine to ensure data is loaded without blocking your scene.