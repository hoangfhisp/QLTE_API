# System Design and Architecture Specification

## 1. Architectural Philosophy
The QLTE backend platform is engineered using a Modular Monolith architecture governed by Domain-Driven Design (DDD) principles and Clean Architecture separation of concerns. The platform balances high decoupling between business sub-domains while maintaining unified deployment ergonomics under ASP.NET Core 8.

```mermaid
graph TD
    subgraph Presentation_Layer [Presentation and Transport Layer]
        API_GW[API Controllers / Filter Pipelines]
        SignalR_Hubs[SignalR Realtime Hubs]
    end

    subgraph Application_Layer [Application Layer]
        Orchestrators[Workflow Orchestrators]
        AppServices[Domain Coordination Services]
        Validators[Fluent / Attribute Validation]
        DTO_Mappers[DTO Mapping Layer]
    end

    subgraph Domain_Layer [Domain Core Layer]
        DomainEntities[Enterprise Entities]
        DomainServices[Domain Business Services]
        ValueObjects[Value Objects and Exceptions]
    end

    subgraph Infrastructure_Layer [Infrastructure and Persistence Layer]
        EF_Contexts[EF Core DbContexts]
        Repositories[Repository Implementations]
        IdentityStore[ASP.NET Core Identity]
        ExtServices[MailKit / Google OAuth / File Storage]
    end

    API_GW --> Application_Layer
    SignalR_Hubs --> Application_Layer
    Application_Layer --> Domain_Layer
    Application_Layer --> Infrastructure_Layer
    Infrastructure_Layer --> Domain_Layer
```

---

## 2. Service Domain Decomposition

The application divides domain responsibilities into three bounded contexts alongside a shared infrastructure library:

### 2.1 AuthService Module
- **Purpose**: Identity credential validation, token issuance, refresh token rotation, and identity lifecycle events.
- **Key Capabilities**:
  - JWT token minting and signature validation.
  - Refresh token persistence with revocation tracking.
  - Multi-factor validation and email verification via MailKit.
  - External OAuth2 provider integration (Google Identity).

### 2.2 UserService Module
- **Purpose**: User account lifecycle management, parental profile registry, and role-based access control (RBAC).
- **Key Capabilities**:
  - User profile creation, profile updates, and avatar media binding.
  - Role management (Parent, Admin, Children).
  - Integration with ASP.NET Core Identity for secure password hashing and claims management.

### 2.3 ChildService Module
- **Purpose**: Child lifecycle management, device enrollment, safety geofencing, usage restrictions, and emergency escalation.
- **Key Capabilities**:
  - Geofencing and coordinate analysis using the Haversine mathematical algorithm.
  - Device inventory management, telemetry heartbeat tracking, and remote locking controls.
  - Device restriction orchestration (study mode, application blocking, web domain firewall).
  - SOS dispatch, automated parent broadcast, and peer-to-peer signaling mediation.

### 2.4 Shared.FileStorage Module
- **Purpose**: Unified filesystem management, static asset persistence, slugification, and media integrity checks.
- **Key Capabilities**:
  - Safe file writes with extension whitelisting and path normalization.
  - Slug generation for Vietnamese Unicode strings.

---

## 3. End-to-End Workflow Sequence Diagrams

### 3.1 Realtime Location Telemetry and Geofence Boundary Check
The sequence below illustrates the lifecycle of a location ping dispatched by a child device to the server:

```mermaid
sequenceDiagram
    autonumber
    participant ChildDevice as Child Device Client
    participant LocationHub as LocationHub (SignalR)
    participant LocationService as ILocationService
    participant ZoneService as IZoneService
    participant ChildDb as SQL Server (ChildDbContext)
    participant ParentApp as Parent Device Client

    ChildDevice->>LocationHub: SendLocation(childId, lat, lng)
    activate LocationHub
    LocationHub->>ChildDb: Verify child existence and retrieve current status
    LocationHub->>ParentApp: Broadcast ReceiveChildLocation(childId, lat, lng)
    
    LocationHub->>LocationService: CreateLocationHistoryAsync(dto)
    activate LocationService
    LocationService->>ChildDb: Insert LocationHistory record
    deactivate LocationService

    LocationHub->>ZoneService: CheckSafeZoneAsync(childId, lat, lng)
    activate ZoneService
    ZoneService->>ChildDb: Query active SafeZones for childId
    ZoneService-->>ZoneService: Compute Haversine distance against all zones
    ZoneService-->>LocationHub: Evaluation Result (True: In Zone, False: Out of Zone)
    deactivate ZoneService

    alt Child has breached the boundary (Calculated Out of Zone)
        LocationHub->>ChildDb: Update Child.TrangThai = "False"
        LocationHub->>ParentApp: Broadcast ChildLeftSafeZone payload with alerts
    else Child has returned to safe boundary
        LocationHub->>ChildDb: Update Child.TrangThai = "True"
    end
    deactivate LocationHub
```

### 3.2 SOS Emergency Alarm Dispatch Workflow
The sequence below illustrates the critical path executed when an emergency SOS button is activated by a child device:

```mermaid
sequenceDiagram
    autonumber
    participant ChildDevice as Child Device Client
    participant SOSController as SOSRequestController
    participant SOSService as ISOSRequestService
    participant SOSHub as SOSHub (SignalR)
    participant Database as SQL Server
    participant ParentApp as Parent Notification Listener

    ChildDevice->>SOSController: POST /api/SOSRequest/Create (ChildId, Lat, Lng, Notes)
    activate SOSController
    SOSController->>SOSService: CreateAsync(dto)
    activate SOSService
    SOSService->>Database: Persist new SOSRequest record (Status: Active)
    SOSService-->>SOSController: Confirmation and SOS Record Metadata
    deactivate SOSService
    
    SOSController->>SOSHub: Trigger Broadcast to Group(ChildId) / Parents
    activate SOSHub
    SOSHub->>ParentApp: Emit ReceiveSOS(sosId, childId, coordinates, timestamp)
    deactivate SOSHub
    
    SOSController-->>ChildDevice: HTTP 200 OK (SOS Dispatched)
    deactivate SOSController
```

---

## 4. Database Context Separation Architecture

The platform isolates persistence concerns across three logical SQL Server database instances or connection profiles:

1. **AuthDb**: Houses identity security artifacts, authentication logs, and refresh token states.
2. **UserDb**: Stores user account structures, credential metadata, roles, and administrative assignments.
3. **ChildDb**: Contains high-frequency transactional data including `LocationHistory`, `SafeZones`, `DangerZones`, `DeviceInfo`, `DeviceRestrictions`, `ChatMessages`, and `SOSRequests`.

Separation ensures that operational database load from location polling and chat does not impact core user authentication throughput.
