# CampusXR Explorer – System Architecture

## System Architecture Diagram

![CampusXR System Architecture](system-architecture.png)

## Architecture Explanation

CampusXR Explorer uses a modular architecture developed in Unity 6 with C#. The application allows prospective students and visitors to explore Belgium Campus iTversity Kempton Park through interactive 360-degree campus environments. The system is designed for immersive VR experiences, with the current prototype successfully built and demonstrated on a Meta Quest headset.

The architecture consists of several main components: the user interface, navigation, immersive campus experience, data management, XR/VR functionality and analytics. These components work together to provide an interactive virtual campus tour.

### User Interface Layer

The user interface provides the main screens and controls needed to interact with the application. This includes the Main Menu, Building Map, Settings/Help and immersive tour interface.

Users can select campus locations through the interactive campus map and navigate to different areas of the virtual campus. The interface also supports interactive hotspots that display information about campus facilities.

### Navigation Layer

The navigation layer manages movement between different campus locations within the immersive tour.

The application uses interactive navigation arrows that allow users to move between connected 360-degree environments, similar to Google Street View.

Instead of manually placing every navigation arrow, the system reads navigation information from the `links` entries in `campus_data.json`. Each link identifies the destination site, arrow direction and display label.

This data-driven approach makes it easier to update navigation routes and add new campus locations without changing the main navigation logic.

### Immersive Campus Experience Layer

The immersive experience layer displays 360-degree photographs of the Belgium Campus Kempton Park campus within Unity.

Users can explore different campus environments, including classrooms, reception, cafeteria, library and other campus facilities.

Interactive hotspots provide additional information about these locations. Hotspots can display descriptions, images and navigation buttons, allowing users to learn more about specific facilities or enter connected locations.

The campus experience combines the 360-degree environments, navigation arrows and hotspot interactions to create an immersive and interactive virtual tour.

### XR/VR Layer

The XR/VR layer provides the functionality required to run the immersive experience on supported VR hardware.

The application uses OpenXR, Unity XR Plugin Management and the XR Interaction Toolkit to support VR interaction and headset functionality.

The team has successfully built and demonstrated the current prototype on a Meta Quest headset. Further improvements and testing are being completed as part of the final Milestone 2 integration process.

### Local Data Layer

The local data layer separates campus information from the application's main logic.

Campus information is stored in `campus_data.json`, located in Unity's `StreamingAssets` directory. This file contains campus location identifiers, display labels, navigation links, hotspot information, descriptions and image references.

The `JsonLoader` component loads the campus data and converts it into the corresponding C# data objects.

The `SceneDataLoader` component identifies the relevant campus information using site identifiers. The updated implementation also supports scene-name lookup as a fallback.

The data structure is defined in `CampusData.cs`, which contains the classes used to represent campus locations, hotspots and navigation links.

The project also includes `CampusDataValidator.cs`, which checks the JSON data for potential problems such as duplicate identifiers, missing navigation targets and excessive hotspots.

This approach allows the team to maintain and update campus content without having to modify the main application logic for every content change.

### Analytics Layer

The analytics layer uses `AnalyticsLogger` and Firebase Cloud Firestore to support consent-based analytics event logging.

The system is designed to record user interactions such as viewing campus locations and activating hotspots. These events can help the team understand how visitors interact with the virtual campus tour.

Analytics functionality is intended to operate only when the user has provided the required consent.

The team is completing the final integration and verification of Firebase event logging for the Milestone 2 demonstration.

### Deployment and Target Devices

CampusXR Explorer is developed using Unity and is designed to support immersive campus experiences on compatible hardware.

The current VR prototype has been successfully built and demonstrated on a Meta Quest headset.

Android smartphone deployment is being handled as a separate build configuration because mobile devices require touchscreen controls and non-VR viewing functionality.

Separating the Quest and smartphone configurations helps prevent device-specific settings from interfering with the working VR application.

### Architecture Summary

The CampusXR Explorer architecture combines Unity-based interfaces, data-driven navigation, interactive 360-degree environments, XR/VR functionality, structured JSON data and Firebase analytics.

By separating these responsibilities into different components, the application is easier to maintain, test and extend. Additional campus locations, navigation links and hotspot information can be incorporated through the structured data system.

The current architecture supports the working Meta Quest prototype while allowing further improvements to analytics, mobile compatibility and the overall campus-tour experience.
