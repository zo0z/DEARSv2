CREATE TABLE [dbo].[Courses]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [CourseCode] NCHAR(10) NULL UNIQUE, 
    [TitleArabic] NVARCHAR(MAX) NULL, 
    [TitleEnglish] NVARCHAR(MAX) NULL
)