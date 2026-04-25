# Reporte de Pruebas - TicketsSystem

**Fecha:** 25/04/2026
**Total de pruebas:** 63 (53 unitarias + 10 de integración)
**Estado:** 63 PASARON, 0 fallaron, 0 ignorados
**Duración:** Unitarias: 174 ms | Integración: 14 s

---

## Proyectos de Prueba

### 1. TicketsSystem.Tests (53 pruebas unitarias - Pasaron todas)
Librerías: xUnit, Moq, FluentAssertions, AutoFixture

### 2. TicketsSystem.Tests.Integration (10 pruebas de integración - Pasaron todas)
Librerías: xUnit, FluentAssertions, Testcontainers.MsSql, Microsoft.AspNetCore.Mvc.Testing, SignalR.Client
Infraestructura: SQL Server 2022 real en contenedor Docker, WebApplicationFactory<Program> in-memory

---

## Pruebas Unitarias (TicketsSystem.Tests)

### 1.1 NotificationServiceTests (4 pruebas)
Archivo: `TicketsSystem.Tests/Services/NotificationServiceTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 1 | `GetUserNotificationsAsync_ReturnsBadRequest_WhenUserIdIsEmpty` | Retorna `BadRequestError` cuando el userId es GUID vacío | ✅ PASA |
| 2 | `GetUserNotificationsAsync_ReturnsNotFound_WhenUserDoesNotExist` | Retorna `NotFoundError` si el usuario no existe; nunca consulta el repositorio de notificaciones | ✅ PASA |
| 3 | `CreateANotificationAsync_PersistsAndBroadcasts_WhenTypeIsNewTicket` | Crea notificación, guarda cambios y emite evento SignalR `NotifyTicketCreated` cuando el tipo es `NewTicket` | ✅ PASA |
| 4 | `ToggleNotificationReadStatusAsync_UpdatesAndSaves_WhenNotificationExists` | Cambia `IsRead` de `false` a `true`, actualiza la entidad y guarda | ✅ PASA |

---

### 1.2 UserServiceTests (8 pruebas)
Archivo: `TicketsSystem.Tests/Services/UserServiceTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 5 | `LoginAsync_ReturnsUnauthorized_WhenUserDoesNotExist` | Retorna `UnauthorizedError` si el email no está registrado | ✅ PASA |
| 6 | `LoginAsync_ReturnsForbidden_WhenUserIsDeactivated` | Retorna `ForbiddenError` si el usuario está desactivado (`IsActive = false`) | ✅ PASA |
| 7 | `LoginAsync_ReturnsUnauthorized_WhenPasswordIsInvalid` | Retorna `UnauthorizedError` si la contraseña no coincide con el hash | ✅ PASA |
| 8 | `LoginAsync_ReturnsJwtTokenWithClaims_WhenCredentialsAreValid` | Genera un JWT válido con claims `sub`, `email` y `role` | ✅ PASA |
| 9 | `DeactivateOrActivateAUserAsync_ReturnsBadRequest_WhenUserIdIsEmpty` | Retorna `BadRequestError` cuando el userId es GUID vacío | ✅ PASA |
| 10 | `DeactivateOrActivateAUserAsync_ReturnsBadRequest_WhenTryingToToggleSelf` | Retorna `BadRequestError` si un admin intenta desactivarse a sí mismo; no persiste | ✅ PASA |
| 11 | `DeactivateOrActivateAUserAsync_TogglesStatusAndSaves_WhenValidUser` | Cambia `IsActive` de `true` a `false`, actualiza y guarda | ✅ PASA |
| 12 | `DeactivateOrActivateAUserAsync_ThrowsFormatException_WhenUserIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID es inválido | ✅ PASA |

---

### 1.3 TicketsServiceTests (29 pruebas)
Archivo: `TicketsSystem.Tests/Services/TicketsServiceTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 13 | `GetAllTicketsWithFiltersAsync_ReturnsForbidden_ForUserWithoutSelfFilters` | Retorna `ForbiddenError` si un usuario `"User"` intenta ver tickets sin filtros `CurrentUserOnly` o `AssignedToMeOnly` | ✅ PASA |
| 14 | `CreateATicketAsync_CreatesTicketHistoryAndNotification_WhenTicketCanBeLoaded` | Crea ticket con estado `Open`, registra historial "Ticket Created", guarda y envía notificación | ✅ PASA |
| 15 | `UpdateATicketInfoAsync_ReturnsBadRequest_WhenTicketIdIsEmpty` | Retorna `BadRequestError` cuando el ticketId es GUID vacío | ✅ PASA |
| 16 | `UpdateATicketInfoAsync_ReturnsBadRequest_WhenAgentIsNotAssignedToTicket` | Retorna `BadRequestError` si un agente intenta modificar un ticket que no le pertenece | ✅ PASA |
| 17 | `UpdateATicketInfoAsync_ReturnsNotFound_WhenAssignedAgentDoesNotExist` | Retorna `NotFoundError` si el `AssignedToUserId` no existe en la BD | ✅ PASA |
| 18 | `AssingTicketAsync_ReturnsForbidden_WhenTargetUserIsNotAgent` | Retorna `ForbiddenError` si el usuario asignado no tiene rol de agente; nunca consulta el ticket | ✅ PASA |
| 19 | `AssingTicketAsync_AssignsTicketAndPersists_WhenInputIsValid` | Asigna ticket al agente, actualiza, registra cambios con `TrackChanges` y guarda | ✅ PASA |
| 20 | `CloseTicketsAsync_ReturnsBadRequest_WhenTicketIdIsEmpty` | Retorna `BadRequestError` cuando el ticketId es GUID vacío | ✅ PASA |
| 21 | `CloseTicketsAsync_ReturnsBadRequest_WhenTicketIsAlreadyClosed` | Retorna `BadRequestError` si el ticket ya está cerrado; no actualiza | ✅ PASA |
| 22 | `CloseTicketsAsync_ClosesTicketAndCreatesNotification_WhenTicketIsOpen` | Cambia estado a `Closed`, asigna `ClosedAt`, actualiza, registra cambios y envía notificación `UpdateTicket` al creador | ✅ PASA |
| 23 | `ReopenTicketsAsync_ReturnsBadRequest_WhenTicketIsNotClosed` | Retorna `BadRequestError` si el ticket no está cerrado | ✅ PASA |
| 24 | `ReopenTicketsAsync_ReopensTicketAndCreatesNotification_WhenTicketIsClosed` | Cambia estado a `Reopened`, limpia `ClosedAt`, actualiza, registra cambios y envía notificación al creador | ✅ PASA |
| 25 | `UpdateTicketUser_ReturnsBadRequest_WhenCurrentUserIsNotTicketOwner` | Retorna `BadRequestError` si el usuario actual no es el dueño del ticket | ✅ PASA |
| 26 | `UpdateTicketUser_ReturnsForbidden_WhenUserAttemptsToChangeStatusOrAssignment` | Retorna `ForbiddenError` si el usuario propietario intenta cambiar `StatusId` o `AssignedToUserId` | ✅ PASA |
| 27 | `UpdateTicketUser_UpdatesAllowedFields_WhenOwnerSendsValidChanges` | Permite al propietario cambiar `Title`, `Description` y `PriorityId`; registra cambios y guarda | ✅ PASA |
| 28 | `UpdateATicketInfoAsync_CreatesNotification_WhenStatusChanges` | Admin actualiza ticket cambiando el estado; registra cambios y envía notificación `UpdateTicket` | ✅ PASA |
| 29 | `UpdateATicketInfoAsync_DoesNotCreateNotification_WhenStatusDoesNotChange` | Admin actualiza ticket sin cambiar estado; no se envía notificación | ✅ PASA |
| 30 | `AbandonATicketAsync_ReturnsBadRequest_WhenTicketIdIsEmpty` | Retorna `BadRequestError` cuando el ticketId es GUID vacío | ✅ PASA |
| 31 | `AbandonATicketAsync_ReturnsNotFound_WhenTicketDoesNotExist` | Retorna `NotFoundError` si el ticket no existe | ✅ PASA |
| 32 | `AbandonATicketAsync_UnassignsTicketAndPersists_WhenTicketExists` | Desasigna el agente (`AssignedToUserId = null`), actualiza, registra cambios y guarda | ✅ PASA |
| 33 | `AcceptTickets_AssignsCurrentUser_WhenInputIsValid` | Agente se auto-asigna al ticket cuando el input es válido | ✅ PASA |
| 34 | `GetCurrentUserTicketsCountAsync_MapsRepositorySummary_ToDto` | Mapea correctamente el resumen del repositorio a `TicketsCountDto` (suma total correcta) | ✅ PASA |
| 35 | `GetTodaysTicketsCountAsync_ReturnsCount_FromRepository` | Retorna el contador de tickets de hoy desde el repositorio | ✅ PASA |
| 36 | `AssingTicketAsync_ThrowsFormatException_WhenUserIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID de usuario es inválido | ✅ PASA |
| 37 | `AssingTicketAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID de ticket es inválido | ✅ PASA |
| 38 | `UpdateATicketInfoAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID de ticket es inválido | ✅ PASA |
| 39 | `CloseTicketsAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID de ticket es inválido | ✅ PASA |
| 40 | `ReopenTicketsAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID de ticket es inválido | ✅ PASA |
| 41 | `GetTicketByIdAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid` | Lanza `FormatException` cuando el GUID de ticket es inválido | ✅ PASA |

---

### 1.4 GetUserRoleServiceTests (3 pruebas)
Archivo: `TicketsSystem.Tests/Services/GetUserRoleServiceTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 42 | `UserIsAdmin_ReturnsTrue_WhenUserRoleIsAdmin` | Retorna `true` cuando el usuario tiene rol `"Admin"` | ✅ PASA |
| 43 | `UserIsAgent_ReturnsFalse_WhenUserDoesNotExist` | Retorna `false` si el usuario no existe en el repositorio | ✅ PASA |
| 44 | `UserIsUser_ReturnsFalse_WhenRoleIsDifferent` | Retorna `false` si el usuario tiene un rol diferente a `"User"` | ✅ PASA |

---

### 1.5 CurrentUserServiceTests (6 pruebas)
Archivo: `TicketsSystem.Tests/Services/CurrentUserServiceTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 45 | `GetCurrentUserId_ReturnsNameIdentifier_WhenPresent` | Lee el userId desde `ClaimTypes.NameIdentifier` | ✅ PASA |
| 46 | `GetCurrentUserId_ReturnsSubClaim_WhenNameIdentifierIsMissing` | Lee el userId desde `"sub"` como fallback | ✅ PASA |
| 47 | `GetCurrentUserId_ThrowsUnauthorized_WhenNoValidClaimExists` | Lanza `UnauthorizedAccessException` si el claim no contiene un GUID válido | ✅ PASA |
| 48 | `GetCurrentUserEmail_ReturnsEmailClaim` | Retorna el email desde `ClaimTypes.Email` | ✅ PASA |
| 49 | `GetCurrentUserRole_ReturnsRoleClaim` | Retorna el rol desde `ClaimTypes.Role` | ✅ PASA |
| 50 | `GetCurrentUserName_ReturnsUserFullName_FromRepository` | Busca el usuario en BD por userId y retorna su `FullName` | ✅ PASA |

---

### 1.6 TicketsCreateValidatorTests (3 pruebas)
Archivo: `TicketsSystem.Tests/Validations/TicketsCreateValidatorTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 51 | `Validate_ReturnsSuccess_WhenDtoIsValid` | DTO válido pasa la validación | ✅ PASA |
| 52 | `Validate_ReturnsError_WhenTitleIsEmpty` | Retorna error de validación si `Title` está vacío | ✅ PASA |
| 53 | `Validate_ReturnsError_WhenPriorityIsOutOfRange` | Retorna error de validación si `PriorityId` está fuera del rango permitido | ✅ PASA |

---

## Pruebas de Integración (TicketsSystem.Tests.Integration)

**Infraestructura:** Las pruebas usan `Testcontainers.MsSql` para levantar un contenedor real de SQL Server 2022, ejecutar migraciones de EF Core y sembrar datos semilla (3 usuarios: Admin, Agent, EndUser con IDs fijos y roles). Usan `WebApplicationFactory<Program>` para alojar la API en memoria con la conexión real a la BD. La colección xUnit `IntegrationTestCollection` comparte el contenedor entre todas las pruebas.

**Datos semilla:**
| Usuario | ID GUID | Email | Rol |
|---------|---------|-------|-----|
| Admin | `11111111-1111-1111-1111-111111111111` | admin.integration@test.com | Admin |
| Agent | `22222222-2222-2222-2222-222222222222` | agent.integration@test.com | Agent |
| EndUser | `33333333-3333-3333-3333-333333333333` | user.integration@test.com | User |

Todos comparten la contraseña: `IntegrationPass123!`

### 2.1 AuthenticationIntegrationTests (5 pruebas)
Archivo: `TicketsSystem.Tests.Integration/Integration/AuthenticationIntegrationTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 54 | `Login_ReturnsJwt_And_CanAccessProtectedEndpoint` | Inicia sesión como EndUser, extrae JWT de cookie, accede a `/api/authentication/getcurrentuser` con Bearer token y verifica userId, email y role en la respuesta JSON | ✅ PASA |
| 55 | `ProtectedEndpoint_ReturnsUnauthorized_WithoutToken` | Llama a `/api/tickets/gettickets` sin autenticación y espera 401 Unauthorized | ✅ PASA |
| 56 | `CreateTicket_PersistsTicketInDatabase` | Crea un ticket via API, luego consulta directamente la BD para verificar que el ticket se persistió correctamente con los valores esperados | ✅ PASA |
| 57 | `AgentAcceptAndCloseTicket_CreatesHistoryAndNotification` | Flujo completo: EndUser crea ticket → Agent lo acepta → Agent lo cierra. Verifica en BD: asignación, estado `Closed`, `ClosedAt` no nulo, entradas de historial, y que el EndUser recibe notificación `UpdateTicket` con el título del ticket | ✅ PASA |
| 58 | `Login_ReturnsUnauthorized_WithInvalidCredentials` | Envía credenciales incorrectas y espera 401 Unauthorized | ✅ PASA |

### 2.2 SignalRIntegrationTests (5 pruebas)
Archivo: `TicketsSystem.Tests.Integration/Integration/SignalRIntegrationTests.cs`

| # | Prueba | Descripción | Resultado |
|---|--------|-------------|-----------|
| 59 | `AgentReceives_ReceiveNewTicket_WhenUserCreatesTicket` | Agent conectado al hub SignalR via LongPolling; EndUser crea un ticket; el agente recibe evento `ReceiveNewTicket` con el título y el creador correctos | ✅ PASA |
| 60 | `UserReceives_ReceiveNewTicketStatusChange_WhenAgentClosesTicket` | Usuario conectado al hub; agente cierra el ticket; el usuario recibe evento `ReceiveNewTicketStatusChange` con estado `Closed` (StatusId=4) | ✅ PASA |
| 61 | `ExternalComment_ByAgent_NotifiesUserOnly` | Agente agrega comentario externo a un ticket; el usuario recibe el evento `ReceiveNewTicketComment`; el agente NO recibe el evento | ✅ PASA |
| 62 | `ExternalComment_ByUser_NotifiesAgentOnly` | Usuario agrega comentario externo; el agente recibe el evento; el usuario NO recibe su propio evento | ✅ PASA |
| 63 | `InternalComment_ByAgent_DoesNotNotifyUser` | Agente agrega comentario interno (`isInternal: true`); ni el usuario ni el agente reciben el evento (solo visible para otros agentes) | ✅ PASA |

---

## Resumen por Clase de Prueba

| Clase | Tipo | Pruebas | Pasaron |
|-------|------|---------|---------|
| `NotificationServiceTests` | Unitaria | 4 | 4 (100%) |
| `UserServiceTests` | Unitaria | 8 | 8 (100%) |
| `TicketsServiceTests` | Unitaria | 29 | 29 (100%) |
| `GetUserRoleServiceTests` | Unitaria | 3 | 3 (100%) |
| `CurrentUserServiceTests` | Unitaria | 6 | 6 (100%) |
| `TicketsCreateValidatorTests` | Unitaria | 3 | 3 (100%) |
| **Subtotal Unitarias** | | **53** | **53 (100%)** |
| `AuthenticationIntegrationTests` | Integración | 5 | 5 (100%) |
| `SignalRIntegrationTests` | Integración | 5 | 5 (100%) |
| **Subtotal Integración** | | **10** | **10 (100%)** |
| **Total General** | | **63** | **63 (100%)** |

---

## Tiempos de Ejecución

- **Pruebas unitarias:** ~174 ms (todas en memoria con mocks)
- **Pruebas de integración:** ~14 s (incluye spin-up de contenedor SQL Server + migraciones + seed data)
- **Total:** ~14.2 s

---

## Análisis de Cobertura de Funcionalidades

| Funcionalidad | Cubierta por pruebas unitarias | Cubierta por pruebas de integración |
|--------------|-------------------------------|-------------------------------------|
| Autenticación (login JWT) | 4 pruebas | 3 pruebas |
| CRUD de Tickets | 17 pruebas | 2 pruebas |
| Asignación/Aceptación/Abandono de Tickets | 5 pruebas | 1 prueba |
| Cierre/Reapertura de Tickets | 4 pruebas | 1 prueba |
| Notificaciones | 4 pruebas | 1 prueba |
| Historial de tickets | (incluido en TicketsServiceTests) | 1 prueba |
| Validación de DTOs | 3 pruebas | - |
| Roles de usuario | 3 pruebas | - |
| CurrentUser (claims) | 6 pruebas | - |
| SignalR en tiempo real | - | 5 pruebas |
| Persistencia en BD | - | 3 pruebas |
| Endpoints protegidos (401) | - | 1 prueba |

**Conclusión:** Las 63 pruebas pasan correctamente. La cobertura es sólida tanto a nivel unitario (con mocks para lógica de negocio y validación de errores/casos límite) como a nivel de integración (contra SQL Server real + SignalR real, probando flujos completos de principio a fin).
