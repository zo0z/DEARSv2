CREATE TABLE [dbo].[Batches]
(
	--[Id] INT NOT NULL IDENTITY(1,1),
    [YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL,
	NameEnglish NVARCHAR(50),
	NameArabic NVARCHAR(50),
	CONSTRAINT [FK_Batch_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_Batch_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) , 
	CONSTRAINT [PK_Batch1] PRIMARY KEY (YearId, GradeId),
 --   CONSTRAINT [PK_Batch] PRIMARY KEY (Id),
	--CONSTRAINT [PK_Batch_Id] UNIQUE(YearId, GradeId),
)
