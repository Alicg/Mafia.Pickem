USE [MafiaPickem];
GO

ALTER TABLE pickem.PickemUser
ADD TelegramUsername nvarchar(255) NULL;
GO

CREATE UNIQUE INDEX UQ_PickemUser_TelegramUsername
    ON pickem.PickemUser (TelegramUsername)
    WHERE TelegramUsername IS NOT NULL;
GO

CREATE TABLE pickem.TournamentOperator
(
    Id              int identity    NOT NULL
        CONSTRAINT PK_TournamentOperator PRIMARY KEY,
    TournamentId    int             NOT NULL
        CONSTRAINT FK_TournamentOperator_Tournament
            REFERENCES pickem.Tournament (Id)
            ON DELETE CASCADE,
    OperatorUsername nvarchar(255)  NOT NULL,
    DateCreated     datetime2(0)    DEFAULT sysdatetime() NOT NULL,

    CONSTRAINT UQ_TournamentOperator_Tournament_User
        UNIQUE (TournamentId, OperatorUsername)
);
GO

CREATE INDEX IX_TournamentOperator_UserId_TournamentId
    ON pickem.TournamentOperator (OperatorUsername, TournamentId);
GO