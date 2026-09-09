IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [AcademicYears] (
        [Id] int NOT NULL IDENTITY,
        [YearName] nvarchar(max) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [IsEnrollmentOpen] bit NOT NULL,
        [EnrollmentOpenStart] datetime2 NULL,
        [EnrollmentOpenEnd] datetime2 NULL,
        [EnrollmentGraceEnd] datetime2 NULL,
        [IsEnrollmentGraceEnabled] bit NOT NULL,
        [EnrollmentLateFeeType] nvarchar(max) NOT NULL,
        [EnrollmentLateFeeAmount] decimal(18,2) NOT NULL,
        [IsExamOpen] bit NOT NULL,
        [ExamOpenStart] datetime2 NULL,
        [ExamOpenEnd] datetime2 NULL,
        [ExamGraceEnd] datetime2 NULL,
        [IsExamGraceEnabled] bit NOT NULL,
        [ExamLateFeeType] nvarchar(max) NOT NULL,
        [ExamLateFeeAmount] decimal(18,2) NOT NULL,
        [ExamTimetableAnnounced] bit NOT NULL,
        [ResultsDeclared] bit NOT NULL,
        [PromotionDone] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AcademicYears] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] int NOT NULL IDENTITY,
        [LogName] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [SubjectType] nvarchar(max) NULL,
        [SubjectId] int NULL,
        [UserId] int NULL,
        [CausedByUsername] nvarchar(max) NULL,
        [PropertiesJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Districts] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [ShortCode] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Districts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [InvoiceSequences] (
        [Id] int NOT NULL IDENTITY,
        [SequenceType] nvarchar(max) NOT NULL,
        [AcademicYearId] int NULL,
        [SchoolId] int NULL,
        [DistrictCode] nvarchar(max) NULL,
        [Zone] int NULL,
        [GroupCode] nvarchar(max) NULL,
        [Prefix] nvarchar(max) NULL,
        [LastNumber] int NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InvoiceSequences] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Settings] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [WindowOverrides] (
        [Id] int NOT NULL IDENTITY,
        [ScopeType] nvarchar(max) NOT NULL,
        [ScopeId] int NOT NULL,
        [WindowType] nvarchar(max) NOT NULL,
        [NormalStart] datetime2 NULL,
        [NormalEnd] datetime2 NULL,
        [GraceEnd] datetime2 NULL,
        [IsGraceEnabled] bit NOT NULL,
        [LateFeeType] nvarchar(max) NOT NULL,
        [LateFeeAmount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WindowOverrides] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [ExamSchedules] (
        [Id] int NOT NULL IDENTITY,
        [AcademicYearId] int NOT NULL,
        [ClassLevel] nvarchar(max) NOT NULL,
        [Group] nvarchar(max) NOT NULL,
        [SubjectName] nvarchar(max) NOT NULL,
        [ExamDate] datetime2 NOT NULL,
        [Session] nvarchar(max) NOT NULL,
        [PaperDurationMinutes] int NOT NULL,
        [MaxMarks] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ExamSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExamSchedules_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [FeeRates] (
        [Id] int NOT NULL IDENTITY,
        [AcademicYearId] int NOT NULL,
        [FeeType] nvarchar(max) NOT NULL,
        [ClassLevel] nvarchar(max) NOT NULL,
        [GroupName] nvarchar(max) NOT NULL,
        [StudentType] nvarchar(max) NOT NULL,
        [FeeSlab] nvarchar(max) NOT NULL,
        [StandardFee] decimal(18,2) NOT NULL,
        [LateFee] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FeeRates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FeeRates_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [ExamCenters] (
        [Id] int NOT NULL IDENTITY,
        [CenterCode] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [DistrictId] int NOT NULL,
        [SuperintendentName] nvarchar(max) NULL,
        [SuperintendentPhone] nvarchar(max) NULL,
        [Capacity] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ExamCenters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExamCenters_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Tehsils] (
        [Id] int NOT NULL IDENTITY,
        [DistrictId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Tehsils] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tehsils_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Schools] (
        [Id] int NOT NULL IDENTITY,
        [SemisCode] nvarchar(450) NOT NULL,
        [Code] nvarchar(max) NULL,
        [Name] nvarchar(max) NOT NULL,
        [DistrictId] int NOT NULL,
        [TehsilId] int NULL,
        [Type] int NOT NULL,
        [Zone] int NOT NULL,
        [Address] nvarchar(max) NULL,
        [ContactNumber] nvarchar(max) NULL,
        [HeadName] nvarchar(max) NULL,
        [HeadPhone] nvarchar(max) NULL,
        [HeadCnic] nvarchar(max) NULL,
        [HeadEmail] nvarchar(max) NULL,
        [AllowedLevelsJson] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Schools] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Schools_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Schools_Tehsils_TehsilId] FOREIGN KEY ([TehsilId]) REFERENCES [Tehsils] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [ExamCenterSchools] (
        [Id] int NOT NULL IDENTITY,
        [ExamCenterId] int NOT NULL,
        [SchoolId] int NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ExamCenterSchools] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExamCenterSchools_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExamCenterSchools_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] int NOT NULL IDENTITY,
        [Uuid] uniqueidentifier NOT NULL,
        [SchoolId] int NOT NULL,
        [AcademicYearId] int NOT NULL,
        [InvoiceType] nvarchar(max) NOT NULL,
        [InvoiceNumber] nvarchar(450) NOT NULL,
        [ClassGroupStudentType] nvarchar(max) NOT NULL,
        [StudentCount] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [Phase] nvarchar(max) NOT NULL,
        [RejectionReason] nvarchar(max) NULL,
        [GeneratedAt] datetime2 NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [CancelledAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [SchoolSpecialPermissions] (
        [Id] int NOT NULL IDENTITY,
        [SchoolId] int NOT NULL,
        [PermissionType] nvarchar(max) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [ExpiryDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SchoolSpecialPermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchoolSpecialPermissions_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(450) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Role] int NOT NULL,
        [SchoolId] int NULL,
        [DistrictId] int NULL,
        [IsActive] bit NOT NULL,
        [MustChangePassword] bit NOT NULL,
        [FailedLoginAttempts] int NOT NULL,
        [LockoutEnd] datetime2 NULL,
        [LastLoginAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Users_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] int NOT NULL IDENTITY,
        [Uuid] uniqueidentifier NOT NULL,
        [SchoolId] int NOT NULL,
        [AcademicYearId] int NOT NULL,
        [StudentName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NULL,
        [FatherCnic] nvarchar(max) NULL,
        [Surname] nvarchar(max) NULL,
        [GrNumber] nvarchar(max) NULL,
        [IdentificationMark] nvarchar(max) NULL,
        [Cnic] nvarchar(450) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [DateOfBirthWords] nvarchar(max) NULL,
        [Gender] nvarchar(max) NOT NULL,
        [Medium] nvarchar(max) NOT NULL,
        [Religion] nvarchar(max) NOT NULL,
        [Nationality] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NULL,
        [MobileNumber] nvarchar(max) NULL,
        [PhotoPath] nvarchar(max) NULL,
        [ClassLevel] nvarchar(max) NOT NULL,
        [Group] nvarchar(max) NOT NULL,
        [StudentType] nvarchar(max) NOT NULL,
        [SubjectsJson] nvarchar(max) NOT NULL,
        [AdmissionDate] datetime2 NULL,
        [BoardRegNo] nvarchar(max) NULL,
        [EligibilityNo] nvarchar(max) NULL,
        [PreviousBoard] nvarchar(max) NULL,
        [BoardPassingDate] datetime2 NULL,
        [EnrollmentNumber] nvarchar(450) NULL,
        [EnrollmentNumberAllottedAt] datetime2 NULL,
        [InvoiceId] int NULL,
        [ChallanStatus] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Enrollments_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [ExaminationForms] (
        [Id] int NOT NULL IDENTITY,
        [Uuid] uniqueidentifier NOT NULL,
        [EnrollmentId] int NOT NULL,
        [SchoolId] int NOT NULL,
        [AcademicYearId] int NOT NULL,
        [ClassLevel] nvarchar(max) NOT NULL,
        [Group] nvarchar(max) NOT NULL,
        [StudentType] nvarchar(max) NOT NULL,
        [PreviousSeatNumber] nvarchar(max) NULL,
        [PreviousYear] nvarchar(max) NULL,
        [RollNumber] nvarchar(max) NULL,
        [ExamCenterId] int NULL,
        [SubjectsJson] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ExaminationForms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExaminationForms_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExaminationForms_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExaminationForms_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ExaminationForms_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [InvoiceItems] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [EnrollmentId] int NOT NULL,
        [IsIncluded] bit NOT NULL,
        [FeeAmount] decimal(18,2) NOT NULL,
        [InvoiceTypeSnapshot] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InvoiceItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceItems_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InvoiceItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Results] (
        [Id] int NOT NULL IDENTITY,
        [ExaminationFormId] int NOT NULL,
        [MarksJson] nvarchar(max) NOT NULL,
        [TotalObtained] int NOT NULL,
        [TotalMaxMarks] int NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [Grade] nvarchar(max) NOT NULL,
        [IsPassed] bit NOT NULL,
        [Remarks] nvarchar(max) NULL,
        [DeclaredAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Results] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Results_ExaminationForms_ExaminationFormId] FOREIGN KEY ([ExaminationFormId]) REFERENCES [ExaminationForms] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE TABLE [Certificates] (
        [Id] int NOT NULL IDENTITY,
        [Uuid] uniqueidentifier NOT NULL,
        [ResultId] int NOT NULL,
        [CertificateNumber] nvarchar(450) NOT NULL,
        [VerificationToken] nvarchar(450) NOT NULL,
        [StudentName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [EnrollmentNumber] nvarchar(max) NOT NULL,
        [RollNumber] nvarchar(max) NOT NULL,
        [SchoolName] nvarchar(max) NOT NULL,
        [ClassLevel] nvarchar(max) NOT NULL,
        [Group] nvarchar(max) NOT NULL,
        [Grade] nvarchar(max) NOT NULL,
        [TotalMarksObtained] int NOT NULL,
        [TotalMaxMarks] int NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [IsRevoked] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Certificates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Certificates_Results_ResultId] FOREIGN KEY ([ResultId]) REFERENCES [Results] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificates_CertificateNumber] ON [Certificates] ([CertificateNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificates_ResultId] ON [Certificates] ([ResultId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certificates_VerificationToken] ON [Certificates] ([VerificationToken]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrollments_AcademicYearId] ON [Enrollments] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Enrollments_EnrollmentNumber] ON [Enrollments] ([EnrollmentNumber]) WHERE [EnrollmentNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrollments_InvoiceId] ON [Enrollments] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrollments_SchoolId_AcademicYearId_Cnic] ON [Enrollments] ([SchoolId], [AcademicYearId], [Cnic]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExamCenters_DistrictId] ON [ExamCenters] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExamCenterSchools_ExamCenterId] ON [ExamCenterSchools] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExamCenterSchools_SchoolId] ON [ExamCenterSchools] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExaminationForms_AcademicYearId] ON [ExaminationForms] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExaminationForms_EnrollmentId] ON [ExaminationForms] ([EnrollmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExaminationForms_ExamCenterId] ON [ExaminationForms] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExaminationForms_SchoolId] ON [ExaminationForms] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_AcademicYearId] ON [ExamSchedules] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FeeRates_AcademicYearId] ON [FeeRates] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InvoiceItems_EnrollmentId] ON [InvoiceItems] ([EnrollmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InvoiceItems_InvoiceId] ON [InvoiceItems] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_AcademicYearId] ON [Invoices] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_SchoolId] ON [Invoices] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Results_ExaminationFormId] ON [Results] ([ExaminationFormId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Schools_DistrictId] ON [Schools] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Schools_SemisCode] ON [Schools] ([SemisCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Schools_TehsilId] ON [Schools] ([TehsilId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SchoolSpecialPermissions_SchoolId] ON [SchoolSpecialPermissions] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tehsils_DistrictId] ON [Tehsils] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_DistrictId] ON [Users] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_SchoolId] ON [Users] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260819110521_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260819110521_InitialCreate', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    DROP INDEX [IX_Invoices_SchoolId] ON [Invoices];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    ALTER TABLE [Invoices] ADD [PaymentMethod] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    ALTER TABLE [Invoices] ADD [PaymentReceivedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    ALTER TABLE [Invoices] ADD [PaymentReference] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    ALTER TABLE [Invoices] ADD [PaymentVerifiedByUsername] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Invoices_PaymentReference] ON [Invoices] ([PaymentReference]) WHERE [PaymentReference] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    CREATE INDEX [IX_Invoices_SchoolId_AcademicYearId_Status] ON [Invoices] ([SchoolId], [AcademicYearId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820161246_AddInvoicePaymentReconciliation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820161246_AddInvoicePaymentReconciliation', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    ALTER TABLE [Users] ADD [MfaEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    ALTER TABLE [Users] ADD [MfaEnrolledAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    ALTER TABLE [Users] ADD [MfaLastVerifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    ALTER TABLE [Users] ADD [MfaRecoveryCodesHash] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    ALTER TABLE [Users] ADD [MfaSecret] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    CREATE TABLE [ApprovalRequests] (
        [Id] bigint NOT NULL IDENTITY,
        [RequestType] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [SubmittedByUserId] int NOT NULL,
        [SubmittedByUsername] nvarchar(100) NOT NULL,
        [ReviewedByUserId] int NULL,
        [ReviewedByUsername] nvarchar(100) NULL,
        [ReviewComment] nvarchar(max) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [ReviewedAt] datetime2 NULL,
        CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    CREATE TABLE [PermissionGrants] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Permission] nvarchar(100) NOT NULL,
        [ScopeType] int NOT NULL,
        [ScopeId] int NULL,
        [IsAllowed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NULL,
        CONSTRAINT [PK_PermissionGrants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PermissionGrants_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    CREATE TABLE [SecurityAuditEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [EventType] nvarchar(100) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(450) NULL,
        [EntityId] nvarchar(450) NULL,
        [UserId] int NULL,
        [Username] nvarchar(100) NULL,
        [IpAddress] nvarchar(64) NULL,
        [CorrelationId] nvarchar(100) NULL,
        [DetailsJson] nvarchar(max) NULL,
        [OccurredAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SecurityAuditEvents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    CREATE INDEX [IX_ApprovalRequests_EntityType_EntityId_Status] ON [ApprovalRequests] ([EntityType], [EntityId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PermissionGrants_UserId_Permission_ScopeType_ScopeId] ON [PermissionGrants] ([UserId], [Permission], [ScopeType], [ScopeId]) WHERE [ScopeId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditEvents_EntityType_EntityId] ON [SecurityAuditEvents] ([EntityType], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditEvents_OccurredAt_EventType] ON [SecurityAuditEvents] ([OccurredAt], [EventType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823165343_Phase1SecurityGovernance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823165343_Phase1SecurityGovernance', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [WindowOverrides] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD [TenantId1] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Tehsils] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Settings] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [SecurityAuditEvents] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [SchoolSpecialPermissions] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Schools] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Schools] ADD [TenantId1] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Results] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [PermissionGrants] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [InvoiceSequences] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Invoices] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [InvoiceItems] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [FeeRates] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExamSchedules] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExaminationForms] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExamCenterSchools] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExamCenters] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Enrollments] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Districts] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Districts] ADD [TenantId1] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Certificates] ADD [RevocationReason] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Certificates] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Certificates] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ApprovalRequests] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ActivityLogs] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [AcademicYears] ADD [TenantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [Tenants] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Code', N'IsActive', N'CreatedAt', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Tenants]'))
        SET IDENTITY_INSERT [Tenants] ON;
    EXEC(N'INSERT INTO [Tenants] ([Id], [Name], [Code], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (1, N''BISE Hyderabad'', N''BISE-HYD'', CAST(1 AS bit), ''2026-09-09T17:01:35.0459454Z'', ''2026-09-09T17:01:35.0459468Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Code', N'IsActive', N'CreatedAt', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Tenants]'))
        SET IDENTITY_INSERT [Tenants] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [AcademicYears] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [ActivityLogs] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [ApprovalRequests] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Certificates] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Districts] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Enrollments] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [ExamCenters] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [ExamCenterSchools] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [ExamSchedules] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [ExaminationForms] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [FeeRates] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [InvoiceItems] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [InvoiceSequences] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Invoices] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [PermissionGrants] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Results] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Schools] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [SchoolSpecialPermissions] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [SecurityAuditEvents] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Settings] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Tehsils] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [Users] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    UPDATE [WindowOverrides] SET [TenantId] = 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [CertificateStatusHistories] (
        [Id] bigint NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [CertificateId] int NOT NULL,
        [FromStatus] int NOT NULL,
        [ToStatus] int NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [ChangedByUserId] int NOT NULL,
        [ChangedByUsername] nvarchar(100) NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CertificateStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificateStatusHistories_Certificates_CertificateId] FOREIGN KEY ([CertificateId]) REFERENCES [Certificates] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CertificateStatusHistories_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [ExaminationSessions] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [AcademicYearId] int NOT NULL,
        [ExaminationType] nvarchar(max) NOT NULL,
        [ClassLevel] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [RegistrationOpenAt] datetime2 NOT NULL,
        [RegistrationCloseAt] datetime2 NOT NULL,
        [ExaminationStartAt] datetime2 NULL,
        [ExaminationEndAt] datetime2 NULL,
        [ResultDeclarationAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ExaminationSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExaminationSessions_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExaminationSessions_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [ResultCorrectionRequests] (
        [Id] bigint NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ResultId] int NOT NULL,
        [RequestNumber] nvarchar(50) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [ProposedMarksJson] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [SubmittedByUserId] int NOT NULL,
        [SubmittedByUsername] nvarchar(max) NOT NULL,
        [ReviewedByUserId] int NULL,
        [ReviewedByUsername] nvarchar(max) NULL,
        [ReviewComment] nvarchar(max) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [ReviewedAt] datetime2 NULL,
        [AppliedAt] datetime2 NULL,
        CONSTRAINT [PK_ResultCorrectionRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResultCorrectionRequests_Results_ResultId] FOREIGN KEY ([ResultId]) REFERENCES [Results] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResultCorrectionRequests_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [ResultRecheckRequests] (
        [Id] bigint NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ResultId] int NOT NULL,
        [RequestNumber] nvarchar(50) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [SubmittedByUserId] int NOT NULL,
        [SubmittedByUsername] nvarchar(max) NOT NULL,
        [ReviewedByUserId] int NULL,
        [ReviewedByUsername] nvarchar(max) NULL,
        [ReviewComment] nvarchar(max) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [ReviewedAt] datetime2 NULL,
        CONSTRAINT [PK_ResultRecheckRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResultRecheckRequests_Results_ResultId] FOREIGN KEY ([ResultId]) REFERENCES [Results] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResultRecheckRequests_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [SchoolAffiliationApplications] (
        [Id] bigint NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [SchoolId] int NOT NULL,
        [ApplicationNumber] nvarchar(50) NOT NULL,
        [RequestedCategory] nvarchar(max) NOT NULL,
        [RequestedLevelsJson] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [SubmittedAt] datetime2 NULL,
        [ReviewedByUserId] int NULL,
        [ReviewedByUsername] nvarchar(max) NULL,
        [ReviewComment] nvarchar(max) NULL,
        [ReviewedAt] datetime2 NULL,
        [ValidUntil] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SchoolAffiliationApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchoolAffiliationApplications_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SchoolAffiliationApplications_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [SubjectSchemes] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ExaminationSessionId] int NOT NULL,
        [ClassLevel] nvarchar(max) NOT NULL,
        [Group] nvarchar(450) NOT NULL,
        [SubjectCode] nvarchar(50) NOT NULL,
        [SubjectName] nvarchar(200) NOT NULL,
        [MaxMarks] int NOT NULL,
        [PassingMarks] int NOT NULL,
        [HasPractical] bit NOT NULL,
        [PracticalMaxMarks] int NOT NULL,
        [IsCompulsory] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_SubjectSchemes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubjectSchemes_ExaminationSessions_ExaminationSessionId] FOREIGN KEY ([ExaminationSessionId]) REFERENCES [ExaminationSessions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SubjectSchemes_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE TABLE [SchoolInspections] (
        [Id] bigint NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [AffiliationApplicationId] bigint NOT NULL,
        [InspectedAt] datetime2 NOT NULL,
        [InspectorUserId] int NOT NULL,
        [InspectorUsername] nvarchar(100) NOT NULL,
        [Findings] nvarchar(max) NOT NULL,
        [Capacity] int NULL,
        [Passed] bit NOT NULL,
        [AttachmentPath] nvarchar(max) NULL,
        CONSTRAINT [PK_SchoolInspections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchoolInspections_SchoolAffiliationApplications_AffiliationApplicationId] FOREIGN KEY ([AffiliationApplicationId]) REFERENCES [SchoolAffiliationApplications] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SchoolInspections_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_WindowOverrides_TenantId] ON [WindowOverrides] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Users_TenantId] ON [Users] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Users_TenantId1] ON [Users] ([TenantId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Tehsils_TenantId] ON [Tehsils] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Settings_TenantId] ON [Settings] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditEvents_TenantId] ON [SecurityAuditEvents] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SchoolSpecialPermissions_TenantId] ON [SchoolSpecialPermissions] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Schools_TenantId] ON [Schools] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Schools_TenantId1] ON [Schools] ([TenantId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Results_TenantId] ON [Results] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_PermissionGrants_TenantId] ON [PermissionGrants] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_InvoiceSequences_TenantId] ON [InvoiceSequences] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Invoices_TenantId] ON [Invoices] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_InvoiceItems_TenantId] ON [InvoiceItems] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_FeeRates_TenantId] ON [FeeRates] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_TenantId] ON [ExamSchedules] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ExaminationForms_TenantId] ON [ExaminationForms] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ExamCenterSchools_TenantId] ON [ExamCenterSchools] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ExamCenters_TenantId] ON [ExamCenters] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Enrollments_TenantId] ON [Enrollments] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Districts_TenantId] ON [Districts] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Districts_TenantId1] ON [Districts] ([TenantId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_Certificates_TenantId] ON [Certificates] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ApprovalRequests_TenantId] ON [ApprovalRequests] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_TenantId] ON [ActivityLogs] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_AcademicYears_TenantId] ON [AcademicYears] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_CertificateStatusHistories_CertificateId_ChangedAt] ON [CertificateStatusHistories] ([CertificateId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_CertificateStatusHistories_TenantId] ON [CertificateStatusHistories] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ExaminationSessions_AcademicYearId_ClassLevel_Status] ON [ExaminationSessions] ([AcademicYearId], [ClassLevel], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExaminationSessions_Code] ON [ExaminationSessions] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ExaminationSessions_TenantId] ON [ExaminationSessions] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ResultCorrectionRequests_RequestNumber] ON [ResultCorrectionRequests] ([RequestNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ResultCorrectionRequests_ResultId_Status] ON [ResultCorrectionRequests] ([ResultId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ResultCorrectionRequests_TenantId] ON [ResultCorrectionRequests] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ResultRecheckRequests_RequestNumber] ON [ResultRecheckRequests] ([RequestNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ResultRecheckRequests_ResultId_Status] ON [ResultRecheckRequests] ([ResultId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_ResultRecheckRequests_TenantId] ON [ResultRecheckRequests] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SchoolAffiliationApplications_ApplicationNumber] ON [SchoolAffiliationApplications] ([ApplicationNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SchoolAffiliationApplications_SchoolId_Status] ON [SchoolAffiliationApplications] ([SchoolId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SchoolAffiliationApplications_TenantId] ON [SchoolAffiliationApplications] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SchoolInspections_AffiliationApplicationId_InspectedAt] ON [SchoolInspections] ([AffiliationApplicationId], [InspectedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SchoolInspections_TenantId] ON [SchoolInspections] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubjectSchemes_ExaminationSessionId_SubjectCode_Group] ON [SubjectSchemes] ([ExaminationSessionId], [SubjectCode], [Group]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE INDEX [IX_SubjectSchemes_TenantId] ON [SubjectSchemes] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tenants_Code] ON [Tenants] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [AcademicYears] ADD CONSTRAINT [FK_AcademicYears_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ActivityLogs] ADD CONSTRAINT [FK_ActivityLogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ApprovalRequests] ADD CONSTRAINT [FK_ApprovalRequests_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Certificates] ADD CONSTRAINT [FK_Certificates_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Districts] ADD CONSTRAINT [FK_Districts_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Districts] ADD CONSTRAINT [FK_Districts_Tenants_TenantId1] FOREIGN KEY ([TenantId1]) REFERENCES [Tenants] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Enrollments] ADD CONSTRAINT [FK_Enrollments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExamCenters] ADD CONSTRAINT [FK_ExamCenters_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExamCenterSchools] ADD CONSTRAINT [FK_ExamCenterSchools_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExaminationForms] ADD CONSTRAINT [FK_ExaminationForms_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [ExamSchedules] ADD CONSTRAINT [FK_ExamSchedules_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [FeeRates] ADD CONSTRAINT [FK_FeeRates_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [InvoiceItems] ADD CONSTRAINT [FK_InvoiceItems_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [InvoiceSequences] ADD CONSTRAINT [FK_InvoiceSequences_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [PermissionGrants] ADD CONSTRAINT [FK_PermissionGrants_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Results] ADD CONSTRAINT [FK_Results_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Schools] ADD CONSTRAINT [FK_Schools_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Schools] ADD CONSTRAINT [FK_Schools_Tenants_TenantId1] FOREIGN KEY ([TenantId1]) REFERENCES [Tenants] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [SchoolSpecialPermissions] ADD CONSTRAINT [FK_SchoolSpecialPermissions_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [SecurityAuditEvents] ADD CONSTRAINT [FK_SecurityAuditEvents_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Settings] ADD CONSTRAINT [FK_Settings_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Tehsils] ADD CONSTRAINT [FK_Tehsils_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Tenants_TenantId1] FOREIGN KEY ([TenantId1]) REFERENCES [Tenants] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    ALTER TABLE [WindowOverrides] ADD CONSTRAINT [FK_WindowOverrides_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823174128_AddMultiTenantFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823174128_AddMultiTenantFoundation', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    ALTER TABLE [Districts] DROP CONSTRAINT [FK_Districts_Tenants_TenantId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    ALTER TABLE [Schools] DROP CONSTRAINT [FK_Schools_Tenants_TenantId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Tenants_TenantId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    DROP INDEX [IX_Users_TenantId1] ON [Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    DROP INDEX [IX_Schools_TenantId1] ON [Schools];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    DROP INDEX [IX_Districts_TenantId1] ON [Districts];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'TenantId1');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Users] DROP COLUMN [TenantId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Schools]') AND [c].[name] = N'TenantId1');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Schools] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Schools] DROP COLUMN [TenantId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Districts]') AND [c].[name] = N'TenantId1');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Districts] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Districts] DROP COLUMN [TenantId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909122811_AlignCurrentModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260909122811_AlignCurrentModel', N'10.0.11');
END;

COMMIT;
GO

