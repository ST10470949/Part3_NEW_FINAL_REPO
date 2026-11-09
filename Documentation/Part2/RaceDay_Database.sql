/* =====================================================================
   RaceDay Database Script - Part 2
   SQL Server / SQL Server Express / LocalDB
   Part 2 schema with Part 3 image URL fields added for the MVC/Azure extension.
   ===================================================================== */

IF DB_ID(N'RaceDay') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role         NVARCHAR(20) NOT NULL,
    CreatedAt    DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    ProfilePictureUrl NVARCHAR(500) NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant'))
);
GO

CREATE TABLE Events (
    EventID     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    OrganiserID INT NOT NULL,
    Name        NVARCHAR(150) NOT NULL,
    EventDate   DATE NOT NULL,
    Location    NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DistanceKm  DECIMAL(6,2) NOT NULL,
    EventType   NVARCHAR(20) NOT NULL,
    BannerImageUrl NVARCHAR(500) NULL,
    CreatedAt   DATETIME2 NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
        ON DELETE NO ACTION
);
GO

CREATE TABLE Categories (
    CategoryID      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    EventID         INT NOT NULL,
    Name            NVARCHAR(50) NOT NULL,
    EntryFee        DECIMAL(8,2) NOT NULL CONSTRAINT DF_Categories_EntryFee DEFAULT 0,
    MaxParticipants INT NOT NULL CONSTRAINT DF_Categories_MaxParticipants DEFAULT 100,
    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventID) REFERENCES Events(EventID)
        ON DELETE CASCADE
);
GO

CREATE TABLE RouteWeatherInfo (
    RouteInfoID     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RouteWeatherInfo PRIMARY KEY,
    EventID         INT NOT NULL,
    WeatherForecast NVARCHAR(255) NULL,
    RouteMapURL     NVARCHAR(255) NULL,
    UpdatedAt       DATETIME2 NOT NULL CONSTRAINT DF_RouteWeatherInfo_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_RouteWeatherInfo_Event UNIQUE (EventID),
    CONSTRAINT FK_RouteWeatherInfo_Event
        FOREIGN KEY (EventID) REFERENCES Events(EventID)
        ON DELETE CASCADE
);
GO

CREATE TABLE Enrolments (
    EnrolmentID   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enrolments PRIMARY KEY,
    ParticipantID INT NOT NULL,
    CategoryID    INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL CONSTRAINT DF_Enrolments_EnrolmentDate DEFAULT SYSUTCDATETIME(),
    Status        NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT 'Confirmed',
    CONSTRAINT UQ_Enrolment UNIQUE (ParticipantID, CategoryID),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Pending','Confirmed','Cancelled')),
    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantID) REFERENCES Users(UserID)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
        ON DELETE CASCADE
);
GO

CREATE TABLE Results (
    ResultID     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Results PRIMARY KEY,
    EnrolmentID  INT NOT NULL,
    CapturedByID INT NOT NULL,
    FinishTime   TIME NULL,
    Position     INT NULL,
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentID),
    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
        ON DELETE CASCADE,
    CONSTRAINT FK_Results_CapturedBy
        FOREIGN KEY (CapturedByID) REFERENCES Users(UserID)
        ON DELETE NO ACTION,
    CONSTRAINT CK_Results_Position CHECK (Position IS NULL OR Position > 0)
);
GO

/* ---------------------------------------------------------------------
   Optional sample data.
   The sample accounts use a valid ASP.NET Identity v3 password hash.
   Sample password for all four accounts: RaceDay123!
   Change/remove these accounts for a real deployment.
   --------------------------------------------------------------------- */
INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES
('Thandiwe Mokoena', 'thandiwe.mokoena@raceday.co.za', 'AQIAAACghgEAEAAAACtBY8FjGG5OxiWGVOb9BjoCnL6/rjkHg3CKmI5HmC9W/zITEwBhYOfNvA3l6GGaYQ==', 'Organiser'),
('Johan van der Merwe', 'johan.vdm@raceday.co.za', 'AQIAAACghgEAEAAAAN0TqnjnpvIV0hp63V2CtxKmLseZd4nHkIFbWgB6wRo6PHCkCULgo4W3nXnxdyC9pw==', 'Organiser'),
('Lindiwe Dlamini', 'lindiwe.dlamini@example.com', 'AQIAAACghgEAEAAAAPfRFuddLugLG3jhLH1zq7tQhZ42d7cfORuXydDqjck21dtsm3rVa1asNT5SUucmhQ==', 'Participant'),
('Sipho Nkosi', 'sipho.nkosi@example.com', 'AQIAAACghgEAEAAAADgJMLYV2dKNLwpdlZlo3CRDUJ52+h/CHX3d2r9XRD6oqpTTtSwFrmR4x6gVul73nw==', 'Participant');
GO

INSERT INTO Events
    (OrganiserID, Name, EventDate, Location, Description, DistanceKm, EventType)
VALUES
    (1, 'Cape Town Cycle Tour', '2026-11-08', 'Cape Town',
     'Iconic cycling event around the Cape Peninsula.', 109.00, 'Cycle'),
    (1, 'Soweto Marathon', '2026-11-01', 'Soweto',
     'Road running event through the historic streets of Soweto.', 42.20, 'Run'),
    (2, 'Durban Beachfront Park Run', '2026-09-19', 'Durban',
     'Community 5km fun run along the Durban beachfront.', 5.00, 'Run');
GO

INSERT INTO Categories (EventID, Name, EntryFee, MaxParticipants) VALUES
(1, '109km Cycle', 650.00, 15000),
(1, '45km Cycle', 450.00, 5000),
(2, '42.2km Marathon', 350.00, 8000),
(2, '10km Fun Run', 150.00, 4000),
(3, '5km Park Run', 0.00, 1000);
GO

INSERT INTO Enrolments (ParticipantID, CategoryID, Status) VALUES
(3, 1, 'Confirmed'),
(4, 3, 'Confirmed'),
(3, 5, 'Confirmed');
GO

INSERT INTO Results (EnrolmentID, CapturedByID, FinishTime, Position) VALUES
(1, 1, '04:12:35', 245),
(2, 1, '03:45:10', 118);
GO

PRINT 'RaceDay database created successfully.';
GO
