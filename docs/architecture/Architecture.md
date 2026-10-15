# CampusXR Explorer – System Architecture

## System Architecture Diagram

![CampusXR Explorer System Architecture](./system-architecture.png)


## Architecture Overview

CampusXR Explorer is an immersive 360-degree virtual reality campus-tour application developed using Unity 6 and C# for Belgium Campus iTversity, Kempton Park.

The system follows a modular architecture divided into four main layers:

1. **Presentation Layer** – Manages the user interface and VR experience.
2. **Application Layer** – Handles navigation, hotspots, guided tours, audio and visitor consent.
3. **Local Data Layer** – Loads and manages campus information stored in JSON.
4. **Cloud Layer** – Supports Firebase authentication and consent-based analytics.

This separation makes the system easier to maintain, test and extend.

## 1. Presentation Layer

The Presentation Layer manages the visitor's interaction with the VR application.

### Main Components

- **MainMenu:** Allows visitors to start the application, provide POPIA consent and select Guided Tour or Free Roam mode.
- **SplashScreen:** Displays a loading screen while the application prepares the tour environment.
- **Tour360:** Provides the immersive 360-degree campus experience, including the campus map, settings and information hotspots.
- **XR Rig:** Supports interaction using the Meta Quest 3 headset and Touch controllers.

### Scene Flow

`MainMenu → SplashScreen → Tour360`

The visitor begins at the main menu, proceeds through the loading screen and enters the virtual campus environment.

## 2. Application Layer

The Application Layer contains the main functionality and interaction logic of CampusXR Explorer.

### NavigationArrowManager

Manages navigation arrows using campus location and link data. It helps visitors move between connected locations and works with the scene-transition system.

### Hotspot Interaction

Displays information about campus facilities, including descriptions, images and available audio or navigation options.

### Guided Tour

Provides a structured campus-tour experience using route information and red navigation highlights. The guided-tour functionality uses shortest-path search to determine routes between locations.

### TourVoiceover and AudioManager

Manage audio playback and narration associated with the guided-tour experience. Audio is used to provide additional information about campus locations.

### SceneTransitionManager and SiteArrivalHandler

Manage transitions between campus environments and handle actions required when a visitor arrives at a new location.

### ConsentManager

Handles POPIA consent and determines whether analytics collection is permitted.

It also supports the anonymous authentication process after the visitor grants the relevant consent.

### AnalyticsLogger

Records permitted visitor activities, such as sessions, facility views and hotspot interactions.

Analytics logging depends on visitor consent.

### Application Interaction Flow

The architecture diagram illustrates these primary relationships:

- **NavigationArrowManager → SceneTransitionManager:** Navigation between campus locations.
- **Hotspot Interaction → SceneTransitionManager:** Location changes initiated through hotspot navigation, where supported.
- **Guided Tour → TourVoiceover and AudioManager:** Audio narration during guided-tour stops.
- **ConsentManager → AnalyticsLogger:** Enables analytics logging when consent is granted.
- **ConsentManager → Firebase Anonymous Authentication:** Supports anonymous authentication after consent.

Guided Tour and Free Roam provide different ways of exploring the same campus environment.

## 3. Local Data Layer

The Local Data Layer stores and manages the campus information required by the application.

### Data Source

`StreamingAssets/campus_data.json`

This JSON file contains information about campus locations, navigation links, hotspots and related content.

### Main Components

- **JsonLoader:** Reads and parses the JSON data.
- **CampusData:** Represents the campus information using C# data models.
- **LocationData:** Stores information about individual campus locations.
- **HotspotData:** Represents interactive information points.
- **LinkData:** Defines connections between campus locations.
- **SceneDataLoader:** Provides the required location and scene information to the application.
- **CampusDataValidator:** Checks campus data for configuration issues such as missing links or duplicate identifiers.

### Data Flow

`campus_data.json → JsonLoader → C# Data Models → SceneDataLoader`

The loaded data is used by the application components:

- **Location and link data → NavigationArrowManager**
- **Hotspot data → Hotspot Interaction**

This data-driven structure allows campus information to be updated without requiring major changes to the application logic.

## 4. Cloud Layer

The Cloud Layer uses Firebase services to support anonymous visitor sessions and analytics storage.

### Firebase Anonymous Authentication

Provides anonymous authentication after the required visitor consent has been accepted.

### Firebase Cloud Firestore

Stores information associated with visitor sessions and permitted analytics events.

Examples include:

- Anonymous visitor and session records.
- Campus facility views.
- Hotspot interaction events.
- Other permitted analytics information.

### Consent-Based Analytics

The ConsentManager determines whether analytics logging is enabled.

When consent is granted, AnalyticsLogger can record permitted events and store the relevant information using Firebase.

Visitors who decline analytics consent can continue exploring the campus without optional analytics collection.

### Cloud Interaction Flow

`ConsentManager → Firebase Anonymous Authentication → Cloud Firestore`

`ConsentManager → AnalyticsLogger → Cloud Firestore`

These flows represent authentication and consent-based analytics responsibilities.

## 5. Deployment and Technology Stack

CampusXR Explorer uses the following technologies:

| Technology | Purpose |
|---|---|
| Unity 6 | VR application development |
| C# | Application logic and functionality |
| OpenXR | VR platform compatibility |
| XR Interaction Toolkit | VR interaction support |
| Meta Quest 3 | VR headset and controllers |
| JSON | Local campus information |
| Firebase Authentication | Anonymous visitor authentication |
| Cloud Firestore | Analytics and session data storage |
| Git and GitHub | Version control and collaboration |

The VR prototype is designed for the Meta Quest environment. Android smartphone deployment is managed as a separate development activity.

## 6. Architecture Design Benefits

The architecture provides several benefits:

- **Modularity:** Each layer has a clearly defined responsibility.
- **Maintainability:** Components can be updated independently.
- **Scalability:** Additional campus locations and hotspots can be added through the data structure.
- **Reusability:** Navigation, audio and scene-management components can support multiple campus locations.
- **Privacy:** Analytics collection is controlled through visitor consent.
- **Testability:** Individual components and interactions can be tested separately.

## 7. Architecture Summary

CampusXR Explorer combines a 360-degree VR interface, interactive campus navigation, guided tours, audio narration, JSON-based campus data and Firebase services.

The Presentation Layer manages the visitor experience, the Application Layer controls interactions, the Local Data Layer supplies campus information, and the Cloud Layer supports anonymous authentication and consent-based analytics.

Together, these layers provide a structured foundation for the CampusXR Explorer prototype developed for **PRJ381 Milestone 2**.