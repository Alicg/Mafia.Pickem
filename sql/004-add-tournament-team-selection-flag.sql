USE [MafiaPickem];
GO

ALTER TABLE pickem.Tournament
ADD ShowTeamSelection bit NOT NULL
    CONSTRAINT DF_Tournament_ShowTeamSelection DEFAULT 1;
GO