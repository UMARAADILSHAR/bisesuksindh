using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;

namespace BiseSukkur.Infrastructure.Services;

public class PdfReportService : IPdfReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ICertificateService _certService;

    // Standard Theme Colors
    private static readonly Color PrimaryBlue = new DeviceRgb(30, 58, 138);     // #1e3a8a
    private static readonly Color DarkHeader = new DeviceRgb(15, 23, 42);       // #0f172a
    private static readonly Color BorderColor = new DeviceRgb(203, 213, 225);   // #cbd5e1
    private static readonly Color LightBg = new DeviceRgb(248, 250, 252);       // #f8fafc
    private static readonly Color GoldColor = new DeviceRgb(217, 119, 6);       // #d97706
    private static readonly Color GreenColor = new DeviceRgb(4, 120, 87);       // #047857

    public PdfReportService(ApplicationDbContext context, ICertificateService certService)
    {
        _context = context;
        _certService = certService;
    }

    private static Image? GetLogoImage(float width, float height)
    {
        string[] candidatePaths = [
            System.IO.Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "bise-logo.png"),
            System.IO.Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "bise-logo.jpg"),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "bise-logo.png"),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "src", "BiseSukkur.Web", "wwwroot", "images", "bise-logo.png")
        ];

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                try
                {
                    var imgData = ImageDataFactory.Create(path);
                    return new Image(imgData).SetWidth(width).SetHeight(height);
                }
                catch { }
            }
        }
        return null;
    }

    public async Task<byte[]> GenerateEnrollmentCardPdfAsync(int enrollmentId)
    {
        var student = await _context.Enrollments
            .Include(e => e.School).ThenInclude(s => s!.District)
            .Include(e => e.AcademicYear)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (student == null) throw new KeyNotFoundException($"Enrollment with ID {enrollmentId} not found.");

        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(28, 28, 28, 28);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Outer Card Container Frame
            var cardTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            cardTable.SetBorder(new SolidBorder(PrimaryBlue, 2f));
            cardTable.SetBackgroundColor(ColorConstants.WHITE);

            var cardCell = new Cell().SetPadding(16);

            // 1. Header (Logo + Title + QR)
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 70, 15 })).UseAllAvailableWidth();
            headerTable.SetBorder(Border.NO_BORDER);

            // Logo
            var logoCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            var logoImg = GetLogoImage(46, 46);
            if (logoImg != null) logoCell.Add(logoImg);
            headerTable.AddCell(logoCell);

            // Title
            var titleHeader = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            titleHeader.Add(new Paragraph("BOARD OF INTERMEDIATE & SECONDARY EDUCATION SUKKUR")
                .SetFont(fontBold).SetFontSize(11.5f).SetFontColor(PrimaryBlue).SetMarginBottom(2));
            titleHeader.Add(new Paragraph("STUDENT REGISTRATION & ENROLLMENT CARD")
                .SetFont(fontBold).SetFontSize(9.5f).SetFontColor(DarkHeader).SetMarginBottom(1));
            titleHeader.Add(new Paragraph($"Academic Session: {student.AcademicYear?.YearName ?? "2025–2026"} | BISE Sukkur Sindh")
                .SetFont(fontRegular).SetFontSize(7.5f).SetFontColor(ColorConstants.DARK_GRAY));
            headerTable.AddCell(titleHeader);

            // QR Code in Header
            var qrBytes = _certService.GenerateQrCodePng($"BISE-ENROLL:{student.EnrollmentNumber ?? student.Id.ToString()}|{student.Cnic}");
            var qrImg = new Image(ImageDataFactory.Create(qrBytes)).SetWidth(46).SetHeight(46).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            var rightHeader = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetVerticalAlignment(VerticalAlignment.MIDDLE).Add(qrImg);
            headerTable.AddCell(rightHeader);

            cardCell.Add(headerTable);

            // Divider line
            var divider = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            divider.SetBorder(new SolidBorder(PrimaryBlue, 1f));
            divider.SetMarginTop(8).SetMarginBottom(12);
            cardCell.Add(divider);

            // 2. Body Details (Data Table + Photo Box)
            var bodyTable = new Table(UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
            bodyTable.SetBorder(Border.NO_BORDER);

            // Data rows
            var detailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 35, 65 })).UseAllAvailableWidth();
            detailsTable.SetBorder(Border.NO_BORDER);

            void AddRow(string label, string value, bool isMono = false, bool isBold = false)
            {
                var lblCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(4)
                    .Add(new Paragraph(label).SetFont(fontBold).SetFontSize(8f).SetFontColor(DarkHeader));
                var valCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(4);
                var p = new Paragraph(value).SetFontSize(8f).SetFont(isBold ? fontBold : fontRegular);
                if (isMono) p.SetFontColor(PrimaryBlue);
                valCell.Add(p);
                detailsTable.AddCell(lblCell);
                detailsTable.AddCell(valCell);
            }

            AddRow("REGISTRATION NUMBER:", student.EnrollmentNumber ?? "PENDING ALLOTMENT", true, true);
            AddRow("CANDIDATE NAME:", student.StudentName.ToUpperInvariant(), false, true);
            AddRow("FATHER'S NAME:", (student.FatherName ?? "-").ToUpperInvariant());
            AddRow("CNIC / B-FORM NO:", student.Cnic);
            AddRow("DATE OF BIRTH:", $"{student.DateOfBirth:dd MMMM yyyy} ({student.DateOfBirthWords})");
            AddRow("GENDER / RELIGION:", $"{student.Gender}  |  {student.Religion}");
            AddRow("INSTITUTION:", $"{student.School?.Name ?? "School"} (SEMIS: {student.School?.SemisCode ?? "-"})");
            AddRow("CLASS & GROUP:", $"{student.ClassLevel} - {student.Group} ({student.StudentType}) | Medium: {student.Medium}");
            AddRow("G.R / ADMISSION NO:", student.GrNumber ?? "-");

            var leftBodyCell = new Cell().SetBorder(Border.NO_BORDER).Add(detailsTable);
            bodyTable.AddCell(leftBodyCell);

            // Photo box
            var photoCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
            var photoBox = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(95).SetHeight(120);
            photoBox.SetBorder(new SolidBorder(BorderColor, 1.5f)).SetBackgroundColor(LightBg);
            photoBox.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            var innerPhotoCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE).SetTextAlignment(TextAlignment.CENTER);
            innerPhotoCell.Add(new Paragraph("PASSPORT\nPHOTO\nSTAMP").SetFont(fontBold).SetFontSize(7).SetFontColor(ColorConstants.GRAY));
            photoBox.AddCell(innerPhotoCell);
            photoCell.Add(photoBox);
            photoCell.Add(new Paragraph("Official Stamp").SetFont(fontRegular).SetFontSize(6.5f).SetFontColor(ColorConstants.DARK_GRAY).SetMarginTop(3));
            bodyTable.AddCell(photoCell);

            cardCell.Add(bodyTable);

            // 3. Footer Signature Bar
            var sigTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 })).UseAllAvailableWidth().SetMarginTop(24);
            sigTable.SetBorder(Border.NO_BORDER);

            var sig1 = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("___________________________\nCandidate Signature").SetFont(fontRegular).SetFontSize(7f).SetFontColor(DarkHeader));
            var sig2 = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("___________________________\nPrincipal / Head Signature").SetFont(fontRegular).SetFontSize(7f).SetFontColor(DarkHeader));
            var sig3 = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("___________________________\nSecretary BISE Sukkur").SetFont(fontBold).SetFontSize(7f).SetFontColor(PrimaryBlue));

            sigTable.AddCell(sig1);
            sigTable.AddCell(sig2);
            sigTable.AddCell(sig3);
            cardCell.Add(sigTable);

            cardTable.AddCell(cardCell);
            doc.Add(cardTable);
            doc.Close();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateRollNumberSlipPdfAsync(int examFormId)
    {
        var form = await _context.ExaminationForms
            .Include(ef => ef.Enrollment).ThenInclude(e => e!.School)
            .Include(ef => ef.ExamCenter)
            .Include(ef => ef.AcademicYear)
            .FirstOrDefaultAsync(ef => ef.Id == examFormId);

        if (form == null) throw new KeyNotFoundException($"Examination Form {examFormId} not found.");

        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(24, 24, 24, 24);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Outer Frame
            var frameTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            frameTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1.5f));
            var rootCell = new Cell().SetPadding(14);

            // Header with Logo
            var hdrTable = new Table(UnitValue.CreatePercentArray(new float[] { 14, 86 })).UseAllAvailableWidth().SetMarginBottom(8);
            hdrTable.SetBorder(Border.NO_BORDER);

            var logoCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            var logo = GetLogoImage(48, 48);
            if (logo != null) logoCell.Add(logo);
            hdrTable.AddCell(logoCell);

            var textCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            textCell.Add(new Paragraph("BOARD OF INTERMEDIATE & SECONDARY EDUCATION SUKKUR")
                .SetFont(fontBold).SetFontSize(12).SetMarginBottom(2));
            textCell.Add(new Paragraph($"EXAMINATION ADMIT CARD / ROLL NUMBER SLIP — ANNUAL {form.AcademicYear?.YearName ?? "2026"}")
                .SetFont(fontBold).SetFontSize(10f).SetMarginBottom(1));
            textCell.Add(new Paragraph($"{form.ClassLevel} — {form.Group.ToUpperInvariant()} GROUP ({form.StudentType.ToUpperInvariant()})")
                .SetFont(fontBold).SetFontSize(8.5f).SetFontColor(PrimaryBlue));
            hdrTable.AddCell(textCell);

            rootCell.Add(hdrTable);

            // Candidate Summary + Photo
            var infoGrid = new Table(UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
            infoGrid.SetBorder(Border.NO_BORDER);

            var candInfo = new Table(UnitValue.CreatePercentArray(new float[] { 32, 68 })).UseAllAvailableWidth();
            candInfo.SetBorder(Border.NO_BORDER);

            void AddInfo(string lbl, string val, bool highlight = false)
            {
                var c1 = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(3)
                    .Add(new Paragraph(lbl).SetFont(fontBold).SetFontSize(8f).SetFontColor(ColorConstants.BLACK));
                var c2 = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(3);
                var p = new Paragraph(val).SetFontSize(8f).SetFont(fontRegular);
                if (highlight) p.SetFont(fontBold).SetFontColor(PrimaryBlue);
                c2.Add(p);
                candInfo.AddCell(c1);
                candInfo.AddCell(c2);
            }

            AddInfo("ROLL NUMBER:", form.RollNumber ?? "PENDING", true);
            AddInfo("ENROLLMENT NO:", form.Enrollment?.EnrollmentNumber ?? "-");
            AddInfo("CANDIDATE NAME:", (form.Enrollment?.StudentName ?? "-").ToUpperInvariant(), true);
            AddInfo("FATHER'S NAME:", (form.Enrollment?.FatherName ?? "-").ToUpperInvariant());
            AddInfo("CNIC / B-FORM:", form.Enrollment?.Cnic ?? "-");
            AddInfo("INSTITUTION:", form.Enrollment?.School?.Name ?? "School");
            AddInfo("EXAMINATION CENTER:", $"{form.ExamCenter?.Name ?? "Designated Center"} ({form.ExamCenter?.CenterCode ?? "-"})", true);

            infoGrid.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(candInfo));

            // Photo box + QR
            var rightBox = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
            var pBox = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(90).SetHeight(105);
            pBox.SetBorder(new SolidBorder(ColorConstants.BLACK, 1f)).SetBackgroundColor(LightBg).SetHorizontalAlignment(HorizontalAlignment.CENTER);
            var pCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE).SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("PHOTO").SetFont(fontBold).SetFontSize(7).SetFontColor(ColorConstants.GRAY));
            pBox.AddCell(pCell);
            rightBox.Add(pBox);

            var qrBytes = _certService.GenerateQrCodePng($"BISE-ROLL:{form.RollNumber}|{form.Enrollment?.Cnic}");
            var qrImg = new Image(ImageDataFactory.Create(qrBytes)).SetWidth(40).SetHeight(40).SetHorizontalAlignment(HorizontalAlignment.CENTER).SetMarginTop(4);
            rightBox.Add(qrImg);
            infoGrid.AddCell(rightBox);

            rootCell.Add(infoGrid);

            // Date Sheet Table
            rootCell.Add(new Paragraph("EXAMINATION PAPERS & TIMETABLE SCHEDULE")
                .SetFont(fontBold).SetFontSize(8.5f).SetMarginTop(10).SetMarginBottom(4));

            var paperTable = new Table(UnitValue.CreatePercentArray(new float[] { 18, 42, 20, 20 })).UseAllAvailableWidth();
            paperTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1f));

            void AddTh(string text) =>
                paperTable.AddHeaderCell(new Cell().SetBackgroundColor(new DeviceRgb(241, 245, 249)).SetPadding(4)
                    .Add(new Paragraph(text).SetFont(fontBold).SetFontSize(7.5f).SetTextAlignment(TextAlignment.CENTER)));

            AddTh("Date");
            AddTh("Subject Paper");
            AddTh("Session / Timing");
            AddTh("Invigilator Sign");

            string[] defaultSubjects = form.ClassLevel.StartsWith("SSC")
                ? ["English (Compulsory)", "Sindhi / Urdu (Compulsory)", "Pakistan Studies", "Mathematics", "Physics", "Chemistry", "Biology / Computer"]
                : ["English", "Urdu", "Islamic Studies", "Physics", "Chemistry", "Mathematics / Biology"];

            var startDate = DateTime.Today.AddDays(30);
            int dayOffset = 0;
            foreach (var sub in defaultSubjects)
            {
                var examDate = startDate.AddDays(dayOffset);
                dayOffset += (dayOffset % 2 == 0) ? 2 : 3;

                paperTable.AddCell(new Cell().SetPadding(3).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph(examDate.ToString("dd-MM-yyyy")).SetFont(fontRegular).SetFontSize(7.5f)));
                paperTable.AddCell(new Cell().SetPadding(3)
                    .Add(new Paragraph(sub).SetFont(fontBold).SetFontSize(7.5f)));
                paperTable.AddCell(new Cell().SetPadding(3).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Morning (09:00 AM)").SetFont(fontRegular).SetFontSize(7.5f)));
                paperTable.AddCell(new Cell().SetPadding(3).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("").SetFont(fontRegular).SetFontSize(7.5f)));
            }

            rootCell.Add(paperTable);

            // Instructions & Signatures
            var signRow = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth().SetMarginTop(18);
            signRow.SetBorder(Border.NO_BORDER);
            signRow.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("___________________________\nCandidate Signature").SetFont(fontRegular).SetFontSize(7f)));
            signRow.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("___________________________\nController of Examinations").SetFont(fontBold).SetFontSize(7.5f).SetFontColor(PrimaryBlue)));

            rootCell.Add(signRow);

            frameTable.AddCell(rootCell);
            doc.Add(frameTable);
            doc.Close();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateCertificatePdfAsync(int certificateIdOrResultId)
    {
        var cert = await _context.Certificates
            .FirstOrDefaultAsync(c => c.Id == certificateIdOrResultId || c.ResultId == certificateIdOrResultId);

        if (cert == null)
        {
            var res = await _context.Results
                .Include(r => r.ExaminationForm).ThenInclude(ef => ef!.Enrollment).ThenInclude(e => e!.School)
                .Include(r => r.ExaminationForm).ThenInclude(ef => ef!.AcademicYear)
                .FirstOrDefaultAsync(r => r.Id == certificateIdOrResultId);

            if (res == null) throw new KeyNotFoundException("Certificate or Result record not found.");
            cert = await _certService.GenerateCertificateAsync(res.Id);
        }

        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(28, 28, 28, 28);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var fontTimesBold = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            var fontTimes = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

            // Double Ornamental Frame
            var certTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
            certTable.SetBorder(new SolidBorder(PrimaryBlue, 4f));
            certTable.SetBackgroundColor(new DeviceRgb(255, 253, 245));

            var certCell = new Cell().SetPadding(24).SetBorder(new DoubleBorder(GoldColor, 2f));

            // Header with Emblem Logo
            var logo = GetLogoImage(58, 58);
            if (logo != null)
            {
                logo.SetHorizontalAlignment(HorizontalAlignment.CENTER).SetMarginBottom(8);
                certCell.Add(logo);
            }

            certCell.Add(new Paragraph("BOARD OF INTERMEDIATE & SECONDARY EDUCATION SUKKUR")
                .SetFont(fontTimesBold).SetFontSize(15).SetFontColor(PrimaryBlue).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(2));
            certCell.Add(new Paragraph("PROVISIONAL PASSING CERTIFICATE & STATEMENT OF MARKS")
                .SetFont(fontTimesBold).SetFontSize(11f).SetFontColor(GoldColor).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(16));

            // Statement text
            var statement = new Paragraph()
                .SetFont(fontTimes).SetFontSize(10.5f).SetMultipliedLeading(1.8f).SetTextAlignment(TextAlignment.JUSTIFIED)
                .Add("This is to certify that ")
                .Add(new Text(cert.StudentName.ToUpperInvariant()).SetFont(fontTimesBold).SetFontColor(PrimaryBlue))
                .Add(", Son / Daughter of ")
                .Add(new Text((cert.FatherName ?? "-").ToUpperInvariant()).SetFont(fontTimesBold))
                .Add(", bearing Registration Number ")
                .Add(new Text(cert.EnrollmentNumber).SetFont(fontBold))
                .Add(" and Examination Roll Number ")
                .Add(new Text(cert.RollNumber).SetFont(fontBold).SetFontColor(PrimaryBlue))
                .Add(", of institution ")
                .Add(new Text(cert.SchoolName).SetFont(fontTimesBold))
                .Add($", has successfully passed the {cert.ClassLevel} Annual Examination in {cert.Group} Group securing ")
                .Add(new Text($"{cert.TotalMarksObtained} / {cert.TotalMaxMarks} Marks").SetFont(fontBold).SetFontColor(GreenColor))
                .Add(" and is placed in Grade ")
                .Add(new Text(cert.Grade).SetFont(fontBold).SetFontSize(12).SetFontColor(GreenColor))
                .Add(".");

            certCell.Add(statement);

            // Details & Verification Footer
            var certFooter = new Table(UnitValue.CreatePercentArray(new float[] { 40, 20, 40 })).UseAllAvailableWidth().SetMarginTop(30);
            certFooter.SetBorder(Border.NO_BORDER);

            var metaCell = new Cell().SetBorder(Border.NO_BORDER);
            metaCell.Add(new Paragraph($"Certificate #: {cert.CertificateNumber}").SetFont(fontBold).SetFontSize(7.5f));
            metaCell.Add(new Paragraph($"Token: {cert.VerificationToken}").SetFont(fontRegular).SetFontSize(7f).SetFontColor(ColorConstants.DARK_GRAY));
            metaCell.Add(new Paragraph($"Issue Date: {cert.IssueDate:dd MMMM yyyy}").SetFont(fontRegular).SetFontSize(7f).SetFontColor(ColorConstants.DARK_GRAY));
            certFooter.AddCell(metaCell);

            var qrBytes = _certService.GenerateQrCodePng($"https://bisesukkur.edu.pk/certificate/verify/{cert.CertificateNumber}");
            var qrImg = new Image(ImageDataFactory.Create(qrBytes)).SetWidth(55).SetHeight(55).SetHorizontalAlignment(HorizontalAlignment.CENTER);
            certFooter.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(qrImg));

            var ctrlCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
            ctrlCell.Add(new Paragraph("___________________________\nController of Examinations\nBISE Sukkur").SetFont(fontBold).SetFontSize(7.5f).SetFontColor(PrimaryBlue));
            certFooter.AddCell(ctrlCell);

            certCell.Add(certFooter);
            certTable.AddCell(certCell);
            doc.Add(certTable);
            doc.Close();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateBankChallanPdfAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.School).ThenInclude(s => s!.District)
            .Include(i => i.AcademicYear)
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            // Landscape A4 for 3-Column Bank Slip (Bank Copy, Board Copy, School Copy)
            var doc = new Document(pdf, PageSize.A4.Rotate());
            doc.SetMargins(15, 15, 15, 15);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var threeCols = new Table(UnitValue.CreatePercentArray(new float[] { 33.3f, 33.3f, 33.4f })).UseAllAvailableWidth();
            threeCols.SetBorder(Border.NO_BORDER);

            string[] copies = ["1. BANK COPY", "2. BOARD COPY", "3. INSTITUTION / SCHOOL COPY"];

            foreach (var copyName in copies)
            {
                var copyCell = new Cell().SetPadding(8).SetBorder(new SolidBorder(BorderColor, 1f));

                // Slip Header with Mini Logo
                var slipHdr = new Table(UnitValue.CreatePercentArray(new float[] { 18, 82 })).UseAllAvailableWidth().SetMarginBottom(4);
                slipHdr.SetBorder(Border.NO_BORDER);

                var logo = GetLogoImage(28, 28);
                var lCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
                if (logo != null) lCell.Add(logo);
                slipHdr.AddCell(lCell);

                var rCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
                rCell.Add(new Paragraph("HBL / SINDH BANK CHALLAN").SetFont(fontBold).SetFontSize(8f).SetFontColor(PrimaryBlue));
                rCell.Add(new Paragraph("BISE SUKKUR COLLECTION A/C").SetFont(fontBold).SetFontSize(6.5f));
                slipHdr.AddCell(rCell);

                copyCell.Add(slipHdr);
                copyCell.Add(new Paragraph($"Account #: 0042-79015482-03  |  {copyName}").SetFont(fontBold).SetFontSize(6.5f).SetFontColor(GoldColor).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(6));

                // Slip Fields
                var fields = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 })).UseAllAvailableWidth();
                fields.SetBorder(Border.NO_BORDER);

                void AddF(string k, string v, bool b = false)
                {
                    fields.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(2).Add(new Paragraph(k).SetFont(fontBold).SetFontSize(6.5f).SetFontColor(DarkHeader)));
                    var p = new Paragraph(v).SetFontSize(6.5f).SetFont(b ? fontBold : fontRegular);
                    if (b) p.SetFontColor(PrimaryBlue);
                    fields.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(2).Add(p));
                }

                AddF("CHALLAN NO:", invoice.InvoiceNumber, true);
                AddF("ISSUE DATE:", invoice.GeneratedAt.ToString("dd-MM-yyyy"));
                AddF("DUE DATE:", invoice.GeneratedAt.AddDays(15).ToString("dd-MM-yyyy"), true);
                AddF("INSTITUTION:", $"{invoice.School?.Name ?? "School"}");
                AddF("SEMIS CODE:", invoice.School?.SemisCode ?? "-");
                AddF("CATEGORY:", invoice.ClassGroupStudentType);
                AddF("CANDIDATES:", $"{invoice.StudentCount} Student(s)");
                AddF("PAYABLE AMOUNT:", $"PKR {invoice.Amount:N0}", true);

                copyCell.Add(fields);

                // Stamp Box
                var stampTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth().SetMarginTop(14);
                stampTable.SetBorder(Border.NO_BORDER);
                stampTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("____________________\nDepositor Signature").SetFont(fontRegular).SetFontSize(5.5f)));
                stampTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("____________________\nBank Officer Stamp").SetFont(fontRegular).SetFontSize(5.5f)));

                copyCell.Add(stampTable);
                threeCols.AddCell(copyCell);
            }

            doc.Add(threeCols);
            doc.Close();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateCandidateListPdfAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.School)
            .Include(i => i.AcademicYear)
            .Include(i => i.Items).ThenInclude(it => it.Enrollment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(20, 20, 20, 20);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Header with Logo
            var hdrTable = new Table(UnitValue.CreatePercentArray(new float[] { 12, 88 })).UseAllAvailableWidth().SetMarginBottom(8);
            hdrTable.SetBorder(Border.NO_BORDER);

            var logo = GetLogoImage(42, 42);
            var lCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            if (logo != null) lCell.Add(logo);
            hdrTable.AddCell(lCell);

            var rCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
            rCell.Add(new Paragraph("BOARD OF INTERMEDIATE & SECONDARY EDUCATION SUKKUR")
                .SetFont(fontBold).SetFontSize(11.5f).SetMarginBottom(1));
            rCell.Add(new Paragraph("OFFICIAL CANDIDATE SUBMISSION DESPATCH LIST")
                .SetFont(fontBold).SetFontSize(9f).SetFontColor(PrimaryBlue).SetMarginBottom(1));
            rCell.Add(new Paragraph($"School: {invoice.School?.Name} (SEMIS: {invoice.School?.SemisCode})  |  Challan #: {invoice.InvoiceNumber}  |  Batch: {invoice.ClassGroupStudentType}")
                .SetFont(fontRegular).SetFontSize(7.5f));
            hdrTable.AddCell(rCell);

            doc.Add(hdrTable);

            // Table
            var listTable = new Table(UnitValue.CreatePercentArray(new float[] { 5, 24, 22, 18, 12, 11, 8 })).UseAllAvailableWidth();
            listTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1f));

            void AddH(string text) =>
                listTable.AddHeaderCell(new Cell().SetBackgroundColor(new DeviceRgb(241, 245, 249)).SetPadding(3)
                    .Add(new Paragraph(text).SetFont(fontBold).SetFontSize(7f).SetTextAlignment(TextAlignment.CENTER)));

            AddH("#");
            AddH("Candidate Name");
            AddH("Father's Name");
            AddH("CNIC / B-Form");
            AddH("DOB");
            AddH("GR No");
            AddH("Photo");

            int idx = 1;
            var includedItems = invoice.Items.Where(it => it.IsIncluded).OrderBy(it => it.EnrollmentId).ToList();

            foreach (var it in includedItems)
            {
                var e = it.Enrollment;
                listTable.AddCell(new Cell().SetPadding(3).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(idx.ToString()).SetFont(fontRegular).SetFontSize(6.5f)));
                listTable.AddCell(new Cell().SetPadding(3).Add(new Paragraph((e?.StudentName ?? "-").ToUpperInvariant()).SetFont(fontBold).SetFontSize(6.5f)));
                listTable.AddCell(new Cell().SetPadding(3).Add(new Paragraph((e?.FatherName ?? "-").ToUpperInvariant()).SetFont(fontRegular).SetFontSize(6.5f)));
                listTable.AddCell(new Cell().SetPadding(3).Add(new Paragraph(e?.Cnic ?? "-").SetFont(fontRegular).SetFontSize(6.5f)));
                listTable.AddCell(new Cell().SetPadding(3).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(e?.DateOfBirth.ToString("dd-MM-yyyy") ?? "-").SetFont(fontRegular).SetFontSize(6.5f)));
                listTable.AddCell(new Cell().SetPadding(3).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph(e?.GrNumber ?? "-").SetFont(fontRegular).SetFontSize(6.5f)));
                listTable.AddCell(new Cell().SetPadding(2).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph("[ STAMP ]").SetFont(fontRegular).SetFontSize(5f).SetFontColor(ColorConstants.GRAY)));
                idx++;
            }

            doc.Add(listTable);

            // Signature footer
            var sig = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 })).UseAllAvailableWidth().SetMarginTop(24);
            sig.SetBorder(Border.NO_BORDER);
            sig.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph("Prepared By: ______________").SetFont(fontRegular).SetFontSize(7f)));
            sig.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph("Checked By: ______________").SetFont(fontRegular).SetFontSize(7f)));
            sig.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph("Principal / Headmaster Stamp").SetFont(fontBold).SetFontSize(7f)));
            doc.Add(sig);

            doc.Close();
        }

        return ms.ToArray();
    }
}
