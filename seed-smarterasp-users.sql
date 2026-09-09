-- ============================================================================
-- BISE Sukkur - SmarterASP.NET Seed Script
-- Run this script in the SmarterASP.NET MS SQL Query Tool to activate all demo accounts
-- Password for all accounts: Admin@12345
-- ============================================================================

-- 1. Ensure Tenant exists
IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Code] = 'BISE-SUK')
BEGIN
    IF EXISTS (SELECT 1 FROM [Tenants] WHERE [Code] = 'BISE-HYD')
    BEGIN
        UPDATE [Tenants] SET [Name] = 'BISE Sukkur', [Code] = 'BISE-SUK' WHERE [Code] = 'BISE-HYD';
    END
    ELSE
    BEGIN
        INSERT INTO [Tenants] ([Name], [Code], [IsActive], [CreatedAt], [UpdatedAt])
        VALUES ('BISE Sukkur', 'BISE-SUK', 1, GETUTCDATE(), GETUTCDATE());
    END
END

DECLARE @TenantId INT = (SELECT TOP 1 [Id] FROM [Tenants] WHERE [Code] = 'BISE-SUK');
IF @TenantId IS NULL SET @TenantId = 1;

-- 2. Ensure Districts exist
IF NOT EXISTS (SELECT 1 FROM [Districts] WHERE [Name] = 'Sukkur')
BEGIN
    INSERT INTO [Districts] ([TenantId], [Name], [ShortCode], [CreatedAt])
    VALUES (@TenantId, 'Sukkur', 'SK', GETUTCDATE());
    
    DECLARE @SukDistId INT = SCOPE_IDENTITY();
    INSERT INTO [Tehsils] ([DistrictId], [Name]) VALUES
    (@SukDistId, 'Sukkur City'), (@SukDistId, 'New Sukkur'), (@SukDistId, 'Rohri'), (@SukDistId, 'Pano Akil'), (@SukDistId, 'Salehpat');
END

IF NOT EXISTS (SELECT 1 FROM [Districts] WHERE [Name] = 'Khairpur')
BEGIN
    INSERT INTO [Districts] ([TenantId], [Name], [ShortCode], [CreatedAt])
    VALUES (@TenantId, 'Khairpur', 'KP', GETUTCDATE());
    
    DECLARE @KpDistId INT = SCOPE_IDENTITY();
    INSERT INTO [Tehsils] ([DistrictId], [Name]) VALUES
    (@KpDistId, 'Khairpur'), (@KpDistId, 'Kot Diji'), (@KpDistId, 'Kingri'), (@KpDistId, 'Sobhodero'), (@KpDistId, 'Gambat'), (@KpDistId, 'Thari Mirwah'), (@KpDistId, 'Faiz Ganj'), (@KpDistId, 'Nara');
END

IF NOT EXISTS (SELECT 1 FROM [Districts] WHERE [Name] = 'Ghotki')
BEGIN
    INSERT INTO [Districts] ([TenantId], [Name], [ShortCode], [CreatedAt])
    VALUES (@TenantId, 'Ghotki', 'GH', GETUTCDATE());
    
    DECLARE @GhDistId INT = SCOPE_IDENTITY();
    INSERT INTO [Tehsils] ([DistrictId], [Name]) VALUES
    (@GhDistId, 'Ghotki'), (@GhDistId, 'Mirpur Mathelo'), (@GhDistId, 'Daharki'), (@GhDistId, 'Ubauro'), (@GhDistId, 'Khangarh');
END

DECLARE @SukDist INT = (SELECT TOP 1 [Id] FROM [Districts] WHERE [Name] = 'Sukkur');
DECLARE @SukTeh INT = (SELECT TOP 1 [Id] FROM [Tehsils] WHERE [DistrictId] = @SukDist);

-- 3. Ensure Schools exist
IF NOT EXISTS (SELECT 1 FROM [Schools] WHERE [Code] = '001')
BEGIN
    INSERT INTO [Schools] ([TenantId], [SemisCode], [Code], [Name], [DistrictId], [TehsilId], [Type], [Zone], [Address], [ContactNumber], [HeadName], [HeadPhone], [HeadEmail], [AllowedLevelsJson], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, '418010045', '001', 'Govt Comprehensive High School Sukkur', @SukDist, @SukTeh, 0, 1, 'Minara Road, Sukkur', '071-9310123', 'Mohammad Ali Qureshi', '0300-1234567', 'head.ghs@bisesukkur.edu.pk', '["SSC-I","SSC-II","HSC-I","HSC-II"]', 1, GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM [Schools] WHERE [Code] = '002')
BEGIN
    INSERT INTO [Schools] ([TenantId], [SemisCode], [Code], [Name], [DistrictId], [TehsilId], [Type], [Zone], [Address], [ContactNumber], [HeadName], [HeadPhone], [HeadEmail], [AllowedLevelsJson], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, '418010089', '002', 'Public School Sukkur', @SukDist, @SukTeh, 1, 1, 'Military Road, Sukkur', '071-5631234', 'Jawad Ahmed Shah', '0333-9876543', 'admin.publicschool@bisesukkur.edu.pk', '["SSC-I","SSC-II"]', 1, GETUTCDATE(), GETUTCDATE());
END

DECLARE @School1Id INT = (SELECT TOP 1 [Id] FROM [Schools] WHERE [Code] = '001');
DECLARE @School2Id INT = (SELECT TOP 1 [Id] FROM [Schools] WHERE [Code] = '002');

-- BCrypt hash for "Admin@12345"
DECLARE @Hash NVARCHAR(MAX) = '$2a$11$gHpNvMWiixl2oaRbWe69/e02Ocy0MKlyviVAqo7sP8JUGbufNyyQy';

-- 4. Seed / Reset Users with password "Admin@12345"

-- superadmin (Role 0)
IF EXISTS (SELECT 1 FROM [Users] WHERE LOWER([Username]) = 'superadmin')
BEGIN
    UPDATE [Users]
    SET [PasswordHash] = @Hash,
        [IsActive] = 1,
        [MustChangePassword] = 0,
        [FailedLoginAttempts] = 0,
        [LockoutEnd] = NULL,
        [Role] = 0,
        [UpdatedAt] = GETUTCDATE()
    WHERE LOWER([Username]) = 'superadmin';
END
ELSE
BEGIN
    INSERT INTO [Users] ([TenantId], [Username], [PasswordHash], [Name], [Email], [Role], [SchoolId], [DistrictId], [IsActive], [MustChangePassword], [FailedLoginAttempts], [LockoutEnd], [MfaEnabled], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, 'superadmin', @Hash, 'Board Super Administrator', 'superadmin@bisesukkur.edu.pk', 0, NULL, NULL, 1, 0, 0, NULL, 0, GETUTCDATE(), GETUTCDATE());
END

-- districtadmin (Role 1)
IF EXISTS (SELECT 1 FROM [Users] WHERE LOWER([Username]) = 'districtadmin')
BEGIN
    UPDATE [Users]
    SET [PasswordHash] = @Hash,
        [IsActive] = 1,
        [MustChangePassword] = 0,
        [FailedLoginAttempts] = 0,
        [LockoutEnd] = NULL,
        [Role] = 1,
        [DistrictId] = @SukDist,
        [UpdatedAt] = GETUTCDATE()
    WHERE LOWER([Username]) = 'districtadmin';
END
ELSE
BEGIN
    INSERT INTO [Users] ([TenantId], [Username], [PasswordHash], [Name], [Email], [Role], [SchoolId], [DistrictId], [IsActive], [MustChangePassword], [FailedLoginAttempts], [LockoutEnd], [MfaEnabled], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, 'districtadmin', @Hash, 'Sukkur District Monitor', 'sukkur.monitor@bisesukkur.edu.pk', 1, NULL, @SukDist, 1, 0, 0, NULL, 0, GETUTCDATE(), GETUTCDATE());
END

-- schooladmin (Role 2)
IF EXISTS (SELECT 1 FROM [Users] WHERE LOWER([Username]) = 'schooladmin')
BEGIN
    UPDATE [Users]
    SET [PasswordHash] = @Hash,
        [IsActive] = 1,
        [MustChangePassword] = 0,
        [FailedLoginAttempts] = 0,
        [LockoutEnd] = NULL,
        [Role] = 2,
        [SchoolId] = @School1Id,
        [DistrictId] = @SukDist,
        [UpdatedAt] = GETUTCDATE()
    WHERE LOWER([Username]) = 'schooladmin';
END
ELSE
BEGIN
    INSERT INTO [Users] ([TenantId], [Username], [PasswordHash], [Name], [Email], [Role], [SchoolId], [DistrictId], [IsActive], [MustChangePassword], [FailedLoginAttempts], [LockoutEnd], [MfaEnabled], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, 'schooladmin', @Hash, 'Govt High School Admin', 'ghs.sukkur@bisesukkur.edu.pk', 2, @School1Id, @SukDist, 1, 0, 0, NULL, 0, GETUTCDATE(), GETUTCDATE());
END

-- sk1-001 (Role 2)
IF EXISTS (SELECT 1 FROM [Users] WHERE LOWER([Username]) = 'sk1-001')
BEGIN
    UPDATE [Users]
    SET [PasswordHash] = @Hash,
        [IsActive] = 1,
        [MustChangePassword] = 0,
        [FailedLoginAttempts] = 0,
        [LockoutEnd] = NULL,
        [Role] = 2,
        [SchoolId] = @School1Id,
        [DistrictId] = @SukDist,
        [UpdatedAt] = GETUTCDATE()
    WHERE LOWER([Username]) = 'sk1-001';
END
ELSE
BEGIN
    INSERT INTO [Users] ([TenantId], [Username], [PasswordHash], [Name], [Email], [Role], [SchoolId], [DistrictId], [IsActive], [MustChangePassword], [FailedLoginAttempts], [LockoutEnd], [MfaEnabled], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, 'sk1-001', @Hash, 'Govt High School Admin', 'sk1.sukkur@bisesukkur.edu.pk', 2, @School1Id, @SukDist, 1, 0, 0, NULL, 0, GETUTCDATE(), GETUTCDATE());
END

-- publicschool_sukkur (Role 2)
IF EXISTS (SELECT 1 FROM [Users] WHERE LOWER([Username]) = 'publicschool_sukkur')
BEGIN
    UPDATE [Users]
    SET [PasswordHash] = @Hash,
        [IsActive] = 1,
        [MustChangePassword] = 0,
        [FailedLoginAttempts] = 0,
        [LockoutEnd] = NULL,
        [Role] = 2,
        [SchoolId] = @School2Id,
        [DistrictId] = @SukDist,
        [UpdatedAt] = GETUTCDATE()
    WHERE LOWER([Username]) = 'publicschool_sukkur';
END
ELSE
BEGIN
    INSERT INTO [Users] ([TenantId], [Username], [PasswordHash], [Name], [Email], [Role], [SchoolId], [DistrictId], [IsActive], [MustChangePassword], [FailedLoginAttempts], [LockoutEnd], [MfaEnabled], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, 'publicschool_sukkur', @Hash, 'Public School Sukkur Admin', 'publicschool@bisesukkur.edu.pk', 2, @School2Id, @SukDist, 1, 0, 0, NULL, 0, GETUTCDATE(), GETUTCDATE());
END

-- 5. Academic Year 2026
IF NOT EXISTS (SELECT 1 FROM [AcademicYears] WHERE [YearName] = '2026')
BEGIN
    INSERT INTO [AcademicYears] ([TenantId], [YearName], [StartDate], [EndDate], [IsActive], [IsEnrollmentOpen], [EnrollmentOpenStart], [EnrollmentOpenEnd], [EnrollmentGraceEnd], [IsEnrollmentGraceEnabled], [EnrollmentLateFeeType], [EnrollmentLateFeeAmount], [IsExamOpen], [ExamOpenStart], [ExamOpenEnd], [ExamGraceEnd], [IsExamGraceEnabled], [ExamLateFeeType], [ExamLateFeeAmount], [ExamTimetableAnnounced], [ResultsDeclared], [PromotionDone], [CreatedAt])
    VALUES (@TenantId, '2026', '2026-01-01', '2026-12-31', 1, 1, '2026-01-01', '2026-09-30 23:59:59', '2026-10-31 23:59:59', 1, 'flat', 800.00, 1, '2026-09-01', '2026-11-30 23:59:59', '2026-12-15 23:59:59', 1, 'flat', 1000.00, 0, 0, 0, GETUTCDATE());
END

SELECT 'SUCCESS: All accounts created/updated with password Admin@12345!' AS [Result];
SELECT [Username], [Name], [Role], [IsActive], [MustChangePassword] FROM [Users];
