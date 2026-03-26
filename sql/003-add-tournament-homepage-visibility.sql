USE [MafiaPickem];
GO

ALTER TABLE pickem.Tournament
ADD VisibleOnHomePage bit NOT NULL
    CONSTRAINT DF_Tournament_VisibleOnHomePage DEFAULT 1;
GO