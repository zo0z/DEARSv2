CREATE TABLE [dbo].[Students]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [Index] INT NULL, 
    [UnivNo] NCHAR(10) NULL, 
    [NameArabic] NVARCHAR(MAX) NULL, 
    [NameEnglish] NVARCHAR(MAX) NULL
)
