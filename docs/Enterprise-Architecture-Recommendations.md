# BISE Hyderabad Enterprise Architecture Recommendations

## 1. Project Overview

The solution is a .NET 10 layered Blazor application composed of:

- **BiseHyderabad.Core** — entities, enums, DTOs, interfaces, and helpers.
- **BiseHyderabad.Application** — MediatR commands, queries, and FluentValidation validators.
- **BiseHyderabad.Infrastructure** — EF Core SQL Server persistence, migrations, and business services.
- **BiseHyderabad.Web** — Blazor interactive server UI, authentication, authorization, middleware, and pages.
- **Tests** — Core, Infrastructure, and Web test projects.

The current business hierarchy is:

```text
Board
 ├── District
 │    └── Tehsil
 │         └── School
 │              ├── Users
 │              ├── Enrollments
 │              ├── Invoices
 │              └── Examination records
```

Current foundations are good:

- Cookie authentication
- School, district, and SuperAdmin roles
- MFA service
- Permission grants
- Approval requests
- Security audit events
- Rate limiting
- Health checks
- Resource authorization handlers
- Activity logging
- MediatR and FluentValidation
- Enrollment, examination, certificate, invoice, challan, and reporting modules

The solution currently builds successfully. Existing discovered tests passed:

- Core: 22/22
- Infrastructure: 8/8
- Web: 2/2

The current design is a single-board, district/school-scoped system. It is not yet a complete multi-tenant platform.

---

## 2. Priority Classification

### Critical — implement before onboarding multiple organizations

1. Tenant isolation and tenant-aware data ownership.
2. Server-side authorization for every ID-based operation.
3. Mandatory MFA and stronger controls for privileged users.
4. Concurrency-safe and idempotent enrollment saving.
5. Secure private storage for student photos and generated PDFs.
6. Cross-tenant authorization and data-leakage tests.

### High — implement before production scale

1. Formal SuperAdmin governance and approval workflows.
2. Role assignment history instead of one scalar role.
3. Enrollment state machine and history.
4. Optimistic concurrency using `rowversion`.
5. Transactional enrollment-number allocation.
6. Strong invoice, enrollment, and tenant consistency constraints.
7. Structured audit logging.

### Medium — implement during operational maturity

1. Draft/resume enrollment experience.
2. Background jobs and outbox processing.
3. Centralized observability.
4. Bulk import/export workflows.
5. Operational dashboards and alerts.
6. Disaster recovery testing.

---

# 3. Multi-Tenant Architecture

## 3.1 Current gap

The current entity model does not have a `TenantId` or equivalent organization boundary. Users, districts, schools, enrollments, invoices, settings, audit records, approvals, fee rates, academic years, and sequence records can therefore be treated as global unless every service correctly applies manual filters.

Relevant areas:

- `src/BiseHyderabad.Core/Entities/UserAndTimelineEntities.cs`
- `src/BiseHyderabad.Core/Entities/SchoolAndLocationEntities.cs`
- `src/BiseHyderabad.Core/Entities/EnrollmentAndBillingEntities.cs`
- `src/BiseHyderabad.Core/Entities/SecurityEntities.cs`
- `src/BiseHyderabad.Infrastructure/Data/ApplicationDbContext.cs`
- `src/BiseHyderabad.Core/Interfaces/ICurrentUserService.cs`

## 3.2 Recommended strategy

Start with a shared database and shared schema using a mandatory tenant discriminator:

```text
Tenant
 ├── Tenant users and role assignments
 ├── Districts
 ├── Tehsils
 ├── Schools
 ├── Academic years
 ├── Fee rates
 ├── Enrollments
 ├── Invoices
 ├── Examination records
 ├── Certificates
 └── Security and audit records
```

This is the most practical approach for the existing EF Core design. It minimizes infrastructure complexity while allowing future migration to separate schemas or databases for high-isolation tenants.

## 3.3 Tenant context

Add a server-side tenant context:

```csharp
public interface ITenantContext
{
    int TenantId { get; }
    bool IsPlatformAdministrator { get; }
}
```

Extend `ICurrentUserService` with:

- `TenantId`
- `IsPlatformAdministrator`
- permitted district IDs
- permitted school IDs
- effective permissions
- impersonation state

The tenant must be resolved from trusted authentication/session context. Never trust a tenant ID submitted by a browser form, query string, or route parameter.

## 3.4 Tenant-owned entities

Add `TenantId` to all tenant-owned records, including:

- Users
- Districts
- Tehsils
- Schools
- Academic years
- Fee rates
- Window overrides
- Enrollments
- Invoices
- Invoice items, or enforce their aggregate relationship
- Invoice sequences
- Examination sessions and forms
- Results
- Certificates
- Recheck and correction requests
- Affiliation applications and inspections
- Approvals
- Activity logs
- Security audit events
- Settings
- Uploaded-file metadata

## 3.5 Query filters

Use EF Core global query filters as a secondary protection:

```csharp
modelBuilder.Entity<Enrollment>()
    .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
```

Global filters must not be the only protection. Services and authorization policies must still verify tenant ownership. Provide a carefully restricted way for platform administrators to intentionally query across tenants.

## 3.6 Tenant-aware uniqueness

Update unique indexes to include `TenantId` where values are only unique within an organization:

- Usernames
- School codes
- SEMIS codes, if tenant-specific
- Enrollment numbers
- Invoice numbers
- Certificate numbers
- Settings keys
- Payment references, if applicable
- Application numbers
- Request numbers

## 3.7 Tenant migration approach

For the current single-board database:

1. Create the `Tenant` table.
2. Insert the existing board as the initial tenant.
3. Add nullable `TenantId` columns temporarily.
4. Backfill every existing row with the initial tenant ID.
5. Validate that no rows remain without a tenant.
6. Add foreign keys and indexes.
7. Make `TenantId` non-nullable.
8. Add tenant-aware query filters and authorization tests.

Do not onboard additional tenants until the isolation tests pass.

---

# 4. SuperAdmin and Governance

## 4.1 Separate platform and tenant administration

Use two administrative levels:

```text
Platform SuperAdmin
 ├── Tenant lifecycle
 ├── Tenant activation and suspension
 ├── Platform configuration
 ├── Cross-tenant reporting
 ├── Support and controlled impersonation
 └── Emergency operations

Tenant Administrator
 ├── Districts
 ├── Schools
 ├── Users
 ├── Fee rates
 ├── Enrollment operations
 └── Examination operations
```

Do not make every SuperAdmin action automatically cross-tenant. Cross-tenant access should be an explicit capability and should be visible in the UI.

## 4.2 SuperAdmin capabilities

The SuperAdmin area should provide:

- Tenant creation and onboarding
- Tenant activation, suspension, and archival
- District and tehsil administration
- School affiliation approval
- School lifecycle management
- User and role administration
- Fee-rate versioning and publication
- Academic-year configuration
- Enrollment-window management
- Examination-session management
- Challan and payment verification
- Result correction governance
- Certificate status governance
- Audit investigation
- Health and operational monitoring
- Reports with tenant/district/school drill-down
- Controlled impersonation
- Bulk operations with approval

## 4.3 Dual control and approval requirements

Require approval or separation of duties for:

- Creating or deleting a tenant
- Assigning SuperAdmin privileges
- Disabling a SuperAdmin
- Changing published fee rates
- Reopening a closed enrollment window
- Extending deadlines
- Verifying or reversing payments
- Correcting examination results
- Changing certificate status
- Bulk enrollment changes
- Impersonating another user
- Exporting large volumes of personal data

The existing `ApprovalRequest` entity is a useful foundation, but it should include:

- `TenantId`
- Approval policy
- Required approval count
- Expiration time
- Approval history
- Rejection reason
- Idempotency key
- Version number
- Strongly typed target information where possible

A requester must not approve their own request. This behavior already has test coverage and should be preserved.

## 4.4 Role assignment history

The current `User` model has one scalar role and optional school/district IDs. This is insufficient for delegated and temporary administration.

Introduce a model similar to:

```text
UserRoleAssignment
 ├── UserId
 ├── TenantId
 ├── Role
 ├── ScopeType
 ├── ScopeId
 ├── StartsAt
 ├── EndsAt
 ├── IsActive
 ├── AssignedByUserId
 ├── RevokedByUserId
 └── Reason
```

Benefits:

- Multiple roles per user
- Temporary assignments
- District and school delegation
- Effective-dated access
- Revocation history
- Better auditability

## 4.5 Privileged session controls

For SuperAdmin and tenant administrators:

- Mandatory MFA
- Strong password policy
- Shorter session lifetime
- Re-authentication for dangerous operations
- Device/session listing
- Remote session revocation
- Login notifications
- Failed-login alerts
- Recovery-code management
- No permanent passwords displayed or entered by administrators

The user-management UI currently includes a plain password field. Replace it with an invitation and password-reset workflow. Administrators should not see existing passwords or create permanent credentials on behalf of users.

## 4.6 Controlled impersonation

If support impersonation is required:

- Require a reason
- Require approval for privileged targets
- Show a persistent impersonation banner
- Restrict sensitive functions
- Log the support user and impersonated user
- Expire the session automatically
- Prevent impersonating another SuperAdmin
- Provide an immediate stop-impersonation action

---

# 5. Authorization and Data Isolation

## 5.1 Current risk

Several service operations use raw IDs for sensitive data access. Operations such as enrollment, invoice, certificate, PDF, CNIC, and previous-record lookups must not rely solely on the ID being valid.

Every read and mutation must validate:

```text
CurrentUser.TenantId == Entity.TenantId
```

For school users:

```text
CurrentUser.SchoolId == Entity.SchoolId
```

For district users:

```text
CurrentUser.DistrictId == Entity.School.DistrictId
```

## 5.2 Service boundaries

Avoid unconstrained contracts such as:

```csharp
GetEnrollmentByIdAsync(int id)
```

Use tenant-aware service boundaries and resolve the tenant from the authenticated context:

```csharp
GetEnrollmentAsync(
    int enrollmentId,
    CancellationToken cancellationToken);
```

The implementation should obtain the tenant from `ITenantContext` and apply authorization before returning data.

## 5.3 Protect all access paths

Apply authorization to:

- List queries
- Details pages
- Edit operations
- Delete operations
- CNIC searches
- Previous-record searches
- Invoice lookup by ID
- Invoice lookup by UUID
- Challan generation
- Challan verification
- PDF downloads
- Certificate verification data
- Reports and exports
- Bulk operations
- Background jobs

A secure route attribute on a Blazor page is not sufficient. Services must enforce authorization because they are the actual data boundary.

## 5.4 Strongly typed scope relationships

`PermissionGrant` and `WindowOverride` currently use a scope type plus an untyped scope ID. Validate that:

- A school scope ID identifies a real school.
- A district scope ID identifies a real district.
- The target belongs to the current tenant.
- The user is permitted to grant or consume that scope.
- The school belongs to the selected district.

Where possible, replace polymorphic ID fields with typed relationships or separate tables.

---

# 6. Enrollment Form UX

The existing enrollment form has a polished visual style, responsive grids, document sections, floating labels, and useful field grouping. It should remain a single-page form, enhanced with clear sections and reliable save/review behavior.

## 6.1 Recommended single-page layout

### Candidate identity section

- Student name
- Father or guardian name
- Surname
- B-Form/CNIC
- Date of birth
- Gender
- Photo

### Contact and address section

- Mobile number
- Address
- Emergency contact
- Consent or declaration

### Academic information section

- Class level
- Group
- Student type
- Medium
- Previous board
- Board registration number
- Eligibility number

### Subjects section

- Display subjects valid for the selected class and group.
- Separate compulsory and elective subjects.
- Enforce minimum and maximum subject counts.
- Prevent invalid combinations.
- Show the fee impact of subject selections when applicable.

### Review and confirmation section

- Read-only summary
- Duplicate warnings
- Fee estimate
- Declaration checkbox
- Final confirmation

## 6.2 UX features

Add:

- Section navigation or a sticky table of contents, without changing pages
- Sticky save and submit actions
- Save draft
- Autosave indicator
- Last-saved timestamp
- Resume unfinished draft
- Unsaved-changes warning
- Duplicate candidate warning
- Validation summary at the top
- Inline validation near each field
- Clear required and optional labels
- Accessible keyboard navigation
- Mobile-friendly subject selection
- Final confirmation dialog
- Confirmation number after successful save
- Print/download confirmation after completion

Do not rely only on floating labels. They can be confusing with autofill, long values, and validation errors. Keep persistent labels and use floating behavior only as an enhancement.

## 6.3 Blazor component structure

Keep one route and one form while splitting the markup into focused section components:

```text
EnrollmentForm.razor
 ├── EnrollmentSectionNavigation.razor
 ├── CandidateIdentitySection.razor
 ├── CandidateContactSection.razor
 ├── AcademicDetailsSection.razor
 ├── SubjectSelectionSection.razor
 ├── EnrollmentReviewSection.razor
 └── EnrollmentSaveStatus.razor
```

Use a dedicated form model rather than binding the UI directly to the persistence entity. This prevents incomplete or invalid UI state from being treated as a valid domain object.

---

# 7. Enrollment Validation and Saving

## 7.1 Server-side validation

Client-side validation improves UX but is not a security boundary. The server must validate:

- Tenant ownership
- School ownership
- School active status
- Academic-year validity
- Enrollment-window status
- Grace-period rules
- School-allowed class levels
- Valid class/group combination
- Valid subject combination
- Duplicate candidate rules
- Fee-rate availability
- User authorization

## 7.2 Duplicate prevention

Define the business rules explicitly. Possible rules include:

```text
One normalized CNIC/B-Form + academic year + tenant
One normalized GR number + school + academic year
One enrollment number within a tenant
One invoice number within a tenant and invoice type
```

Normalize before validation and storage:

- CNIC/B-Form
- GR number
- Student names
- Phone numbers
- Email addresses
- School codes
- Usernames

Use database unique indexes to protect against race conditions. Application checks alone are not sufficient.

## 7.3 Idempotent save

Double clicks, browser retries, network retries, and reconnects must not create duplicate enrollments.

Add an idempotency key to the create request:

```csharp
public Guid IdempotencyKey { get; init; }
```

Persist the key with the operation and add a suitable unique index. If the same request is submitted again, return the original result.

## 7.4 Draft and submitted records

Separate draft editing from final submission. A draft may be incomplete, while a submitted enrollment must meet all business rules.

Suggested states:

```text
Draft
 └── Submitted
      ├── Rejected
      ├── Approved
      └── Invoiced
            └── Paid
                  └── NumberAllocated
```

Every transition should validate:

- Allowed previous state
- Authorized actor
- Window status
- Payment status
- Required approvals
- Required fields

## 7.5 Enrollment history

Create an immutable history table containing:

- Enrollment ID
- Tenant ID
- Previous status
- New status
- Changed by user ID
- Reason
- Timestamp
- Correlation ID
- Selected field changes where appropriate

This is essential for education-board operations and dispute resolution.

## 7.6 Error handling

Do not return raw exception messages to users. The enrollment command currently returns exception text directly in a failure result. Instead:

1. Log the complete exception server-side.
2. Include a correlation ID.
3. Return a safe error code and user-friendly message.
4. Show the correlation ID to support staff.

---

# 8. Billing, Challans, and Number Allocation

## 8.1 Invoice/enrollment consistency

An invoice and its invoice items must belong to the same:

- Tenant
- School
- Academic year
- Invoice business context

The current `InvoiceItem` relationship relies heavily on application logic. Add database-backed consistency where possible, or validate the complete aggregate in a transaction.

## 8.2 Concurrency-safe sequences

Enrollment-number allocation must handle two administrators processing records simultaneously.

Required controls:

- Unique index for the complete sequence scope
- Database transaction
- Atomic increment or row-level locking
- Duplicate-key retry for sequence creation
- Idempotent number allocation
- Tenant-aware sequence scope
- Allocation actor and timestamp
- No silent production fallback for missing year values

The fallback year suffix and other defaults should produce a validation error or operational alert instead of silently generating potentially incorrect numbers.

## 8.3 Payment workflow

Use explicit payment states rather than free-form strings:

```text
Pending
 ├── Submitted
 ├── Verified
 ├── Rejected
 ├── Reversed
 └── Cancelled
```

Record:

- Payment reference
- Payment method
- Amount received
- Verification actor
- Verification timestamp
- Reversal actor and reason
- Bank or provider response
- Reconciliation status
- Source document or attachment

Payment verification and reversal should be approval-controlled and idempotent.

## 8.4 Replace free-form statuses

Prefer enums or reference tables for:

- Challan status
- Invoice type
- Invoice phase
- Payment method
- Fee type
- Scope type
- Window type
- Request type
- Entity type

Free-form strings make reporting and validation unreliable.

---

# 9. Student Data and File Security

The application processes sensitive personal data including CNIC/B-Form, date of birth, address, phone numbers, photos, academic information, results, and certificates.

## 9.1 Protect sensitive fields

- Encrypt sensitive values where appropriate.
- Mask CNIC/B-Form in lists and logs.
- Never log passwords or complete personal identifiers.
- Restrict data export permissions.
- Log access to searches and bulk exports.
- Define retention and archival policies.
- Define legal deletion and correction procedures.

## 9.2 Secure student photos and PDFs

Student photos should not be served from predictable public URLs under `wwwroot/uploads`.

Use:

- Private blob or file storage
- Database metadata for files
- Server-generated file names
- Content-type and file-signature validation
- Maximum size and image-dimension validation
- Malware scanning where available
- Authorization before download
- Short-lived download URLs
- Watermarks for official documents where appropriate
- Audit logging for access and downloads

---

# 10. Audit and Compliance

Use separate audit categories:

## Security audit

- Login success and failure
- MFA enrollment and verification
- Password resets
- Account lockout
- Role changes
- Permission changes
- Impersonation
- Authorization failures
- Session revocation

## Business audit

- Enrollment changes
- Enrollment status transitions
- Fee-rate changes
- Deadline overrides
- Payment verification
- Result changes
- Certificate changes
- School affiliation decisions

## Data-access audit

- CNIC searches
- Previous-record searches
- Bulk exports
- PDF downloads
- Cross-school access
- Cross-tenant access

Audit records should include:

- Tenant ID
- Actor user ID
- Actor username snapshot
- Action
- Entity type and ID
- Before value
- After value
- Reason
- IP address
- Correlation ID
- Timestamp

Normal administrators must not be able to edit or delete audit records.

---

# 11. Database and EF Core Hardening

Add or review:

- Maximum lengths for every string column
- Explicit required/optional configuration
- Tenant-aware foreign keys
- Tenant-aware unique indexes
- Optimistic concurrency tokens
- UTC-only date handling
- Check constraints for counts and monetary values
- Check constraints for legal status combinations
- Composite school/district consistency
- Soft-delete policy where required
- Migration validation in CI/CD
- Indexes for common tenant, district, school, year, and status filters

Add a concurrency token to important aggregate roots:

```csharp
public byte[] RowVersion { get; set; } = [];
```

Use optimistic concurrency for:

- Enrollment edits
- Fee rates
- Invoice verification
- Examination results
- Certificate status
- User role changes
- School and tenant configuration

Also enforce that:

```text
School.Tehsil.DistrictId == School.DistrictId
Invoice.TenantId == Enrollment.TenantId
Invoice.SchoolId == Enrollment.SchoolId
Invoice.AcademicYearId == Enrollment.AcademicYearId
ExamCenter.TenantId == AssignedSchool.TenantId
ExamCenter.DistrictId == AssignedSchool.DistrictId
```

---

# 12. Application Architecture

The current layering is a good base. Continue moving business rules out of UI components and infrastructure-specific services into application/domain services.

Recommended structure:

```text
Web
 └── Application
      ├── Commands
      ├── Queries
      ├── Validators
      ├── Authorization policies
      └── Domain rules
           └── Infrastructure
                ├── EF Core
                ├── Email and SMS
                ├── File storage
                ├── Payments
                └── Background processing
```

Recommended additions:

- Domain service for enrollment state transitions
- Tenant-aware query services
- Central authorization policy service
- Outbox pattern for notifications and audit events
- Background jobs for reports, bulk processing, and reconciliation
- Consistent error codes
- Cancellation tokens on all database operations
- Consistent `Result<T>` usage
- Structured logging with correlation IDs
- Application-level business invariants

Avoid exposing EF entities directly from UI-facing workflows. Use command models and response DTOs.

---

# 13. SuperAdmin Dashboard

The dashboard should focus on exceptions and operational control, not only totals.

Recommended indicators:

- Active, suspended, and pending tenants
- Schools requiring approval
- Pending affiliation inspections
- Enrollment-window status
- Enrollments awaiting review
- Failed payments and reconciliation exceptions
- Pending result corrections
- Certificates requiring review
- Failed login and lockout trends
- Unusual data-access alerts
- Recent privileged actions
- Background-job failures
- Database and application health
- Tenant usage and storage

Use drill-down filters:

```text
Tenant → District → Tehsil → School → Academic Year → Module
```

Every cross-tenant report must clearly display the active tenant scope.

---

# 14. Testing Strategy

The existing tests are a useful foundation but do not yet cover the critical security and business boundaries.

## 14.1 Tenant isolation tests

Add tests proving that:

- Tenant A cannot read Tenant B enrollments.
- Tenant A cannot read Tenant B invoices.
- Tenant A cannot read Tenant B certificates.
- Tenant A cannot use Tenant B UUIDs to access records.
- Tenant A cannot download Tenant B PDFs.
- Tenant A cannot perform Tenant B CNIC searches.
- Cross-tenant report access is rejected unless explicitly authorized.

## 14.2 Authorization tests

Add tests for:

- School admin cannot change the school ID.
- District admin cannot access another district.
- Tenant admin cannot access another tenant.
- Disabled users cannot continue using an existing session.
- Role downgrade takes effect immediately.
- SuperAdmin actions require MFA.
- Scope IDs cannot reference another tenant.
- Background jobs preserve tenant context.

## 14.3 Enrollment tests

Add tests for:

- Duplicate normalized CNIC.
- Duplicate normalized GR number.
- Double submission and idempotency.
- Invalid subject combinations.
- Invalid school/class combinations.
- Closed enrollment window.
- Grace-period fee calculation.
- Concurrent enrollment-number allocation.
- Concurrent invoice verification.
- Optimistic concurrency conflicts.
- Draft resume behavior.
- Invalid state transitions.

## 14.4 Approval and audit tests

Add tests for:

- Self-approval prevention.
- Expired approvals.
- Rejected requests not being applied.
- Required approval count.
- Audit event generation for privileged actions.
- Audit immutability through normal services.
- Impersonation audit completeness.

## 14.5 Blazor UI tests

Add tests for:

- Single-page section navigation.
- Unsaved-changes warning.
- Validation summary.
- Duplicate warning.
- Resume draft.
- Mobile layout.
- Keyboard accessibility.
- Disabled save button while saving.
- Retry behavior after connection interruption.
- Confirmation display after successful save.

---

# 15. Deployment and Operations

## 15.1 Distributed deployment

If more than one application instance is deployed:

- Replace process-local memory state with distributed cache where needed.
- Use a shared data-protection key ring.
- Ensure cookie authentication works across instances.
- Use a shared file/blob store.
- Use distributed locks for sequence allocation and scheduled jobs.
- Use centralized logging and tracing.

## 15.2 Observability

Add:

- Structured logs
- Correlation IDs
- Distributed tracing
- Metrics for login, enrollment, payments, and PDF generation
- Failed authorization metrics
- Tenant-level health indicators
- Alerting for payment and background-job failures
- Slow-query monitoring
- Storage and database capacity alerts

## 15.3 Backup and disaster recovery

Document and regularly test:

- Database backups
- Point-in-time restore
- File/blob restoration
- Key-ring recovery
- Migration rollback strategy
- Recovery time objective
- Recovery point objective
- Tenant-specific data restoration procedure
- Disaster communication process

## 15.4 CI/CD

Add pipeline checks for:

- Build
- Unit tests
- Integration tests
- Tenant-isolation tests
- Database migration validation
- Dependency vulnerability scanning
- Secret scanning
- Static analysis
- Container scanning
- Accessibility checks
- Deployment smoke tests

---

# 16. Recommended Implementation Roadmap

## Phase 1 — Security and ownership foundation

1. Add the `Tenant` entity and tenant context.
2. Add `TenantId` to all tenant-owned entities.
3. Backfill the existing database with the initial tenant.
4. Add tenant-aware foreign keys and indexes.
5. Add tenant-aware EF query filters.
6. Enforce tenant ownership in services.
7. Add tenant-isolation tests.
8. Secure uploaded files and PDFs.

## Phase 2 — SuperAdmin governance

1. Separate platform and tenant administration.
2. Implement role assignments.
3. Add privileged-session controls.
4. Require MFA for privileged roles.
5. Add approval policies for high-risk actions.
6. Add detailed before/after audit events.
7. Add session revocation.
8. Implement controlled impersonation if required.

## Phase 3 — Enrollment reliability

1. Create a dedicated enrollment form model.
2. Split the single Blazor form into section components.
3. Add draft persistence and resume.
4. Add autosave and unsaved-change protection.
5. Add duplicate detection.
6. Add idempotency keys.
7. Implement explicit enrollment state transitions.
8. Add enrollment history.
9. Add row-version concurrency.

## Phase 4 — Billing and operations

1. Enforce tenant/school/year consistency between invoices and enrollments.
2. Make sequence allocation transactional and concurrency-safe.
3. Add payment reconciliation jobs.
4. Add outbox-based notifications.
5. Add operational exception dashboards.
6. Add bulk processing with progress and retry support.

## Phase 5 — Enterprise maturity

1. Add centralized logs, metrics, and tracing.
2. Add distributed cache and shared data-protection keys for multi-instance deployment.
3. Move files to private blob storage.
4. Add backup and restore testing.
5. Add disaster recovery runbooks.
6. Add security and dependency scanning.
7. Perform penetration testing.
8. Conduct an accessibility review.
9. Perform load and concurrency testing.

---

# 17. Immediate Next Actions

The recommended order of implementation is:

1. Implement tenant context and tenant-aware ownership.
2. Protect every ID-based service query and mutation.
3. Add tenant-isolation integration tests.
4. Secure SuperAdmin account and role management.
5. Add mandatory MFA and privileged re-authentication.
6. Make enrollment creation idempotent.
7. Add duplicate prevention and database constraints.
8. Make enrollment-number allocation concurrency-safe.
9. Move student photos and PDFs to private storage.
10. Refactor the enrollment form into a section-based single-page Blazor form.

Tenant isolation, privileged administration, and reliable enrollment persistence should be treated as prerequisites before expanding the system to multiple organizations or boards.
