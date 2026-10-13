# CampusXR Explorer – Deployment Guide

**Project:** CampusXR Explorer  
**Module:** PRJ381  
**Institution:** Belgium Campus iTversity  
**Campus:** Kempton Park, South Africa  
**Development Engine:** Unity 6  
**Programming Language:** C#  
**Primary VR Platform:** Meta Quest 3  
**Project Stage:** Milestone 2 – Working VR Prototype  
**Document Status:** Updated Deployment Guide

---

## 1. Introduction

CampusXR Explorer is an immersive virtual reality campus-tour application developed for Belgium Campus iTversity's Kempton Park campus.

The application allows prospective students and visitors to explore campus facilities through interactive 360-degree panoramic environments, navigation controls, campus maps and information hotspots.

This deployment guide explains the development environment, project setup, VR build process, installation, system verification and maintenance procedures.

The development team has confirmed that the implemented VR prototype is operational and that its core features have passed functional testing.

## 2. System Requirements

### 2.1 Hardware Requirements

The following hardware is required or recommended for development and VR deployment:

- A computer capable of running Unity 6.
- Sufficient storage for the Unity project and 360-degree panoramic assets.
- Meta Quest 3 VR headset.
- Compatible USB cable for connecting the headset to the development computer.
- Internet connection for downloading project dependencies and synchronising the GitHub repository.

### 2.2 Software Requirements

| Software | Purpose |
|---|---|
| Unity Hub | Managing Unity installations and projects |
| Unity 6 | Main application development engine |
| C# | Application logic and navigation functionality |
| Android Build Support | Building the application for Meta Quest |
| Android SDK, NDK and OpenJDK | Android build dependencies |
| OpenXR | VR runtime integration |
| XR Plugin Management | Managing XR platform configurations |
| XR Interaction Toolkit | VR interaction components |
| Unity Input System | Handling user interaction |
| Git | Source control |
| GitHub | Repository hosting and collaboration |
| Visual Studio / VS Code | C# development and debugging |

The project should be opened using the Unity Editor version recorded in its project configuration.

## 3. Project Setup

### 3.1 Obtain the Project

1. Open the CampusXR Explorer GitHub repository.
2. Clone or download the repository to the development computer.
3. Extract the project files if downloaded as a ZIP archive.
4. Confirm that the Unity project folders and files are present.

### 3.2 Open the Project in Unity

1. Launch Unity Hub.
2. Select **Add Project**.
3. Locate the CampusXR Explorer Unity project directory.
4. Select the project.
5. Open it using the compatible Unity 6 version.
6. Allow Unity to import the assets, scripts and required packages.

### 3.3 Verify Project Configuration

Before running or building the application, confirm that:

- Required Unity scenes are included.
- 360-degree panoramic assets are available.
- Campus JSON data is included.
- Navigation and hotspot scripts are present.
- XR packages are correctly configured.
- Required input actions and interaction components are assigned.
- No build-blocking compilation errors are present.

## 4. Application Architecture

CampusXR Explorer follows a modular Unity-based architecture.

The application includes the following components:

### 4.1 Presentation Layer

The presentation layer provides the user-facing interface, including:

- Main menu.
- Campus map.
- 360-degree panoramic tour.
- Navigation arrows.
- Information hotspot icons.
- Location information popups.
- Interactive buttons.

### 4.2 Navigation Layer

Navigation functionality manages movement between campus locations and Unity scenes.

The main components include:

**SceneTransitionManager.cs**

Manages scene transitions and destination location identifiers.

**SiteArrivalHandler.cs**

Activates the correct panoramic environment when a visitor enters a location.

**SceneDataLoader.cs**

Manages the active campus location and provides location data to other application components.

### 4.3 Data Layer

Campus location and hotspot information is stored in a JSON configuration file.

The application uses C# data models and loading components to process this information.

**JsonLoader.cs**

Loads campus information from the local JSON data file into the application.

**CampusDataValidator.cs**

Supports validation of campus locations, hotspot information and navigation references.

### 4.4 VR Interaction Layer

The VR interaction layer uses OpenXR, XR Plugin Management, XR Interaction Toolkit and Unity Input System components.

These technologies support interaction with the panoramic environments and application interface.

For further information, see the [System Architecture Documentation](../architecture/Architecture.md).

## 5. Campus Data Configuration

CampusXR Explorer uses a JSON-based data structure for managing campus locations and interactive content.

The primary data file is:

`Assets/StreamingAssets/campus_data.json`

The JSON configuration contains:

- Campus information.
- Location identifiers.
- Unity scene names.
- Location labels.
- Thumbnail asset references.
- Information hotspots.
- Hotspot positions.
- Hotspot titles and descriptions.
- Hotspot image references.
- Navigation target identifiers.
- Navigation directions and labels.

### 5.1 Data Loading

The application loads the JSON data and converts it into the appropriate C# data objects.

The loaded information is used to display campus content and determine navigation destinations.

### 5.2 Location Navigation

Each configured location contains links to connected campus environments.

Navigation controls use the target identifiers to determine which location should be displayed.

### 5.3 Information Hotspots

Information hotspots provide additional information about campus facilities.

A hotspot can include:

- A unique identifier.
- A position in the panoramic environment.
- A title.
- A description.
- An image.
- A destination location.
- A navigation button label.

The team reports that campus data loading, navigation and information hotspots are working correctly in the current VR prototype.

## 6. Meta Quest 3 Deployment

### 6.1 Prepare the VR Headset

1. Ensure the Meta Quest 3 headset is charged.
2. Enable Developer Mode if required for development installation.
3. Connect the headset to the development computer.
4. Allow USB debugging permissions when requested.
5. Confirm that the development computer can communicate with the headset.

### 6.2 Configure Unity for VR Deployment

1. Open CampusXR Explorer in Unity.
2. Navigate to **File → Build Profiles**.
3. Select the Android build target.
4. Confirm that Android Build Support is installed.
5. Verify that the project's XR settings are configured correctly.
6. Ensure the correct application scenes are included.
7. Confirm that the required campus assets and data files are packaged with the application.

### 6.3 Build the Application

1. Select the configured Android build profile.
2. Verify the application identifier and required build settings.
3. Select **Build** or **Build and Run**.
4. Choose a suitable location for the generated APK.
5. Wait for Unity to complete the build.
6. Confirm that the build completes without errors.

### 6.4 Install the Application

1. Connect the Meta Quest 3 to the development computer.
2. Install the generated APK using the team's configured deployment method.
3. Locate the installed application on the headset.
4. Launch CampusXR Explorer.
5. Confirm that the VR environment starts successfully.

The exact installation method and build settings should match the configuration used by the development team.

## 7. Implemented VR Features

The development team has confirmed that the implemented CampusXR Explorer VR features are working successfully.

| Feature | Description | Status |
|---|---|---|
| Application startup | Launches the VR application | Working |
| 360-degree environments | Displays panoramic campus locations | Working |
| Campus navigation | Allows movement between connected locations | Working |
| Navigation arrows | Provides directional navigation controls | Working |
| Information hotspots | Displays interactive campus information | Working |
| Hotspot titles | Shows the selected facility name | Working |
| Hotspot descriptions | Provides information about campus facilities | Working |
| Hotspot images | Displays relevant images | Working |
| Linked-location buttons | Opens the selected campus destination | Working |
| Campus map | Supports campus location selection | Working |
| JSON data loading | Loads configured campus information | Working |
| VR interaction | Supports interaction with the virtual environment | Working |

**Overall Status:** The development team reports that all listed implemented VR features are functioning correctly.

## 8. Testing and System Verification

Functional testing was conducted by the development team to verify that the VR application meets its main operational requirements.

### 8.1 Functional Testing

Testing included:

1. Launching the application.
2. Opening and viewing 360-degree campus environments.
3. Navigating between connected locations.
4. Selecting information hotspots.
5. Viewing hotspot titles, images and descriptions.
6. Closing information popups.
7. Using linked-location navigation buttons.
8. Opening and using the campus map.
9. Loading campus data.
10. Interacting with the application in VR.

### 8.2 Testing Results

The development team confirmed that the implemented VR prototype features passed functional testing.

The VR demonstration also provides visual evidence of panoramic campus locations, navigation controls and information hotspots.

### 8.3 User Acceptance Testing

Development-team acceptance testing was performed against the application's main visitor workflows.

The team reports that the implemented VR features meet the current Milestone 2 acceptance criteria.

Independent testing with prospective students may be conducted as part of later evaluation.

For detailed information, see the [Testing and User Acceptance Testing Documentation](../testing/Testing-and-UAT.md).

## 9. Troubleshooting and System Verification

The development team has confirmed that the implemented CampusXR Explorer VR features are working correctly.

The following components have been tested:

- Application startup and loading.
- 360-degree panoramic environments.
- Navigation between campus locations.
- Interactive information hotspots.
- Campus JSON data loading.
- VR headset controls and interactions.
- Campus map and location selection.

**Current Status:** All listed components are reported as working successfully.

If issues occur during future deployments, the development team can review:

- Unity build settings.
- Scene configuration.
- Navigation links and destination identifiers.
- JSON data configuration.
- XR input and interaction settings.
- Project assets and dependencies.

These checks are included for future maintenance and troubleshooting. They do not indicate that the application currently has unresolved problems.

## 10. Maintenance and Future Updates

The CampusXR Explorer VR prototype has successfully completed development-team functional testing for Milestone 2.

Future maintenance will focus on:

- Keeping campus images and location information updated.
- Maintaining compatibility with Unity and Meta Quest devices.
- Updating the application when new campus facilities are added.
- Performing regression testing after future changes.
- Improving performance and user experience where necessary.
- Maintaining GitHub documentation and version control.

**Current Status:** The implemented VR prototype is operational, with future maintenance and enhancements planned for later milestones.

## 11. Android Smartphone Deployment

CampusXR Explorer also includes a planned Android smartphone deployment target.

The smartphone version requires a suitable Unity build configuration and touch-compatible interaction controls.

The general process involves:

1. Configuring the Android build profile.
2. Checking smartphone input compatibility.
3. Building an Android APK.
4. Installing the APK on a supported smartphone.
5. Testing panoramic viewing, navigation and information hotspots.
6. Recording device compatibility and test results.

**Deployment Status:** The Android smartphone version requires separate build and testing confirmation. The team's successful Meta Quest 3 VR testing does not automatically verify smartphone compatibility.

## 12. Deployment Summary

CampusXR Explorer has reached the stage of a working VR prototype for PRJ381 Milestone 2.

The development team confirms that the implemented features have been tested successfully, including:

- 360-degree panoramic environments.
- Interactive campus navigation.
- Information hotspots.
- Location information popups.
- Campus map functionality.
- JSON-based campus data loading.
- VR interactions.

The application provides an immersive way for prospective students and visitors to explore selected facilities at Belgium Campus iTversity's Kempton Park campus.

The deployment process supports continued development, testing and maintenance as the project progresses towards later milestones.

---

**Project:** CampusXR Explorer  
**Module:** PRJ381  
**Document:** Deployment Guide  
**Milestone:** 2  
**Status:** Working VR Prototype – Team Testing Completed
