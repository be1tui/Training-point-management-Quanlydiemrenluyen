CREATE DATABASE QLDRL
USE QLDRL
CREATE TABLE AdminMenu (
    AdminMenuId INT PRIMARY KEY IDENTITY(1,1),
    ItemName NVARCHAR(100),
    ItemLevel INT,
    ParentLevel INT,
    ItemOrder INT,
    IsActive BIT DEFAULT 1,
    ItemTarget VARCHAR(50),
    AreaName VARCHAR(50),
    ControllerName VARCHAR(50),
    ActionName VARCHAR(50),
    Icon VARCHAR(50),
    IdName VARCHAR(50)
);
-- Bật IDENTITY_INSERT
SET IDENTITY_INSERT AdminMenu ON;

-- Chèn dữ liệu
INSERT INTO AdminMenu (
    AdminMenuId, ItemName, ItemLevel, ParentLevel, ItemOrder, IsActive,
    ItemTarget, AreaName, ControllerName, ActionName, Icon, IdName
)
VALUES
(1, N'Quản lý hệ thống', 1, 0, 1, 1, 'forms-nav', 'Admin', 'Home', 'Index', 'bx bx-layout', 'forms-nav'),
(2, N'Quản lý lớp', 2, 1, 1, 1, NULL, 'Admin', 'Class', 'Index', 'bx-ri-account-box-line', NULL),
(3, N'Quản lý tài khoản', 2, 1, 2, 1, NULL, 'Admin', 'Account', 'Index', 'bx-ri-account-box-line', NULL),
(4, N'Quản lý năm học', 2, 1, 3, 1, NULL, 'Admin', 'AcademicYear', 'Index', 'bi bi-journal-text', NULL),
(5, N'Quản lý bài viết', 2, 1, 4, 1, 'charts-nav', 'Admin', 'Home', 'Index', 'bi bi-bar-chart', 'charts-nav'),
(6, N'Thống kê', 1, 0, 1, 1, NULL, 'Admin', 'Home', 'Index', 'bx bxs-bar-chart-alt-2', NULL),
(7, N'Thống kê 1', 2, 6, 1, 1, NULL, 'Admin', 'Home', 'Index', NULL, NULL),
(8, N'Thống kê 2', 2, 6, 2, 1, 'charts-nav', 'Admin', 'Home', 'Index', 'bi bi-bar-chart', 'charts-nav');

-- Tắt IDENTITY_INSERT
SET IDENTITY_INSERT AdminMenu OFF;

-- bảng khoa/viện
CREATE TABLE Faculty (
    FacultyId INT PRIMARY KEY IDENTITY(1,1),
    FacultyName NVARCHAR(255) NOT NULL, -- Tên khoa/viện
    Description NVARCHAR(MAX) NULL -- Mô tả (nếu cần)
);
INSERT INTO Faculty (FacultyName) VALUES 
(N'Kỹ thuật và Công nghệ'),
(N'Khoa Sư phạm Toán'),
(N'Khoa Sư phạm Ngữ văn'),
(N'Khoa Giáo dục Chính trị'),
(N'Khoa Sinh học'),
(N'Khoa Hóa học'),
(N'Khoa Lịch sử'),
(N'Khoa Vật lý và Kỹ thuật'),
(N'Khoa Ngoại ngữ'),
(N'Khoa Kinh tế'),
(N'Viện Nông nghiệp và Tài nguyên');
select * from Class
CREATE TABLE Class (
    ClassId INT PRIMARY KEY IDENTITY(1,1),
    ClassName VARCHAR(50) NOT NULL, -- Tên lớp
    Description NVARCHAR(255), -- Mô tả lớp
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP -- Ngày tạo lớp
);
ALTER TABLE Class
ALTER COLUMN ClassName NVARCHAR(50) NOT NULL;
-- Nếu mỗi lớp có một lớp trưởng duy nhất
-- thì có thể thêm cột ClassMonitorId vào bảng Class
ALTER TABLE Class ADD ClassMonitorId INT;
ALTER TABLE Class
ADD CONSTRAINT FK_Class_ClassMonitor
FOREIGN KEY (ClassMonitorId) REFERENCES Account(AccountId);

ALTER TABLE Class ADD FacultyId INT;
ALTER TABLE Class
ADD CONSTRAINT FK_Class_Faculty
FOREIGN KEY (FacultyId) REFERENCES Faculty(FacultyId);

INSERT INTO Class (ClassName, FacultyId) VALUES
(N'63K-Công nghệ thông tin ', 1), 
(N'63K-Công nghệ thông tin CLC', 1),
(N'63K-Khoa Học máy tính', 1), 
(N'63K-Sưu phạm toán', 2),
(N'63K-Sưu phạm Tin', 1), 
(N'63K-Sưu phạm toán CLC', 2),
(N'63K-Tin học ứng dụng', 2); 

CREATE TABLE Account (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(255) CHECK (Username LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' OR Username LIKE '[a-zA-Z0-9]%'), -- Tên đăng nhập, kiểm tra định dạng
    Email VARCHAR(255), -- Email
    FullName NVARCHAR(255), -- Họ tên
    Password VARCHAR(255),
    ClassId INT, -- Khóa ngoại đến Class
    Role VARCHAR(35) CHECK (Role IN ('Student', 'Classmonitor', 'Lecturer', 'Admin')),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ClassId) REFERENCES Class(ClassId)
);
ALTER TABLE Account ADD Avatar VARCHAR(255);
ALTER TABLE Account ADD FacultyId INT;
ALTER TABLE Account
ADD CONSTRAINT FK_Account_Faculty
FOREIGN KEY (FacultyId) REFERENCES Faculty(FacultyId);
ALTER TABLE Account
ADD PasswordResetToken NVARCHAR(255);
ALTER TABLE Account
ADD PasswordResetTokenExpiry DATETIME;
Select * from Class
Select * from Faculty
Select * from Account
Select * from AccountProfile

SELECT name
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('AccountProfile');
Select * from AccountProfile
-- bảng thông tin cá nhân 
CREATE TABLE AccountProfile (
    ProfileId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT NOT NULL UNIQUE, -- mỗi tài khoản có 1 hồ sơ
    DateOfBirth DATE, -- Ngày sinh
    PhoneNumber NVARCHAR(15), -- Số điện thoại
    Gender NVARCHAR(10), -- Giới tính
    Address NVARCHAR(255), -- Địa chỉ
    NationalID VARCHAR(20), -- Số CMND/CCCD
    Nationality NVARCHAR(100), -- Quốc tịch
    Ethnicity NVARCHAR(100), -- Dân tộc
    Religion NVARCHAR(100), -- Tôn giáo
    FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);
ALTER TABLE AccountProfile DROP CONSTRAINT FK__AccountPr__Accou__48CFD27E;

ALTER TABLE AccountProfile
ADD CONSTRAINT FK_AccountProfile_Account
FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
ON DELETE CASCADE;


-- Bảng tài khoản người dùng, liên kết với lớp học.
CREATE TABLE AcademicYear (
    AcademicYearId INT IDENTITY(1,1) PRIMARY KEY,
    YearName NVARCHAR(20) NOT NULL -- Tên năm học
);
-- Bảng năm học.
CREATE TABLE Semester (
    SemesterId INT IDENTITY(1,1) PRIMARY KEY,
    SemesterName NVARCHAR(20) NOT NULL, -- Tên học kỳ
    AcademicYearId INT NOT NULL, -- Khóa ngoại đến năm học
    FOREIGN KEY (AcademicYearId) REFERENCES AcademicYear(AcademicYearId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

-- Danh mục tiêu chí rèn luyện (Ví dụ: Ý thức học tập, Tham gia hoạt động...)
CREATE TABLE CriteriaCategory (
    CategoryId INT PRIMARY KEY IDENTITY(1,1), 
    CategoryName NVARCHAR(255) NOT NULL, -- Tên danh mục tiêu chí
    MaxScore INT NOT NULL -- Tổng điểm tối đa cho danh mục
);
Select * from CriteriaCategory
Select * from Criteria
-- Tiêu chí cụ thể trong từng danh mục
CREATE TABLE Criteria (
    CriteriaId INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    CriteriaName NVARCHAR(900) NOT NULL, -- Tên tiêu chí
    MaxScore INT NOT NULL, -- Điểm tối đa cho tiêu chí này
    RequiresEvidence BIT DEFAULT 0, -- Có yêu cầu minh chứng không
    EvaluationMethod NVARCHAR(50), -- Cách đánh giá: 'Student', 'Classmonitor', 'Lecturer'
    FOREIGN KEY (CategoryId) REFERENCES CriteriaCategory(CategoryId)
);
Select * from StudentEvaluation
-- Bài đánh giá của sinh viên
CREATE TABLE StudentEvaluation (
    EvaluationId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT NOT NULL,
    SemesterId INT NOT NULL,
    SubmitDate DATETIME DEFAULT GETDATE(), -- Ngày nộp
    Status NVARCHAR(50) DEFAULT 'Pending', -- Trạng thái: 
    Note NVARCHAR(MAX), -- Ghi chú chung nếu bị từ chối
    FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
    FOREIGN KEY (SemesterId) REFERENCES Semester(SemesterId)
);
Select * from Account
Select * from EvaluationDetail
-- Điểm của từng tiêu chí trong bài đánh giá
CREATE TABLE EvaluationDetail (
    DetailId INT PRIMARY KEY IDENTITY(1,1),
    EvaluationId INT NOT NULL,
    CriteriaId INT NOT NULL,
    StudentScore INT, -- Sinh viên tự đánh giá
    ClassMonitorScore INT, -- Điểm lớp trưởng sửa (nếu có)
    LecturerScore INT, -- Điểm chủ nhiệm khoa sửa (nếu có)
    FinalScore INT, -- Điểm cuối cùng được chốt
    LecturerNote NVARCHAR(MAX), -- Ghi chú của chủ nhiệm khoa nếu có
    FOREIGN KEY (EvaluationId) REFERENCES StudentEvaluation(EvaluationId),
    FOREIGN KEY (CriteriaId) REFERENCES Criteria(CriteriaId)
);
Select * from Evidence
-- Minh chứng do sinh viên nộp cho từng tiêu chí
CREATE TABLE Evidence (
    EvidenceId INT PRIMARY KEY IDENTITY(1,1),
    EvaluationId INT NOT NULL,
    CriteriaId INT NOT NULL,
    FilePath NVARCHAR(500), -- Đường dẫn file minh chứng
    Description NVARCHAR(MAX), -- Minh chứng dạng text, hinh ảnh, video...
    UploadDate DATETIME DEFAULT GETDATE(), -- Ngày upload
    FOREIGN KEY (EvaluationId) REFERENCES StudentEvaluation(EvaluationId),
    FOREIGN KEY (CriteriaId) REFERENCES Criteria(CriteriaId)
);

-- Cấu hình hệ thống: thời hạn đánh giá, thời hạn duyệt
CREATE TABLE EvaluationConfig (
    ConfigId INT PRIMARY KEY IDENTITY(1,1),
    SemesterId INT NOT NULL,
    SelfEvalStart DATETIME, -- Thời gian bắt đầu tự đánh giá
    SelfEvalEnd DATETIME, -- Thời gian kết thúc tự đánh giá
    LecturerEvalStart DATETIME, -- Thời gian bắt đầu giảng viên đánh giá
    LecturerEvalEnd DATETIME, -- Thời gian kết thúc giảng viên đánh giá
    FOREIGN KEY (SemesterId) REFERENCES Semester(SemesterId)
);
SELECT name
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('Notification');
Select * from Notification
-- Thông báo gửi đến người dùng
CREATE TABLE Notification (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT,
    Title NVARCHAR(255), -- Tiêu đề thông báo
    Message NVARCHAR(MAX), -- Nội dung thông báo
    CreatedAt DATETIME DEFAULT GETDATE(), -- Ngày tạo thông báo
    IsRead BIT DEFAULT 0, -- Đã đọc hay chưa
    FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);
ALTER TABLE Notification DROP CONSTRAINT FK__Notificat__Accou__693CA210;

ALTER TABLE Notification
ADD CONSTRAINT FK_Notification_Account
FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
ON DELETE CASCADE;

SELECT 
    fk.name AS FK_Name
FROM 
    sys.foreign_keys fk
WHERE 
    fk.parent_object_id = OBJECT_ID('EvaluationSummary');
Select * from EvaluationSummary
-- Tổng hợp điểm rèn luyện của sinh viên theo học kỳ
CREATE TABLE EvaluationSummary (
    SummaryId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT NOT NULL,
    SemesterId INT NOT NULL,
    TotalScore INT, -- Tổng điểm rèn luyện
    Rank NVARCHAR(50), -- Xếp loại: Xuất sắc, Tốt, Khá, Trung bình, Yếu
    FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
    FOREIGN KEY (SemesterId) REFERENCES Semester(SemesterId)
);
ALTER TABLE EvaluationSummary DROP CONSTRAINT FK_EvaluationSummary_Account;

ALTER TABLE EvaluationSummary
ADD CONSTRAINT FK_EvaluationSummary_Account
FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
ON DELETE CASCADE;

-- Nhật ký hoạt động của hệ thống (có thể dùng cho audit/log nếu cần)
CREATE TABLE SystemLog (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT,
    Action NVARCHAR(255), -- Hành động thực hiện
    LogTime DATETIME DEFAULT GETDATE(), -- Thời gian ghi log
    FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);

SELECT name
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('Attendance')
AND referenced_object_id = OBJECT_ID('Account');
Select * from Attendance
-- Lưu thông tin buổi điểm danh bằng QR code
CREATE TABLE Attendance (
    AttendanceId INT PRIMARY KEY IDENTITY(1,1),
    ClassId INT NOT NULL,
    SessionDate DATE NOT NULL, -- Ngày diễn ra buổi điểm danh
    QRCode NVARCHAR(255), -- mã QR cho buổi điểm danh
    CreatedBy INT, -- -- ID tài khoản lớp trưởng tạo buổi điểm danh
    CreatedAt DATETIME DEFAULT GETDATE(), -- Thời gian tạo bản ghi
    FOREIGN KEY (ClassId) REFERENCES Class(ClassId),
    FOREIGN KEY (CreatedBy) REFERENCES Account(AccountId)
);
ALTER TABLE Attendance DROP CONSTRAINT FK__Attendanc__Creat__75A278F5;

ALTER TABLE Attendance
ADD CONSTRAINT FK_Attendance_CreatedBy
FOREIGN KEY (CreatedBy) REFERENCES Account(AccountId)
ON DELETE CASCADE;

SELECT name
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('AttendanceRecord');
Select * from AttendanceRecord
-- Lưu danh sách sinh viên đã điểm danh
CREATE TABLE AttendanceRecord (
    RecordId INT PRIMARY KEY IDENTITY(1,1),
    AttendanceId INT NOT NULL,
    StudentId INT NOT NULL, -- Khóa ngoại đến Account (sinh viên nào điểm danh)
    CheckInTime DATETIME DEFAULT GETDATE(), -- Thời gian sinh viên điểm danh
    FOREIGN KEY (AttendanceId) REFERENCES Attendance(AttendanceId),
    FOREIGN KEY (StudentId) REFERENCES Account(AccountId)
);
ALTER TABLE AttendanceRecord DROP CONSTRAINT FK__Attendanc__Atten__797309D9;

ALTER TABLE AttendanceRecord
ADD CONSTRAINT FK_AttendanceRecord_Attendance
FOREIGN KEY (AttendanceId) REFERENCES Attendance(AttendanceId)
ON DELETE CASCADE;


select * from MeetingNotification
-- Bảng thông báo họp lớp của chủ nhiệm khoa
CREATE TABLE MeetingNotification (
    MeetingId INT PRIMARY KEY IDENTITY(1,1),
    FacultyId INT NOT NULL,
    ClassId INT NOT NULL,
    AcademicYearId INT NOT NULL,
    SemesterId INT NOT NULL,
    MeetingDate DATE NOT NULL,
    MeetingTime NVARCHAR(20) NOT NULL,
    Location NVARCHAR(255),
    Note NVARCHAR(MAX),
    CreatedBy INT, -- ID Chủ nhiệm khoa
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (FacultyId) REFERENCES Faculty(FacultyId),
    FOREIGN KEY (ClassId) REFERENCES Class(ClassId),
    FOREIGN KEY (AcademicYearId) REFERENCES AcademicYear(AcademicYearId),
    FOREIGN KEY (SemesterId) REFERENCES Semester(SemesterId),
    FOREIGN KEY (CreatedBy) REFERENCES Account(AccountId)
);