-- 1. Create the database
CREATE DATABASE SurveyDB;
GO

-- 2. Tell SQL Server to use this new database
USE SurveyDB;
GO
create table QuestionType
(
    id           int identity
        constraint QuestionType_pk
            primary key,
    questiontype varchar(40)
)
    go

create table Users
(
    Id        nvarchar(450)                  not null
        primary key,
    FullName  nvarchar(150)                  not null,
    Email     nvarchar(256)                  not null
        unique,
    CreatedAt datetime2 default getutcdate() not null,
    usename   nvarchar(50)
        constraint Users_pk
            unique
)
    go

create table Surveys
(
    Id            int identity
        primary key,
    Title         nvarchar(200)                  not null,
    Description   nvarchar(max),
    IsActive      bit       default 1            not null,
    CreatedAt     datetime2 default getutcdate() not null,
    IsAnonymous   bit       default 0            not null,
    UserId        nvarchar(450)                  not null
        constraint FK_Surveys_Users
            references Users,
    Status        tinyint   default 0            not null,
    QuestionCount int       default 0            not null
)
    go

create table Questions
(
    Id           int identity
        primary key,
    SurveyId     int           not null
        constraint FK_Questions_Surveys
            references Surveys
            on delete cascade,
    QuestionText nvarchar(500) not null,
    IsRequired   bit default 1 not null,
    OrderIndex   int default 0 not null,
    SettingsJSON nvarchar(max),
    QuestionType int           not null
        constraint Questions_QuestionType_id_fk
            references QuestionType
)
    go

create table Choices
(
    Id           int identity
        primary key,
    QuestionId   int           not null
        constraint FK_Choices_Questions
            references Questions
            on delete cascade,
    ChoiceText   nvarchar(200) not null,
    OrderIndex   int default 0 not null,
    IsRandomized bit default 0 not null
)
    go

create index IX_Questions_SurveyId
    on Questions (SurveyId)
    go

create table Response
(
    Id          int identity
        primary key,
    SurveyId    int                            not null
        constraint FK_Response_Surveys
            references Surveys
            on delete cascade,
    UserId      nvarchar(450)
        constraint FK_Response_Users
            references Users
            on delete set null,
    SubmittedAt datetime2 default getutcdate() not null,
    isActive    bit       default 0
)
    go

create table Answers
(
    Id          int identity
        primary key,
    ResponseId  int               not null
        constraint FK_Answers_Submissions
            references Response
            on delete cascade,
    QuestionId  int               not null
        constraint FK_Answers_Questions
            references Questions,
    TextValue   nvarchar(max) sparse,
    NumberValue decimal(18, 4) sparse,
    DateValue   datetime2 sparse,
    AnswerType  tinyint default 0 not null
)
    go

create table AnswerSelections
(
    Id        int identity
        primary key,
    AnswerId  int           not null
        constraint FK_AnswerSelections_Answers
            references Answers
            on delete cascade,
    ChoiceId  int           not null
        constraint FK_AnswerSelections_Choices
            references Choices,
    RankOrder int default 0 not null
)
    go

create index IX_Answers_SubmissionId
    on Answers (ResponseId)
    go

create index IX_Submissions_SurveyId
    on Response (SurveyId)
    go

create index IX_Surveys_CreatorId
    on Surveys (UserId)
    go

create index IX_Surveys_Status
    on Surveys (Status)
    go

