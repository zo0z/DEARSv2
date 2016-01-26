CREATE TABLE [dbo].[SemesterBatches]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	CONSTRAINT PK_SemesterBatch PRIMARY KEY (YearId, GradeId, SemesterId),
	CONSTRAINT [FK_SemesterBatch_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_SemesterBatch_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_SemesterBatch_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_SemesterBatch_Batch] FOREIGN KEY (YearId, GradeId) REFERENCES [Batches](YearId, GradeId) , 
)
