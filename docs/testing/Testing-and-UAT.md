# CampusXR Explorer – Testing and User Acceptance Testing (UAT)

**Project:** CampusXR Explorer  
**Module:** PRJ381  
**Milestone:** Milestone 2  
**Development Platform:** Unity 6  
**Target Device:** Meta Quest 3  
**Testing Status:** Functional testing completed and passed, as confirmed by the development team

---

## 1. Introduction

CampusXR Explorer is an interactive virtual reality campus tour developed to help prospective students and visitors explore Belgium Campus iTversity through 360-degree panoramic environments.

Testing was conducted to check that the application functions correctly, displays campus locations, supports navigation and provides interactive information about campus facilities.

The development team confirmed that the implemented prototype features were tested and are working.

## 2. Testing Objectives

The objectives of testing were to:

- Confirm that the application launches successfully.
- Verify that 360-degree campus environments display correctly.
- Check navigation between connected campus locations.
- Verify that information hotspots can be opened.
- Confirm that hotspot images, titles and descriptions display.
- Test the buttons used to enter linked locations.
- Check that the application interface responds to user interaction.
- Identify any remaining improvements before final delivery.

## 3. Testing Environment

| Component | Description |
|---|---|
| Application | CampusXR Explorer |
| Development engine | Unity 6 |
| Programming language | C# |
| VR target device | Meta Quest 3 |
| XR framework | OpenXR and XR Interaction Toolkit |
| Campus content | 360-degree panoramic images |
| Location data | Local JSON configuration |
| Testing approach | Functional testing and development-team acceptance testing |

The current testing report focuses on the VR prototype. A separate Android smartphone build is outside the confirmed scope of these test results.

## 4. Functional Test Cases

The development team reported that the implemented application features were tested successfully.

| Test ID | Feature | Expected Result | Reported Result | Status |
|---|---|---|---|---|
| FT-01 | Application startup | Application launches and displays the interface | Application launches successfully | Passed |
| FT-02 | Main menu | Main menu appears and accepts interaction | Menu operates correctly | Passed |
| FT-03 | 360-degree environment | Campus panorama displays correctly | Panoramic environment displays | Passed |
| FT-04 | Viewing controls | Visitor can look around the environment | Viewing works correctly | Passed |
| FT-05 | Campus navigation | Navigation control opens the intended location | Navigation works | Passed |
| FT-06 | Navigation arrows | Arrows appear and respond to selection | Arrows work | Passed |
| FT-07 | Information hotspots | Selecting a hotspot opens information | Hotspot opens successfully | Passed |
| FT-08 | Hotspot title | Correct location title appears | Title displays correctly | Passed |
| FT-09 | Hotspot image | Relevant location image appears | Image displays correctly | Passed |
| FT-10 | Hotspot description | Relevant description appears | Description displays correctly | Passed |
| FT-11 | Hotspot close button | Information popup closes | Popup closes successfully | Passed |
| FT-12 | Enter-location button | Button opens the linked location | Linked location opens | Passed |
| FT-13 | Campus map | Map displays and allows location selection | Team reports map works | Passed |
| FT-14 | Indoor environments | Indoor campus panoramas display | Indoor environments display | Passed |
| FT-15 | Outdoor environments | Outdoor campus panoramas display | Outdoor environments display | Passed |
| FT-16 | Location data | Application uses configured location information | Location content loads | Passed |

**Result:** 16 functional test cases reported as passed by the development team.

## 5. Tested Campus Environments

The prototype demonstration provides visual evidence of several campus areas, including:

- Campus entrance and exterior spaces.
- Parking and outdoor campus areas.
- Reception and lobby.
- Corridors and classroom entrances.
- Computer laboratories.
- Alpha Classroom.
- Gamma Classroom.
- Delta Classroom area.
- Epsilon / Auditorium.
- Student recreation and social area.

These environments demonstrate the use of 360-degree imagery, interactive navigation and campus information.

## 6. Interactive Hotspot Testing

The application includes information hotspots that allow visitors to learn more about campus facilities.

The following hotspot behaviour was included in the team's functional testing:

1. The visitor selects an information icon.
2. The application displays an information popup.
3. The popup presents the location name, image and description.
4. Where available, the visitor selects the button to enter the linked location.
5. The application navigates to the selected environment.
6. The visitor can close the popup and continue exploring.

The demonstration includes examples for Alpha Classroom, Gamma Classroom and Epsilon / Auditorium.

**Reported outcome:** Hotspot interactions function correctly.

## 7. Navigation Testing

Navigation testing focused on allowing visitors to move through connected campus locations.

The application uses location links configured in the project data, with navigation controls displayed in the virtual environment.

Testing checked that:

- Navigation arrows are displayed.
- Users can select navigation controls.
- Connected campus locations can be opened.
- The correct panoramic environment is displayed after navigation.
- Visitors can continue exploring after moving to another location.

**Reported outcome:** Navigation features passed functional testing.

## 8. User Acceptance Testing (UAT)

### 8.1 Purpose

User Acceptance Testing evaluates whether the application meets its intended purpose from the visitor's perspective.

For Milestone 2, acceptance testing was carried out by the development team using the prototype's intended visitor workflows.

### 8.2 Acceptance Criteria

| ID | Acceptance Criterion | Team-Reported Result | Status |
|---|---|---|---|
| UAT-01 | Visitor can launch the campus tour | Successful | Accepted |
| UAT-02 | Visitor can view campus environments | Successful | Accepted |
| UAT-03 | Visitor can explore 360-degree scenes | Successful | Accepted |
| UAT-04 | Visitor can navigate between locations | Successful | Accepted |
| UAT-05 | Visitor can access facility information | Successful | Accepted |
| UAT-06 | Visitor can view hotspot images and descriptions | Successful | Accepted |
| UAT-07 | Visitor can enter linked campus locations | Successful | Accepted |
| UAT-08 | Visitor can use the campus map | Successful | Accepted |

### 8.3 UAT Outcome

Based on the development team's testing, the implemented CampusXR Explorer prototype meets the listed functional acceptance criteria for Milestone 2.

This is **development-team acceptance testing**. Independent end-user acceptance testing may be conducted as part of later evaluation.

## 9. Testing Evidence

Evidence of the working prototype includes the development team's VR demonstration recording and screenshots showing:

- 360-degree campus environments.
- Indoor and outdoor locations.
- Navigation arrows.
- Information hotspot icons.
- Alpha Classroom information popup.
- Gamma Classroom information popup.
- Epsilon / Auditorium information popup.

The demonstration recording can be used as supporting evidence during the Milestone 2 presentation.

## 10. Limitations and Further Testing

Although the implemented features passed the team's functional tests, further quality assurance remains useful.

Recommended additional activities include:

- Independent usability testing with prospective students.
- Longer VR sessions to assess comfort and usability.
- Performance testing across different campus scenes.
- Checking navigation consistency after future content updates.
- Testing a separate Android smartphone build when available.
- Regression testing after additional features or fixes.
- Confirming analytics behaviour and user consent where applicable.

These activities are recommended improvements and do not invalidate the functional testing completed for the current prototype.

## 11. Conclusion

The CampusXR Explorer development team has completed functional testing of the implemented VR prototype and reports that its main features operate successfully.

The demonstrated application allows visitors to explore panoramic campus environments, navigate between connected locations and interact with information hotspots.

Development-team acceptance testing indicates that the prototype meets its core functional objectives for PRJ381 Milestone 2. Further usability, performance and platform-specific testing can support future development.

---

**Overall Functional Testing Result:** Passed – development-team reported  
**Development-Team Acceptance Result:** Accepted  
**Project Stage:** Milestone 2 VR Prototype