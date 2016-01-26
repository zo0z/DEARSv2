CREATE TABLE [dbo].[SemesterBatchEnrollments]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[StudentId] INT NOT NULL,
	[DisciplineId] INT NOT NULL, 
    CONSTRAINT [PK_SemesterBatchEnrollments] PRIMARY KEY ([YearId], [GradeId], [SemesterId], [StudentId]),
	CONSTRAINT [FK_SemesterBatchEnrollments_SemesterBatches] FOREIGN KEY ([YearId], [GradeId], [SemesterId])
			REFERENCES SemesterBatches ([YearId], [GradeId], [SemesterId]) ,
	CONSTRAINT [FK_SemesterBatchEnrollments_OfferedDisciplines] FOREIGN KEY ([YearId], [GradeId], [SemesterId], [DisciplineId])
			REFERENCES OfferedDisciplines ([YearId], [GradeId], [SemesterId], [DisciplineId]) ,
	CONSTRAINT [FK_SemesterBatchEnrollments_BatchEnrollments] FOREIGN KEY (YearId, GradeId, StudentId)
			REFERENCES BatchEnrollments (YearId, GradeId, StudentId) ,
	CONSTRAINT [FK_SemesterBatchEnrollments_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_SemesterBatchEnrollments_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_SemesterBatchEnrollments_Students] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ,
	CONSTRAINT [FK_SemesterBatchEnrollments_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_SemesterBatchEnrollments_Disciplines] FOREIGN KEY ([DisciplineId]) REFERENCES [Disciplines]([Id]) 
)
