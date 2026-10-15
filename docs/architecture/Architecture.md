# CampusXR Explorer – System Architecture

## System Architecture Diagram

![CampusXR System Architecture](system-architecture.png)

## Architecture Overview

CampusXR Explorer is an immersive 360-degree VR campus-tour application developed using Unity 6 and C# for Belgium Campus iTversity, Kempton Park.

The application uses a modular architecture consisting of four main layers: Presentation, Application, Local Data and Cloud. These layers work together to provide interactive campus navigation, guided tours, information hotspots, audio narration and consent-based analytics.

## 1. Presentation Layer

The Presentation Layer manages the user interface and VR experience.

It includes:

- **MainMenu:** Start screen, POPIA consent and Guided Tour or Free Roam selection.
- **SplashScreen:** Displays the loading screen before entering the tour.
- **Tour360:** Provides 360-degree campus environments, navigation arrows, information hotspots, campus map and settings.
- **XR Rig:** Supports VR interaction using the Meta Quest 3 headset and controllers.

The main scene flow is:

`MainMenu → SplashScreen → Tour360`

## 2. Application Layer

The Application Layer controls the main functionality of the campus tour.

- **NavigationArrowManager:** Manages navigation arrows and connected campus locations.
- **Guided Tour:** Provides a predefined route with red arrows highlighting the next destination.
- **Hotspot System:** Displays information about campus facilities, including images, descriptions and navigation buttons.
- **TourVoiceover and AudioManager:** Manage audio narration during the guided tour.
- **SceneTransitionManager and SiteArrivalHandler:** Handle scene transitions and activate the correct campus environment.
- **ConsentManager:** Manages visitor permission for analytics collection.
- **AnalyticsLogger:** Records permitted visitor interactions.

Visitors can explore the campus using either Guided Tour or Free Roam mode.

## 3. Local Data Layer

The Local Data Layer manages campus information using `StreamingAssets/campus_data.json`.

The main components include:

- **JsonLoader:** Reads and loads the campus JSON data.
- **CampusData:** Defines the C# data models for locations, navigation links and hotspots.
- **SceneDataLoader:** Identifies the active campus location and provides its information.
- **CampusDataValidator:** Checks the data for missing links, duplicate identifiers and other configuration issues.

This data-driven approach allows campus locations and hotspots to be updated without changing the main application logic.

## 4. Cloud Layer

The Cloud Layer uses Firebase services to support anonymous visitor sessions and analytics.

- **Firebase Anonymous Authentication:** Supports anonymous visitor identification when analytics consent is accepted.
- **Firebase Cloud Firestore:** Stores consent-based session information and analytics events.
- **Consent-Based Analytics:** Allows visitor interactions to be recorded only when permission has been granted.

Visitors who decline analytics consent can still use the campus tour.

## 5. Deployment and Technology

CampusXR Explorer uses:

- Unity 6 and C# for application development.
- OpenXR and XR Interaction Toolkit for VR functionality.
- Meta Quest 3 and Touch controllers for immersive interaction.
- JSON for campus data management.
- Firebase Authentication and Cloud Firestore for consent-based analytics.
- Git and GitHub for version control.

The VR prototype has been demonstrated on a Meta Quest headset. Android smartphone deployment is managed separately.

## 6. Architecture Summary

CampusXR Explorer combines an interactive VR interface, guided and free-roam navigation, audio narration, structured JSON data and Firebase services.

The four-layer architecture separates the application's responsibilities, making it easier to maintain, test and extend with additional campus locations and functionality.

The architecture supports the development of the CampusXR Explorer prototype for PRJ381 Milestone 2.
