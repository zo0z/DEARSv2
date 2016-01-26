CREATE TABLE [dbo].[Disciplines]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [NameEnglish] NVARCHAR(50) NULL, 
    [NameArabic] NVARCHAR(50) NULL, 
    [NameEnglishShort] NVARCHAR(50) NULL, 
    [NameArabicShort] NVARCHAR(50) NULL
)
