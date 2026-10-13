# CampusXR Explorer

## Immersive AR/VR Campus Tour for Belgium Campus Open Day

CampusXR Explorer is an interactive virtual campus-tour application developed for Belgium Campus iTversity's Kempton Park campus as part of PRJ381.

The application allows prospective students and visitors to explore campus facilities through immersive 360-degree panoramic environments, interactive navigation and information hotspots.

The project is developed using Unity 6 and C#, with Meta Quest 3 as the primary VR testing platform. A separate Android smartphone version is planned.

## Project Objectives

The main objectives of CampusXR Explorer are to:

- Provide an immersive virtual experience of the Belgium Campus Kempton Park campus.
- Allow visitors to explore indoor and outdoor campus locations.
- Support navigation between connected 360-degree environments.
- Display useful information about campus facilities through interactive hotspots.
- Provide an accessible way for prospective students to become familiar with the campus before visiting.
- Develop and evaluate a functional VR prototype for PRJ381 Milestone 2.

## Main Features

### Implemented VR Prototype Features

The development team has implemented and demonstrated:

- Interactive 360-degree campus environments.
- Indoor and outdoor campus locations.
- Navigation arrows between connected locations.
- Information hotspots containing titles, images and descriptions.
- Buttons for entering linked locations.
- Location-based panoramic environment switching.
- Campus map and location selection, reported working by the development team.
- VR functionality using Unity XR technologies.

### Additional Features and Integrations

The project also includes or plans:

- Multimedia and narration functionality.
- Settings and help interfaces.
- Firebase Cloud Firestore analytics with user consent.
- A separate Android smartphone version.

These features remain subject to implementation and testing confirmation where applicable.

## Technology Stack

| Category | Technology |
|---|---|
| Development Engine | Unity 6 |
| Programming Language | C# |
| VR Framework | OpenXR |
| XR Tools | Unity XR Plugin Management and XR Interaction Toolkit |
| Input | Unity Input System |
| User Interface | Unity UI |
| Campus Content | 360-degree panoramic images |
| Local Data Storage | JSON and Unity StreamingAssets |
| Analytics Integration | Firebase Cloud Firestore |
| Version Control | Git and GitHub |
| Primary VR Device | Meta Quest 3 |
| Additional Target Platform | Android smartphone (planned) |

## System Architecture

CampusXR Explorer uses a modular Unity-based architecture that separates user interaction, navigation, campus data and immersive environments.

The application includes:

- A presentation layer containing menus, maps, hotspot popups and the tour interface.
- Navigation components responsible for changing locations and scenes.
- A data layer that loads and manages campus information.
- An XR layer supporting VR interaction.
- An analytics integration designed to operate with user consent.

For detailed information, see the [System Architecture Documentation](docs/architecture/Architecture.md).

## Data Architecture

Campus location information is stored in:

`Assets/StreamingAssets/campus_data.json`

The data is represented by C# classes defined in `CampusData.cs`, including:

- `CampusData`
- `LocationData`
- `HotspotData`
- `LinkData`
- `Vector3Data`

The `JsonLoader` component reads and deserializes the JSON data using `UnityWebRequest`.

`SceneDataLoader` manages the currently selected location and notifies subscribed components when the location changes.

`SiteArrivalHandler` activates the appropriate 360-degree panorama GameObject, while `SceneTransitionManager` manages scene transitions and destination site identifiers.

For more information, see the [UML Documentation](docs/uml/UML.md).

## Installation and Deployment

### Development Setup

1. Clone the CampusXR Explorer repository.
2. Open the Unity project through Unity Hub.
3. Use the Unity Editor version specified by the project.
4. Ensure the required XR packages are installed.
5. Open the configured starting scene.
6. Run the project in Unity for development and testing.

### Meta Quest 3

The project includes a VR prototype developed and demonstrated for Meta Quest 3.

The deployment process involves configuring the Unity Android build profile, enabling the required XR settings, building the application and installing it on the headset.

The exact configuration should be taken from the team's working build settings.

### Android Smartphone

A separate Android smartphone version is planned. Its final deployment and compatibility testing remain to be confirmed.

For detailed setup instructions, see the [Deployment Guide](docs/deployment/Deployment-Guide.md).

## Testing and User Acceptance Testing

The development team reports that the implemented VR prototype features have been tested successfully.

Functional testing covers:

- Application startup.
- Campus location selection.
- 360-degree environment display.
- Navigation arrows.
- Interactive hotspots.
- Hotspot titles, images and descriptions.
- Linked-location buttons.
- Campus map functionality.

The team's demonstration provides visual evidence of several campus environments and information hotspots.

Development-team acceptance testing has also been documented. Independent user testing and further performance evaluation may be conducted during later development.

See the [Testing and UAT Documentation](docs/testing/Testing-and-UAT.md).

## Current Development Status

**PRJ381 Milestone 2 – Functional VR Prototype**

The team has developed and demonstrated an interactive VR campus-tour prototype.

The current prototype includes multiple campus environments, navigation controls and information hotspots.

The team reports that the implemented core VR features are working and have passed functional testing.

Further development focuses on completing outstanding content, improving usability, verifying optional integrations and preparing the planned Android smartphone version.

## Privacy and Analytics

CampusXR Explorer includes an analytics integration designed to record anonymous application events through Firebase Cloud Firestore.

The intended design requires user consent before analytics events are recorded.

Analytics implementation and consent behaviour should be verified before being described as fully operational.

## Project Documentation

| Document | Description |
|---|---|
| [System Architecture](docs/architecture/Architecture.md) | Application structure and component relationships |
| [Technology Stack](docs/technology-stack/Technology-Stack.md) | Hardware, software and development technologies |
| [UML Diagrams](docs/uml/UML.md) | Use Case, Data Class and Data Loading Sequence diagrams |
| [Deployment Guide](docs/deployment/Deployment-Guide.md) | Project setup, Quest deployment and troubleshooting |
| [Testing and UAT](docs/testing/Testing-and-UAT.md) | Functional test cases, team-reported results and acceptance testing |

## Future Improvements

Planned improvements include:

- Further usability testing with prospective students.
- Additional performance testing on Meta Quest 3.
- Refinement of navigation and hotspot interactions.
- Completion and verification of remaining multimedia features.
- Verification of analytics and consent functionality.
- Development and testing of the Android smartphone version.
- Continued stability and compatibility improvements.

## Project Information

**Project:** CampusXR Explorer  
**Module:** PRJ381  
**Institution:** Belgium Campus iTversity  
**Campus:** Kempton Park, South Africa  
**Project Type:** Immersive AR/VR Campus Tour Mobile Application for Belgium Campus Open Day  
**Current Stage:** Milestone 2 – VR Prototype
