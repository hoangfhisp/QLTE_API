# RESTful API Specification

## 1. Authentication and Identity (`/api/Auth`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| POST | `/api/Auth/LoginUserToken` | Public | Authenticates credentials and returns JWT Bearer with Refresh Token |
| POST | `/api/Auth/ExternalProviderLoginUserToken` | Public | Authenticates external identity token (Google OAuth2) |
| POST | `/api/Auth/RefreshToken` | Public | Exchanges active Refresh Token for new JWT access token |
| POST | `/api/Auth/RevokeToken` | Authenticated | Explicitly revokes designated refresh token |
| POST | `/api/Auth/UserLogOut` | Authenticated | Terminates active user session and invalidates tokens |
| POST | `/api/Auth/ForgotPassword` | Public | Initiates password reset procedure and dispatches OTP email |
| POST | `/api/Auth/VerifyForgotPassword` | Public | Validates OTP code for password reset |
| POST | `/api/Auth/ResetPassword` | Public | Applies updated password with valid verification token |
| POST | `/api/Auth/ChangeEmail` | Authenticated | Dispatches confirmation link for updating email address |
| POST | `/api/Auth/VerifyChangeEmail` | Authenticated | Confirms email modification with verification token |
| GET | `/api/Auth/GetAll` | Admin | Retrieves administrative authentication audit logs |

---

## 2. User Management (`/api/User`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| POST | `/api/User/RegisterUser` | Public | Enrolls new user account into the system |
| PUT | `/api/User/UpdateInfoUser` | Authenticated | Modifies profile details, full name, or avatar file |
| POST | `/api/User/LoginLocalUser` | Public | Internal local credential validation |
| POST | `/api/User/ProviderLoginOrRegisterUser` | Public | Enrolls or logs in OAuth2 provider accounts |
| GET | `/api/User/GetUserByEmail` | Authenticated | Fetches user profile by registered email |
| GET | `/api/User/GetUserByUserId` | Authenticated | Fetches profile by GUID user identifier |
| POST | `/api/User/ResetPassword` | Authenticated | In-app password update requiring old password |
| POST | `/api/User/ChangeEmail` | Authenticated | Modifies account email |
| GET | `/api/User/GetListUser` | Admin | Paginated collection of registered users |
| GET | `/api/User/GetUserByTenND` | Authenticated | Search user profiles by username keyword |
| DELETE | `/api/User/DeleteUser` | Admin | Deactivates or removes a user record |

---

## 3. Role Management (`/api/Role`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| POST | `/api/Role/Create` | Admin | Adds custom enterprise role entry |
| GET | `/api/Role/GetListRole` | Admin | Returns all system roles (Parent, Admin, Children) |
| GET | `/api/Role/GetRoleByRoleName` | Admin | Retrieves specific role schema by identity name |

---

## 4. Child Registry and Profiles (`/api/Child`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/Child/GetAllParentsWithChildren` | Admin | Returns comprehensive hierarchical mapping of parents and children |
| GET | `/api/Child/GetChildrenByUserId` | Parent, Admin | Returns child profiles associated with a parent ID |
| POST | `/api/Child/CreateChild` | Parent, Admin | Registers new child profile and generates tracking key |
| PUT | `/api/Child/UpdateChild` | Parent, Admin | Updates child profile information and birth dates |
| DELETE | `/api/Child/DeleteChild` | Parent, Admin | Deletes child profile and associated tracking history |
| GET | `/api/Child/GetChildByUserId` | Parent, Admin | Returns child details corresponding to linked user |

---

## 5. Geofencing Zones (`/api/SafeZone` & `/api/DangerZone`)

### 5.1 Safe Zones (`/api/SafeZone`)
| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/SafeZone/GetSafeZoneByUserId` | Parent, Admin | Returns all safe zones defined by parent |
| GET | `/api/SafeZone/GetSafeZoneByUserIdAndChildId` | Parent, Admin | Safe zones assigned to specific child |
| POST | `/api/SafeZone/CreateSafeZone` | Parent, Admin | Adds safe zone (coordinates, radius in meters) |
| PUT | `/api/SafeZone/UpdateSafeZone` | Parent, Admin | Updates safe zone boundaries and titles |
| DELETE | `/api/SafeZone/DeleteSafeZone` | Parent, Admin | Removes specific safe zone boundary |

### 5.2 Danger Zones (`/api/DangerZone`)
| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/DangerZone/GetDangerZoneByUserId` | Parent, Admin | Returns all danger zones configured by parent |
| GET | `/api/DangerZone/GetDangerZoneByUserIdAndChildId` | Parent, Admin | Danger zones assigned to specific child |
| POST | `/api/DangerZone/CreateDangerZone` | Parent, Admin | Defines hazardous zone with alert triggers |
| PUT | `/api/DangerZone/UpdateDangerZone` | Parent, Admin | Adjusts danger zone radius or coordinates |
| DELETE | `/api/DangerZone/DeleteDangerZone` | Parent, Admin | Deletes danger zone definition |

---

## 6. Device Telemetry and Control (`/api/DeviceInfo`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/DeviceInfo/GetAll` | Parent, Admin, Children | Lists registered hardware devices |
| GET | `/api/DeviceInfo/Detail/{deviceId}` | Parent, Admin, Children | Queries technical specs and battery level |
| GET | `/api/DeviceInfo/GetByChild` | Parent, Admin, Children | Returns hardware bound to child ID |
| POST | `/api/DeviceInfo/Create` | Parent, Admin | Registers new device serial/IMEI |
| PUT | `/api/DeviceInfo/Update` | Parent, Admin | Updates device identifiers |
| DELETE | `/api/DeviceInfo/Delete` | Parent, Admin | Unbinds device from child profile |
| PUT | `/api/DeviceInfo/UpdateStatus` | Public (Agent) | Device telemetry update (battery, online state) |
| PUT | `/api/DeviceInfo/LockDevice/{childId}` | Parent, Admin | Triggers immediate remote OS lock |
| PUT | `/api/DeviceInfo/UnlockDevice/{childId}`| Parent, Admin | Sends release signal to unlock OS |
| PUT | `/api/DeviceInfo/SetTracking/{childId}` | Parent, Admin | Toggles high-frequency GPS tracking |

---

## 7. Device Restrictions and Firewall Policies (`/api/DeviceRestriction`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/DeviceRestriction/GetAllByDevice` | Parent, Admin | Returns active restrictions for device |
| GET | `/api/DeviceRestriction/GetDetail` | Parent, Admin | Reads policy details by restriction ID |
| POST | `/api/DeviceRestriction/Create` | Parent, Admin | Adds restriction rules (app/domain blacklist) |
| PUT | `/api/DeviceRestriction/Update` | Parent, Admin | Updates usage limits or schedules |
| DELETE | `/api/DeviceRestriction/Delete` | Parent, Admin | Removes restriction rule |
| PATCH | `/api/DeviceRestriction/ToggleFirewall` | Parent, Admin | Enables/disables VPN firewall filtering |
| POST | `/api/DeviceRestriction/ActivateStudyRestriction` | Parent, Admin | Locks device into distraction-free study mode |
| POST | `/api/DeviceRestriction/DeactivateRestriction` | Parent, Admin | Resumes normal device operation |

---

## 8. Emergency Alarms and History (`/api/SOSRequest` & `/api/Location`)

### 8.1 SOS Requests (`/api/SOSRequest`)
| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/SOSRequest/GetAll` | Admin | Complete audit history of SOS triggers |
| GET | `/api/SOSRequest/GetByChild` | Parent, Admin | SOS history for specific child |
| GET | `/api/SOSRequest/Detail/{sosId}` | Parent, Admin | Full details and status of an emergency event |
| POST | `/api/SOSRequest/Create` | Parent, Children, Admin | Triggers distress dispatch and broadcast |
| PUT | `/api/SOSRequest/Update` | Parent, Admin | Resolves or annotates emergency record |
| DELETE | `/api/SOSRequest/Delete` | Admin | Cleans up obsolete test records |

### 8.2 Location History (`/api/Location`)
| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/Location/GetLocationHistoryNew` | Parent, Admin | Queries chronological geodetic track points |

---

## 9. Schedules and Study Periods (`/api/Schedule`, `/api/StudyPeriod`, `/api/ExamSchedule`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/Schedule/GetAllByChild` | Parent, Admin | Timetable records for child |
| POST | `/api/Schedule/Create` | Parent, Admin | Adds regular recurring timetable item |
| GET | `/api/StudyPeriod/GetAllByChild` | Parent, Admin | Retrieves defined study intervals |
| PATCH | `/api/StudyPeriod/ToggleActive` | Parent, Admin | Activates/deactivates specific study period |
| GET | `/api/ExamSchedule/Upcoming` | Parent, Admin | Upcoming exam schedules requiring preparation |

---

## 10. Chat and Interaction History (`/api/Message`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| GET | `/api/Message/History` | Authenticated | Returns paginated private message transcript |
| GET | `/api/Message/GetLastMessagePerUser`| Authenticated | Returns recent conversations inbox list |
