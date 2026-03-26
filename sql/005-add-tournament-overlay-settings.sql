ALTER TABLE pickem.Tournament
ADD OverlaySettingsJson nvarchar(max) NULL;
GO

UPDATE pickem.Tournament
SET OverlaySettingsJson = '{"hideBlocksByPhase":true,"theme":{"fillColorStart":"#163A61","fillColorEnd":"#0B1F3A","fillOpacity":92,"useGradient":true},"leftPanel":{"edgeOffset":15,"topOffset":138},"rightPanel":{"edgeOffset":15,"topOffset":394},"summaryBlock":{"panel":"left","dynamicDisplay":{"enabled":false,"intervalSeconds":30,"visibleDurationSeconds":8}},"firstVoteBlock":{"panel":"right","dynamicDisplay":{"enabled":false,"intervalSeconds":30,"visibleDurationSeconds":8}},"lastRoundBlock":{"panel":"left","dynamicDisplay":{"enabled":false,"intervalSeconds":30,"visibleDurationSeconds":8}},"footerBlock":{"panel":"left","dynamicDisplay":{"enabled":false,"intervalSeconds":30,"visibleDurationSeconds":8}}}'
WHERE OverlaySettingsJson IS NULL;
GO

ALTER TABLE pickem.Tournament
ALTER COLUMN OverlaySettingsJson nvarchar(max) NOT NULL;
GO