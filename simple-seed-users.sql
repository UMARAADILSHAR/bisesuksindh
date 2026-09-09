UPDATE [dbo].[Tenants] 
SET [Name] = 'BISE Sukkur', [Code] = 'BISE-SUK', [IsActive] = 1 
WHERE [Id] = 1;

DELETE FROM [dbo].[Users] 
WHERE [Username] IN ('superadmin', 'districtadmin', 'schooladmin', 'sk1-001', 'publicschool_sukkur');

INSERT INTO [dbo].[Users] 
([TenantId], [Username], [PasswordHash], [Name], [Email], [Role], [SchoolId], [DistrictId], [IsActive], [MustChangePassword], [FailedLoginAttempts], [LockoutEnd], [MfaEnabled], [CreatedAt], [UpdatedAt])
VALUES 
(1, 'superadmin', '$2a$11$gHpNvMWiixl2oaRbWe69/e02Ocy0MKlyviVAqo7sP8JUGbufNyyQy', 'Board Super Administrator', 'superadmin@bisesukkur.edu.pk', 0, NULL, NULL, 1, 0, 0, NULL, 0, '2026-01-01', '2026-01-01'),
(1, 'districtadmin', '$2a$11$gHpNvMWiixl2oaRbWe69/e02Ocy0MKlyviVAqo7sP8JUGbufNyyQy', 'Sukkur District Monitor', 'sukkur.monitor@bisesukkur.edu.pk', 1, NULL, NULL, 1, 0, 0, NULL, 0, '2026-01-01', '2026-01-01'),
(1, 'schooladmin', '$2a$11$gHpNvMWiixl2oaRbWe69/e02Ocy0MKlyviVAqo7sP8JUGbufNyyQy', 'Govt High School Admin', 'ghs.sukkur@bisesukkur.edu.pk', 2, NULL, NULL, 1, 0, 0, NULL, 0, '2026-01-01', '2026-01-01'),
(1, 'sk1-001', '$2a$11$gHpNvMWiixl2oaRbWe69/e02Ocy0MKlyviVAqo7sP8JUGbufNyyQy', 'Govt High School Admin', 'sk1.sukkur@bisesukkur.edu.pk', 2, NULL, NULL, 1, 0, 0, NULL, 0, '2026-01-01', '2026-01-01'),
(1, 'publicschool_sukkur', '$2a$11$gHpNvMWiixl2oaRbWe69/e02Ocy0MKlyviVAqo7sP8JUGbufNyyQy', 'Public School Sukkur Admin', 'publicschool@bisesukkur.edu.pk', 2, NULL, NULL, 1, 0, 0, NULL, 0, '2026-01-01', '2026-01-01');

SELECT [Id], [Username], [Role], [IsActive], [MustChangePassword] FROM [dbo].[Users];
