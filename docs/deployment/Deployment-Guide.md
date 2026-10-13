# CampusXR Explorer – Deployment Guide

**Project:** CampusXR Explorer  
**Module:** PRJ381  
**Milestone:** Milestone 2  
**Platform:** Unity 6 – Meta Quest 3 VR  
**Deployment Status:** VR prototype developed; final deployment verification ongoing

---

## 1. Introduction

CampusXR Explorer is an interactive virtual campus tour developed using Unity 6 and C#. The application allows visitors and prospective students to explore campus locations through 360-degree panoramic environments, interact with information hotspots, and navigate between connected areas.

The primary target platform is the Meta Quest 3 virtual reality headset. A separate Android smartphone version is planned but has not yet been confirmed as successfully built or tested.

This document describes the development environment, software and hardware requirements, project setup, data configuration, deployment workflow, testing procedures and troubleshooting guidance.

The purpose of this guide is to support consistent project setup, testing and deployment by the development team.

## 2. Hardware Requirements

| Hardware | Purpose | Status |
|---|---|---|
| Windows development laptop or PC | Unity development, testing and building | Required |
| Meta Quest 3 | Running and testing the VR application | Primary target |
| USB-C data cable | Connecting Quest 3 to development PC | Required for USB deployment |
| Android smartphone | Testing a separate mobile version | Planned |
| Internet connection | Downloading project dependencies and collaborating through GitHub | Required for initial setup |

The development computer must meet the requirements of the Unity version used by the project.

## 3. Software Requirements

| Software / Technology | Purpose |
|---|---|
| Unity Hub | Managing Unity installations and projects |
| Unity Editor 6 (6000.5.6f1) | Main development environment |
| C# | Application scripting |
| OpenXR Plugin (1.17.1) | VR runtime integration |
| XR Interaction Toolkit (3.5.1) | VR interaction functionality |
| XR Plugin Management (4.5.4) | Managing XR providers |
| Unity Input System (1.20.0) | Input handling |
| Android Build Support | Building applications for Quest |
| Android SDK, NDK and OpenJDK | Android build toolchain |
| Git and GitHub | Version control and collaboration |
| Visual Studio or VS Code | Editing and reviewing C# scripts |
| Unity UI | Application menus, maps and information popups |

Package versions should be checked against the project's `Packages/manifest.json` and `Packages/packages-lock.json` before installation or troubleshooting.

## 4. Project Repository

The project is maintained in the team's GitHub repository:

**Repository:** `CampusXR-Explorer`

The repository contains the Unity project, C# scripts, campus media assets, JSON data and project documentation.

### 4.1 Downloading the Project

1. Open GitHub Desktop or a Git-compatible command-line tool.
2. Clone the `CampusXR-Explorer` repository.
3. Choose a suitable local folder for the project.
4. Allow the repository to finish downloading.
5. Open the repository folder and locate the Unity project.

If the Unity project is stored inside a subfolder, select the folder containing `Assets`, `Packages` and `ProjectSettings` when opening it in Unity.

### 4.2 Updating the Project

Before starting development, fetch and pull the latest changes from the shared repository.

Team members should avoid overwriting each other's Unity scene or project settings changes. Significant changes should be reviewed and tested before integration.

## 5. Setting Up Unity

### 5.1 Install Unity

1. Open Unity Hub.
2. Install the Unity Editor version specified by the project.
3. Add Android Build Support.
4. Include the Android SDK, NDK and OpenJDK components.
5. Wait for the installation to finish.

Using the same Unity Editor version across the team reduces compatibility problems.

### 5.2 Open CampusXR Explorer

1. In Unity Hub, select **Add project** or **Add project from disk**.
2. Browse to the local `CampusXR-Explorer` Unity project folder.
3. Select the project.
4. Open it with the matching Unity Editor version.
5. Allow Unity to import assets and resolve dependencies.

The first import may take longer because Unity needs to process the project's media, scripts and packages.

### 5.3 Check Project Packages

Open Unity's Package Manager and confirm that the required packages are installed.

The project uses OpenXR, XR Interaction Toolkit, XR Plugin Management and Unity's Input System for its VR functionality.

Do not upgrade or remove project packages without checking their compatibility and coordinating with the team.

## 6. Project Structure

The following folders and files are important to the deployment workflow.

| Folder or file | Purpose |
|---|---|
| `Assets/` | Unity scenes, scripts, images and application resources |
| `Packages/` | Package configuration and dependency information |
| `ProjectSettings/` | Unity project configuration |
| `StreamingAssets/campus_data.json` | Campus location, hotspot and navigation data |
| `CampusData.cs` | Defines the campus JSON data classes |
| `JsonLoader.cs` | Loads and deserializes the campus JSON file |
| `SceneDataLoader.cs` | Tracks the current location and provides location data |
| `SiteArrivalHandler.cs` | Activates the selected 360° panorama GameObject |
| `SceneTransitionManager.cs` | Manages scene transitions and destination site IDs |
| `docs/` | Architecture, UML, technology stack and deployment documentation |

The JSON file is accessed through Unity's `Application.streamingAssetsPath`. Its exact source location is under the project's `Assets/StreamingAssets/` folder.

## 7. Campus Data Configuration

CampusXR Explorer uses a JSON-based data structure to manage the information displayed in the virtual tour.

The main data file is:

`Assets/StreamingAssets/campus_data.json`

This file contains the campus name and a collection of locations.

Each location may include:

- A unique location ID.
- The associated Unity scene name.
- A display label and thumbnail reference.
- Information hotspots.
- Navigation links to other locations.

### 7.1 Data Loading

The `JsonLoader` component is automatically created before the first scene loads.

It uses `UnityWebRequest` to access `campus_data.json` from the StreamingAssets directory. This approach supports Android-based platforms, where StreamingAssets may be packaged inside the application.

The JSON content is deserialized into a `CampusData` object using Unity's `JsonUtility`.

Once loading succeeds, the data is stored in `JsonLoader.LoadedData`, and the `DataLoaded` event is raised.

### 7.2 Location Identification

Each campus location must have a valid `id`.

The location ID must match the site ID used by `SiteArrivalHandler` and the corresponding navigation or map controls.

For example, a site identified as `ReceptionSite` must use that identifier consistently throughout the relevant configuration.

### 7.3 Navigation Links

Navigation links use the following fields:

| Field | Description |
|---|---|
| `target_id` | Destination location ID |
| `yaw` | Horizontal direction of the navigation arrow, measured in degrees |
| `label` | Optional text displayed with the arrow |

A navigation target should correspond to an existing location ID in the JSON data.

### 7.4 Hotspot Configuration

Hotspots contain information displayed to the visitor, including titles, images and descriptions.

The `world_position` field uses x, y and z coordinates to position the hotspot in the 360-degree environment.

Optional destination information allows certain hotspots to provide navigation to linked locations.

### 7.5 Data Validation

Before deployment, check that:

- The JSON file is valid and readable.
- Every location has a unique ID.
- Navigation target IDs refer to valid locations.
- Hotspot media references are correct.
- Location IDs match the configured Unity site objects.
- The expected number of hotspots is supported by the current implementation.

The project also includes a `CampusDataValidator` script to assist with data validation.

## 8. Running the Prototype in Unity Editor

The Unity Editor allows developers to inspect scenes, scripts and interactive components before building the application.

### Procedure

1. Open CampusXR Explorer in Unity.
2. Open the project's configured starting scene.
3. Check the Unity Console for compilation errors.
4. Press **Play**.
5. Confirm that the application initializes.
6. Check that campus data loads successfully.
7. Open the relevant campus map or navigation interface.
8. Select a campus location.
9. Verify that the corresponding 360-degree panorama is displayed.
10. Test available hotspots and navigation links.

Some VR interactions may require a connected headset or an appropriately configured XR simulation environment.

### Expected Results

The application should load campus data, display the appropriate environment, and allow the visitor to interact with the implemented navigation and hotspot features.

If an error occurs, record the issue before continuing to device deployment.

## 9. Meta Quest 3 Deployment

The Meta Quest 3 is the primary VR deployment target.

This section describes the intended build and installation procedure. The exact Unity settings must be confirmed against the team's working Quest build configuration.

### 9.1 Prepare the Headset

1. Ensure the Meta Quest 3 is sufficiently charged.
2. Enable Developer Mode through the supported Meta developer setup process.
3. Connect the headset to the development computer using a suitable USB-C cable.
4. Allow the required USB debugging authorization when prompted.
5. Confirm that the headset is recognized by the development tools.

### 9.2 Configure the Unity Build

1. Open the project in Unity.
2. Open **File → Build Profiles**.
3. Select the Android platform.
4. Install any missing Android build modules.
5. Activate the Android build profile.
6. Confirm that the required scenes are included in the build configuration.
7. Verify that OpenXR and the appropriate Quest-compatible XR settings are enabled.
8. Check the application identifier and other Android Player settings against the team's working configuration.

For a standalone Quest build, verify that the project uses a compatible graphics API, CPU architecture and XR configuration.

Do not change a working project configuration without testing the resulting build.

### 9.3 Build the Application

1. Save the current Unity project.
2. Confirm that there are no blocking compilation errors.
3. Connect the Quest headset.
4. Select the configured Android build profile.
5. Choose **Build** or **Build and Run**.
6. Select an appropriate output folder.
7. Wait for Unity to complete the build process.

The expected Android package format for a standalone Quest application is an APK.

### 9.4 Install and Launch

If Build and Run is used, Unity may install and launch the application on the connected device.

Alternatively, an APK may be installed using an appropriate Android debugging tool.

After installation, open the application on the Quest headset and verify the main menu, 360-degree environments, navigation and information hotspots.

### 9.5 Quest Deployment Status

The team has developed and demonstrated a Quest VR prototype.

However, the final build configuration, deployment commands and complete device test results should be confirmed and recorded by the teammate responsible for the latest build.

This guide does not claim that every feature has passed final hardware testing.

## 10. Android Smartphone Deployment

A separate Android smartphone version is planned.

The smartphone version may require different camera controls, user input and interface configuration compared with the Meta Quest 3 version.

Before considering it complete, the team must:

1. Confirm the intended mobile interaction design.
2. Configure the Android build for smartphone use.
3. Test touch-based navigation and interface controls.
4. Verify 360-degree media display.
5. Test information hotspots and navigation.
6. Produce and install a mobile build.
7. Record actual device testing results.

**Current status:** Planned or in development. A successfully tested smartphone build has not yet been confirmed.

## 11. Testing and Verification

Deployment testing should cover the main functions required for a usable virtual campus tour.

| Test ID | Test case | Expected result | Current verification |
|---|---|---|---|
| DEP-01 | Open Unity project | Project opens without blocking errors | To verify |
| DEP-02 | Load campus JSON | Campus data loads successfully | To verify |
| DEP-03 | Open main menu | Menu is displayed correctly | To verify |
| DEP-04 | Select campus location | Correct panorama is activated | To verify |
| DEP-05 | Display hotspot | Correct information popup appears | To verify |
| DEP-06 | Select navigation arrow | Correct destination is activated | To verify |
| DEP-07 | Change Unity scene | Requested site is restored in destination scene | To verify |
| DEP-08 | Test Quest interaction | Implemented VR controls respond correctly | To verify |
| DEP-09 | Build Quest APK | Build completes without blocking errors | To verify |
| DEP-10 | Run Quest APK | Application starts and runs on the headset | To verify |
| DEP-11 | Test Android smartphone | Mobile controls and environment function | To verified |

The test status should be updated only after the relevant test has actually been performed.

Screenshots, videos, logs or issue references may be used as supporting evidence.

## 12. Troubleshooting

### 12.1 JSON File Does Not Load

**Possible causes:**

- `campus_data.json` is missing.
- The file is not located in StreamingAssets.
- The JSON structure does not match `CampusData.cs`.
- The UnityWebRequest operation fails.

**Suggested actions:**

Check the Unity Console, confirm the file path, validate the JSON format and review the error message generated by `JsonLoader`.

### 12.2 Selected Campus Location Is Not Found

**Possible causes:**

- The requested site ID does not exist in the JSON file.
- The site ID does not match the configured `SiteArrivalHandler` entry.
- The destination scene contains an incorrect site configuration.

**Suggested actions:**

Compare the requested site ID with the corresponding `LocationData.id` and `SiteEntry.siteId`.

### 12.3 Hotspots or Navigation Arrows Do Not Update

**Possible causes:**

- The current location has not been set.
- The JSON data is still loading.
- A component is not responding to the `LocationChanged` event.
- Hotspot or link data is missing or invalid.

**Suggested actions:**

Confirm that `SceneDataLoader.CurrentLocation` contains the expected location and that the relevant components receive updated data.

### 12.4 Unity Build Fails

**Possible causes:**

- Missing Android build support.
- Incorrect build profile settings.
- Missing or incompatible packages.
- C# compilation errors.
- Incompatible XR configuration.

**Suggested actions:**

Inspect the Unity Console and build logs. Resolve errors before attempting another build.

### 12.5 Quest Headset Is Not Detected

**Possible causes:**

- USB cable does not support data transfer.
- Developer Mode is not enabled.
- USB debugging authorization has not been accepted.
- Required Android development tools are unavailable.

**Suggested actions:**

Check the cable, device authorization and Android tooling before retrying the deployment.

## 13. Version Control and Team Collaboration

Git and GitHub are used to manage project changes and coordinate team contributions.

The recommended workflow is:

1. Fetch and pull the latest repository changes.
2. Make changes in the assigned project area.
3. Test changes locally where possible.
4. Review the changed files.
5. Commit with a meaningful message.
6. Push changes to GitHub.
7. Coordinate integration and testing with the team.

Avoid committing temporary Unity-generated folders such as `Library`, `Temp`, `Obj` and local build outputs unless the team has a specific reason to track an artifact.

The repository's existing `.gitignore` configuration should be respected.

## 14. Deployment Limitations and Future Improvements

The Milestone 2 prototype is still under development.

Areas requiring further work or verification include:

- Completing end-to-end VR testing on Meta Quest 3.
- Confirming all campus locations and navigation connections.
- Testing hotspot popups and media assets.
- Verifying the final Quest build configuration.
- Completing the planned Android smartphone version.
- Confirming optional analytics and consent-related functionality.
- Improving usability, performance and stability.
- Recording deployment evidence and known issues.

These improvements will support the next development milestone.

## 15. Conclusion

This Deployment Guide documents the setup, configuration and intended deployment process for CampusXR Explorer.

The project uses Unity 6, C#, locally stored JSON campus data and interactive 360-degree environments to deliver a virtual campus experience.

The primary deployment target is Meta Quest 3, while a separate Android smartphone version remains in development.

Following a consistent deployment process helps the team reproduce builds, identify problems and prepare the application for further testing and improvement.

---

## 16. Related Documentation

- [System Architecture](../architecture/Architecture.md)
- [Technology Stack](../technology-stack/Technology-Stack.md)
- [UML Diagrams](../uml/UML.md)

These documents provide additional information about the system structure, development technologies and application behaviour.