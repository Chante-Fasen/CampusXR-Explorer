# CampusXR Explorer

## Immersive AR/VR Campus Tour for Belgium Campus Open Day

CampusXR Explorer is an immersive campus-tour application developed for the
Belgium Campus iTversity Kempton Park campus. The application is designed to
allow prospective students and visitors to explore selected campus locations
through interactive 360-degree environments and VR functionality.

## Main Features

- Interactive campus navigation
- 360-degree campus environments
- Campus map
- Location selection
- Interactive hotspots
- Multimedia and narration support
- VR/XR functionality
- Android support
- Anonymous Firebase analytics subject to user consent

## Technology Stack

- Unity 6
- C#
- OpenXR
- Unity XR Plugin Management
- Android
- JSON / StreamingAssets
- Firebase / Cloud Firestore
- Git and GitHub

## Data Architecture

Campus location and hotspot information is stored in `campus_data.json`.
`JsonLoader` loads this data into the application and `SceneDataLoader`
identifies the information associated with the active Unity scene.

## Current Development Status

The project is currently under active development for Milestone 2.
Campus imagery has been captured and an initial VR prototype has been
demonstrated on the available headset.

Android APK deployment, compatibility testing, additional campus content,
navigation improvements and final usability testing are still in progress.

## Privacy

Analytics functionality is designed to operate only after consent has been
provided. Anonymous session identifiers are used instead of personally
identifiable information.

## Project

PRJ381 – Belgium Campus iTversity  
Kempton Park Campus  
Project: Immersive AR/VR Campus Tour Mobile Application for Belgium Campus Open Day
