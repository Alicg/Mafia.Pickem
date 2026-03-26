ALTER TABLE pickem.Tournament
ADD OverlaySettingsJson nvarchar(max) NULL;
GO

UPDATE pickem.Tournament
SET OverlaySettingsJson = '{"hideBlocksByPhase":true,"theme":{"fillColorStart":"#163A61","fillColorEnd":"#0B1F3A","fillOpacity":92,"useGradient":true},"summaryBlock":{"side":"left","edgeOffset":15,"topOffset":138},"firstVoteBlock":{"side":"right","edgeOffset":15,"topOffset":394},"lastRoundBlock":{"side":"left","edgeOffset":15,"topOffset":394},"footerBlock":{"side":"left","edgeOffset":15,"topOffset":736}}'
WHERE OverlaySettingsJson IS NULL;
GO

ALTER TABLE pickem.Tournament
ALTER COLUMN OverlaySettingsJson nvarchar(max) NOT NULL;
GO