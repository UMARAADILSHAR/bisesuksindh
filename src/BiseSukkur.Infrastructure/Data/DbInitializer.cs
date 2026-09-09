using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BiseSukkur.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, string? seedPassword = null, bool isDevelopment = true)
    {
        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == "BISE-SUK" || t.Code == "BISE-HYD");

        if (tenant == null)
        {
            tenant = new Tenant
            {
                Name = "BISE Sukkur",
                Code = "BISE-SUK",
                IsActive = true
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }
        else if (tenant.Code == "BISE-HYD")
        {
            tenant.Name = "BISE Sukkur";
            tenant.Code = "BISE-SUK";
            await context.SaveChangesAsync();
        }

        context.SetTenantForSeeding(tenant.Id);

        // 1. Seed Districts & Tehsils
        if (!await context.Districts.AnyAsync())
        {
            var districtsData = new Dictionary<string, (string shortCode, string[] tehsils)>
            {
                ["Sukkur"] = ("SK", new[] { "Sukkur City", "New Sukkur", "Rohri", "Pano Akil", "Salehpat" }),
                ["Khairpur"] = ("KP", new[] { "Khairpur", "Kot Diji", "Kingri", "Sobhodero", "Gambat", "Thari Mirwah", "Faiz Ganj", "Nara" }),
                ["Ghotki"] = ("GH", new[] { "Ghotki", "Mirpur Mathelo", "Daharki", "Ubauro", "Khangarh" })
            };

            foreach (var kvp in districtsData)
            {
                var district = new District
                {
                    Name = kvp.Key,
                    ShortCode = kvp.Value.shortCode
                };
                context.Districts.Add(district);
                await context.SaveChangesAsync();

                foreach (var tehName in kvp.Value.tehsils)
                {
                    context.Tehsils.Add(new Tehsil
                    {
                        DistrictId = district.Id,
                        Name = tehName
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        var sukDist = await context.Districts.Include(d => d.Tehsils).FirstOrDefaultAsync(d => d.Name == "Sukkur")
            ?? await context.Districts.Include(d => d.Tehsils).FirstOrDefaultAsync();
        var sukTeh = sukDist?.Tehsils.FirstOrDefault();

        // 2. Seed Schools
        if (!await context.Schools.AnyAsync() && sukDist != null && sukTeh != null)
        {
            var school1 = new School
            {
                SemisCode = "418010045",
                Code = "001",
                Name = "Govt Comprehensive High School Sukkur",
                DistrictId = sukDist.Id,
                TehsilId = sukTeh.Id,
                Type = SchoolType.Public,
                Zone = 1,
                Address = "Minara Road, Sukkur",
                ContactNumber = "071-9310123",
                HeadName = "Mohammad Ali Qureshi",
                HeadPhone = "0300-1234567",
                HeadEmail = "head.ghs@bisesukkur.edu.pk",
                AllowedLevelsJson = "[\"SSC-I\",\"SSC-II\",\"HSC-I\",\"HSC-II\"]",
                IsActive = true
            };

            var school2 = new School
            {
                SemisCode = "418010089",
                Code = "002",
                Name = "Public School Sukkur",
                DistrictId = sukDist.Id,
                TehsilId = sukTeh.Id,
                Type = SchoolType.Private,
                Zone = 1,
                Address = "Military Road, Sukkur",
                ContactNumber = "071-5631234",
                HeadName = "Jawad Ahmed Shah",
                HeadPhone = "0333-9876543",
                HeadEmail = "admin.publicschool@bisesukkur.edu.pk",
                AllowedLevelsJson = "[\"SSC-I\",\"SSC-II\"]",
                IsActive = true
            };

            context.Schools.AddRange(school1, school2);
            await context.SaveChangesAsync();
        }

        var sch1 = await context.Schools.FirstOrDefaultAsync(s => s.Code == "001");
        var sch2 = await context.Schools.FirstOrDefaultAsync(s => s.Code == "002");

        // 3. Seed Users & Ensure Access
        var effectivePassword = !string.IsNullOrWhiteSpace(seedPassword)
            ? seedPassword
            : "Admin@12345";

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword(effectivePassword);

        var demoAccounts = new List<(string Username, string Name, string Email, UserRole Role, int? SchoolId, int? DistrictId)>
        {
            ("superadmin", "Board Super Administrator", "superadmin@bisesukkur.edu.pk", UserRole.SuperAdmin, null, null),
            ("districtadmin", "Sukkur District Monitor", "sukkur.monitor@bisesukkur.edu.pk", UserRole.DistrictAdmin, null, sukDist?.Id),
            ("schooladmin", "Govt High School Admin", "ghs.sukkur@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch1?.Id, sukDist?.Id),
            ("kh1-001", "Govt High School Admin", "kh1.sukkur@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch1?.Id, sukDist?.Id),
            ("sk1-001", "Govt High School Admin", "sk1.sukkur@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch1?.Id, sukDist?.Id),
            ("alfalah-002", "Public School Admin", "publicschool.sukkur@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch2?.Id, sukDist?.Id),
            ("gbhs_rohri", "Govt Boys High School Rohri Admin", "gbhs.rohri@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch1?.Id, sukDist?.Id),
            ("ggss_sukkur", "Govt Girls Sec School Sukkur Admin", "ggss.sukkur@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch2?.Id, sukDist?.Id),
            ("publicschool_sukkur", "Public School Sukkur Admin", "publicschool@bisesukkur.edu.pk", UserRole.SchoolAdmin, sch2?.Id, sukDist?.Id)
        };

        foreach (var acc in demoAccounts)
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == acc.Username.ToLower());
            if (existingUser == null)
            {
                context.Users.Add(new User
                {
                    Username = acc.Username,
                    PasswordHash = defaultPasswordHash,
                    Name = acc.Name,
                    Email = acc.Email,
                    Role = acc.Role,
                    SchoolId = acc.SchoolId,
                    DistrictId = acc.DistrictId,
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    LockoutEnd = null,
                    MustChangePassword = false
                });
            }
            else
            {
                // Ensure password is synced to Admin@12345, active, and lockouts/flags cleared
                existingUser.PasswordHash = defaultPasswordHash;
                existingUser.IsActive = true;
                existingUser.FailedLoginAttempts = 0;
                existingUser.LockoutEnd = null;
                existingUser.MustChangePassword = false;
                if (acc.SchoolId.HasValue && existingUser.SchoolId == null) existingUser.SchoolId = acc.SchoolId;
            }
        }
        await context.SaveChangesAsync();

        // 4. Seed Academic Year 2026
        AcademicYear? activeYear = await context.AcademicYears.FirstOrDefaultAsync(y => y.IsActive);
        if (activeYear == null)
        {
            activeYear = new AcademicYear
            {
                YearName = "2026",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                IsActive = true,
                IsEnrollmentOpen = true,
                EnrollmentOpenStart = new DateTime(2026, 1, 1),
                EnrollmentOpenEnd = new DateTime(2026, 9, 30, 23, 59, 59),
                EnrollmentGraceEnd = new DateTime(2026, 10, 31, 23, 59, 59),
                IsEnrollmentGraceEnabled = true,
                EnrollmentLateFeeType = "flat",
                EnrollmentLateFeeAmount = 800m,
                IsExamOpen = true,
                ExamOpenStart = new DateTime(2026, 9, 1),
                ExamOpenEnd = new DateTime(2026, 11, 30, 23, 59, 59),
                ExamGraceEnd = new DateTime(2026, 12, 15, 23, 59, 59),
                IsExamGraceEnabled = true,
                ExamLateFeeType = "flat",
                ExamLateFeeAmount = 1000m
            };

            context.AcademicYears.Add(activeYear);
            await context.SaveChangesAsync();
        }

        // 5. Seed Fee Rates Matrix
        if (!await context.FeeRates.AnyAsync() && activeYear != null)
        {
            var structure = new Dictionary<string, string[]>
            {
                ["SSC-I"] = new[] { "Science", "General Regular", "General Private", "Arts" },
                ["SSC-II"] = new[] { "Science", "General Regular", "General Private", "Arts" },
                ["HSC-I"] = new[] { "Pre-Medical", "Pre-Engineering", "Commerce", "General Science", "Humanities" },
                ["HSC-II"] = new[] { "Pre-Medical", "Pre-Engineering", "Commerce", "General Science", "Humanities" }
            };

            var studentTypes = new[] { "Regular", "Private", "Repeater", "Reappear" };

            var feeSchedules = new Dictionary<string, Dictionary<string, (decimal pub, decimal priv, decimal late)>>
            {
                ["enrollment"] = new()
                {
                    ["SSC-I"] = (1200m, 2000m, 800m),
                    ["SSC-II"] = (1400m, 2200m, 800m),
                    ["HSC-I"] = (1800m, 3000m, 1000m),
                    ["HSC-II"] = (2000m, 3200m, 1000m)
                },
                ["exam"] = new()
                {
                    ["SSC-I"] = (1500m, 2500m, 1000m),
                    ["SSC-II"] = (1600m, 2600m, 1000m),
                    ["HSC-I"] = (2200m, 3500m, 1200m),
                    ["HSC-II"] = (2400m, 3800m, 1200m)
                }
            };

            var feeRates = new List<FeeRate>();
            foreach (var feeType in new[] { "enrollment", "exam" })
            {
                foreach (var classLevel in structure.Keys)
                {
                    var baseFee = feeSchedules[feeType][classLevel];
                    foreach (var group in structure[classLevel])
                    {
                        foreach (var sType in studentTypes)
                        {
                            // Public Slab
                            decimal pubStd = baseFee.pub + (sType == "Private" ? 400m : (sType == "Repeater" ? 200m : 0m));
                            feeRates.Add(new FeeRate
                            {
                                AcademicYearId = activeYear.Id,
                                FeeType = feeType,
                                ClassLevel = classLevel,
                                GroupName = group,
                                StudentType = sType,
                                FeeSlab = "public",
                                StandardFee = pubStd,
                                LateFee = baseFee.late,
                                IsActive = true
                            });

                            // Private Slab
                            decimal privStd = baseFee.priv + (sType == "Private" ? 500m : (sType == "Repeater" ? 300m : 0m));
                            feeRates.Add(new FeeRate
                            {
                                AcademicYearId = activeYear.Id,
                                FeeType = feeType,
                                ClassLevel = classLevel,
                                GroupName = group,
                                StudentType = sType,
                                FeeSlab = "private",
                                StandardFee = privStd,
                                LateFee = baseFee.late + 200m,
                                IsActive = true
                            });
                        }
                    }
                }
            }

            context.FeeRates.AddRange(feeRates);
            await context.SaveChangesAsync();
        }

        // 6. Seed Invoice Sequences
        if (!await context.InvoiceSequences.AnyAsync())
        {
            context.InvoiceSequences.AddRange(
                new InvoiceSequence { SequenceType = "invoice", LastNumber = 0 },
                new InvoiceSequence { SequenceType = "enrollment", LastNumber = 0 },
                new InvoiceSequence { SequenceType = "exam", LastNumber = 0 }
            );
            await context.SaveChangesAsync();
        }

        // 7. Seed Exam Center & Schedules
        if (!await context.ExamCenters.AnyAsync() && sukDist != null)
        {
            var center = new ExamCenter
            {
                CenterCode = "CTR-SUK-01",
                Name = "Govt Islamia College & Higher Secondary Center Sukkur",
                DistrictId = sukDist.Id,
                SuperintendentName = "Prof. Tariq Mehmood",
                SuperintendentPhone = "0301-7788990",
                Capacity = 600,
                IsActive = true
            };
            context.ExamCenters.Add(center);
            await context.SaveChangesAsync();

            if (sch1 != null)
            {
                context.ExamCenterSchools.Add(new ExamCenterSchool
                {
                    ExamCenterId = center.Id,
                    SchoolId = sch1.Id
                });
                await context.SaveChangesAsync();
            }
        }

        if (!await context.ExamSchedules.AnyAsync() && activeYear != null)
        {
            var examDate = new DateTime(2026, 10, 15);
            var schedules = new List<ExamSchedule>
            {
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "ENGLISH-I", ExamDate = examDate, Session = "Morning" },
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "SINDHI", ExamDate = examDate.AddDays(2), Session = "Morning" },
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "CHEMISTRY-I", ExamDate = examDate.AddDays(4), Session = "Morning" },
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "PHYSICS-I", ExamDate = examDate.AddDays(7), Session = "Morning" },
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "MATHEMATICS-I", ExamDate = examDate.AddDays(9), Session = "Morning" },
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "ISLAMIAT", ExamDate = examDate.AddDays(11), Session = "Morning" },
                new() { AcademicYearId = activeYear.Id, ClassLevel = "SSC-I", Group = "Science", SubjectName = "BIOLOGY-I", ExamDate = examDate.AddDays(14), Session = "Morning" }
            };

            context.ExamSchedules.AddRange(schedules);
            await context.SaveChangesAsync();
        }

        // 8. Seed Demo Enrollments
        if (!await context.Enrollments.AnyAsync() && sch1 != null && activeYear != null)
        {
            var demoStudents = new List<Enrollment>
            {
                new()
                {
                    SchoolId = sch1.Id,
                    AcademicYearId = activeYear.Id,
                    StudentName = "Bilal Ahmed Khan",
                    FatherName = "Ahmed Nawaz Khan",
                    Surname = "Khan",
                    GrNumber = "GR-101",
                    Cnic = "41303-1234567-1",
                    DateOfBirth = new DateTime(2009, 4, 15),
                    Gender = "Male",
                    Medium = "English",
                    Religion = "Islam",
                    Nationality = "Pakistani",
                    ClassLevel = "SSC-I",
                    Group = "Science",
                    StudentType = "Regular",
                    SubjectsJson = "[\"SINDHI\",\"ENGLISH-I\",\"ISLAMIAT\",\"CHEMISTRY-I\",\"PHYSICS-I\",\"BIOLOGY-I\",\"MATHEMATICS-I\"]",
                    Status = EnrollmentStatus.Final
                },
                new()
                {
                    SchoolId = sch1.Id,
                    AcademicYearId = activeYear.Id,
                    StudentName = "Ayesha Fatima",
                    FatherName = "Muhammad Rashid",
                    Surname = "Memon",
                    GrNumber = "GR-102",
                    Cnic = "41303-2345678-2",
                    DateOfBirth = new DateTime(2009, 8, 20),
                    Gender = "Female",
                    Medium = "English",
                    Religion = "Islam",
                    Nationality = "Pakistani",
                    ClassLevel = "SSC-I",
                    Group = "Science",
                    StudentType = "Regular",
                    SubjectsJson = "[\"SINDHI\",\"ENGLISH-I\",\"ISLAMIAT\",\"CHEMISTRY-I\",\"PHYSICS-I\",\"BIOLOGY-I\",\"MATHEMATICS-I\"]",
                    Status = EnrollmentStatus.Final
                },
                new()
                {
                    SchoolId = sch1.Id,
                    AcademicYearId = activeYear.Id,
                    StudentName = "Zubair Ali Chandio",
                    FatherName = "Ghulam Sarwar Chandio",
                    Surname = "Chandio",
                    GrNumber = "GR-103",
                    Cnic = "41303-3456789-3",
                    DateOfBirth = new DateTime(2009, 1, 10),
                    Gender = "Male",
                    Medium = "Sindhi",
                    Religion = "Islam",
                    Nationality = "Pakistani",
                    ClassLevel = "SSC-I",
                    Group = "Science",
                    StudentType = "Regular",
                    SubjectsJson = "[\"SINDHI\",\"ENGLISH-I\",\"ISLAMIAT\",\"CHEMISTRY-I\",\"PHYSICS-I\",\"BIOLOGY-I\",\"MATHEMATICS-I\"]",
                    Status = EnrollmentStatus.Final
                },
                new()
                {
                    SchoolId = sch1.Id,
                    AcademicYearId = activeYear.Id,
                    StudentName = "Sanaullah Soomro",
                    FatherName = "Barkat Ali Soomro",
                    Surname = "Soomro",
                    GrNumber = "GR-104",
                    Cnic = "41303-4567890-5",
                    DateOfBirth = new DateTime(2009, 11, 5),
                    Gender = "Male",
                    Medium = "English",
                    Religion = "Islam",
                    Nationality = "Pakistani",
                    ClassLevel = "SSC-I",
                    Group = "Science",
                    StudentType = "Regular",
                    SubjectsJson = "[\"SINDHI\",\"ENGLISH-I\",\"ISLAMIAT\",\"CHEMISTRY-I\",\"PHYSICS-I\",\"BIOLOGY-I\",\"MATHEMATICS-I\"]",
                    EnrollmentNumber = "E26SSK1-001-0001",
                    EnrollmentNumberAllottedAt = DateTime.UtcNow.AddDays(-10),
                    ChallanStatus = "paid",
                    Status = EnrollmentStatus.Verified
                }
            };

            context.Enrollments.AddRange(demoStudents);
            await context.SaveChangesAsync();

            // Seed demo examination form for the verified student
            var verifiedStudent = demoStudents.Last();
            var examCenter = await context.ExamCenters.FirstOrDefaultAsync();
            var examForm = new ExaminationForm
            {
                EnrollmentId = verifiedStudent.Id,
                SchoolId = sch1.Id,
                AcademicYearId = activeYear.Id,
                ClassLevel = "SSC-I",
                Group = "Science",
                StudentType = "Regular",
                RollNumber = "104012",
                ExamCenterId = examCenter?.Id,
                SubjectsJson = verifiedStudent.SubjectsJson,
                Status = ExamFormStatus.Verified
            };
            context.ExaminationForms.Add(examForm);
            await context.SaveChangesAsync();

            // Seed demo result & certificate
            var result = new Result
            {
                ExaminationFormId = examForm.Id,
                MarksJson = "{\"SINDHI\": 85, \"ENGLISH-I\": 88, \"ISLAMIAT\": 92, \"CHEMISTRY-I\": 90, \"PHYSICS-I\": 94, \"BIOLOGY-I\": 91, \"MATHEMATICS-I\": 95}",
                TotalObtained = 635,
                TotalMaxMarks = 700,
                Percentage = 90.71m,
                Grade = "A-1",
                IsPassed = true,
                Remarks = "Exceptional performance"
            };
            context.Results.Add(result);
            await context.SaveChangesAsync();

            var certificate = new Certificate
            {
                ResultId = result.Id,
                CertificateNumber = "BISE-SUK-2026-0001",
                VerificationToken = "VERIFY-SUK-2026-X9K2L",
                StudentName = verifiedStudent.StudentName,
                FatherName = verifiedStudent.FatherName ?? "",
                EnrollmentNumber = verifiedStudent.EnrollmentNumber ?? "",
                RollNumber = examForm.RollNumber ?? "",
                SchoolName = sch1.Name,
                ClassLevel = "SSC-I",
                Group = "Science",
                Grade = "A-1",
                TotalMarksObtained = 635,
                TotalMaxMarks = 700,
                IssueDate = DateTime.UtcNow
            };
            context.Certificates.Add(certificate);
            await context.SaveChangesAsync();
        }

        // 9. Seed Activity Log
        if (!await context.ActivityLogs.AnyAsync())
        {
            context.ActivityLogs.Add(new ActivityLog
            {
                LogName = "system_initialization",
                Description = "BISE Sukkur Board Management System initialized successfully on .NET 10 Blazor.",
                CausedByUsername = "System"
            });
            await context.SaveChangesAsync();
        }
    }
}
