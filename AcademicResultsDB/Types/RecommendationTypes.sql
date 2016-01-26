CREATE TABLE [dbo].[RecommendationTypes]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [NameArabic] NVARCHAR(50) NULL, 
    [NameEnglish] NVARCHAR(50) NULL, 
    [Pass] BIT NOT NULL, 
    [ShortNameEnglish] NVARCHAR(50) NULL
)
