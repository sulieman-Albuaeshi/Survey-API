create database SurveyDB collate SQL_Latin1_General_CP1_CI_AS
go

use SurveyDB
go

grant connect on database :: SurveyDB to dbo
go

grant view any column encryption key definition, view any column master key definition on database :: SurveyDB to [public]
go

create table dbo.QuestionType
(
    id           int identity
        constraint QuestionType_pk
            primary key,
    questiontype varchar(40)
)
    go

create table dbo.Users
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

create table dbo.Surveys
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
            references dbo.Users,
    Status        tinyint   default 0            not null,
    QuestionCount int       default 0            not null
)
    go

create table dbo.Questions
(
    Id           int identity
        primary key,
    SurveyId     int           not null
        constraint FK_Questions_Surveys
            references dbo.Surveys
            on delete cascade,
    QuestionText nvarchar(500) not null,
    IsRequired   bit default 1 not null,
    OrderIndex   int default 0 not null,
    SettingsJSON nvarchar(max),
    QuestionType int           not null
        constraint Questions_QuestionType_id_fk
            references dbo.QuestionType
)
    go

create table dbo.Choices
(
    Id           int identity
        primary key,
    QuestionId   int           not null
        constraint FK_Choices_Questions
            references dbo.Questions
            on delete cascade,
    ChoiceText   nvarchar(200) not null,
    OrderIndex   int default 0 not null,
    IsRandomized bit default 0 not null
)
    go

create index IX_Questions_SurveyId
    on dbo.Questions (SurveyId)
    go

create table dbo.Response
(
    Id          int identity
        primary key,
    SurveyId    int                            not null
        constraint FK_Response_Surveys
            references dbo.Surveys
            on delete cascade,
    UserId      nvarchar(450)
        constraint FK_Response_Users
            references dbo.Users
            on delete set null,
    SubmittedAt datetime2 default getutcdate() not null,
    isActive    bit       default 0
)
    go

create table dbo.Answers
(
    Id          int identity
        primary key,
    ResponseId  int               not null
        constraint FK_Answers_Submissions
            references dbo.Response
            on delete cascade,
    QuestionId  int               not null
        constraint FK_Answers_Questions
            references dbo.Questions,
    AnswerType  tinyint default 0 not null,
    AnswerValue nvarchar(100)
)
    go

create table dbo.AnswerSelections
(
    Id        int identity
        primary key,
    AnswerId  int           not null
        constraint FK_AnswerSelections_Answers
            references dbo.Answers
            on delete cascade,
    ChoiceId  int           not null
        constraint FK_AnswerSelections_Choices
            references dbo.Choices,
    RankOrder int default 0 not null
)
    go

create index IX_Answers_SubmissionId
    on dbo.Answers (ResponseId)
    go

create index IX_Submissions_SurveyId
    on dbo.Response (SurveyId)
    go

create index IX_Surveys_CreatorId
    on dbo.Surveys (UserId)
    go

create index IX_Surveys_Status
    on dbo.Surveys (Status)
    go