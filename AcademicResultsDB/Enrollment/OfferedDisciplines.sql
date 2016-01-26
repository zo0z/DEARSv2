CREATE TABLE [dbo].[OfferedDisciplines]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[DisciplineId] INT NOT NULL,
	CONSTRAINT [FK_OfferedDisciplines_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_OfferedDisciplines_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_OfferedDisciplines_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_OfferedDisciplines_Disciplines] FOREIGN KEY ([DisciplineId]) REFERENCES [Disciplines]([Id]) ,
	CONSTRAINT [FK_OfferedDisciplines_SemesterBatch] FOREIGN KEY (YearId, GradeId, SemesterId) REFERENCES [SemesterBatches](YearId, GradeId, SemesterId) , 
    CONSTRAINT [PK_OfferedDisciplines] PRIMARY KEY ([YearId], [GradeId], [SemesterId],  [DisciplineId])
)
