CREATE TABLE [dbo].[CourseDisciplines]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[CourseId] INT NOT NULL,
	[DisciplineId] INT NOT NULL,
	Optional BIT NOT NULL,

	CONSTRAINT [FK_CourseDisciplines_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_CourseDisciplines_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_CourseDisciplines_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_CourseDisciplines_Courses] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id]) ,
	CONSTRAINT [FK_CourseDisciplines_Disciplines] FOREIGN KEY ([DisciplineId]) REFERENCES [Disciplines]([Id]) ,

	CONSTRAINT [FK_CourseDisciplines_SemesterBatch] FOREIGN KEY (YearId, GradeId, SemesterId) 
			REFERENCES [SemesterBatches](YearId, GradeId, SemesterId) , 
	CONSTRAINT [FK_CourseDisciplines_OfferedCourses] FOREIGN KEY (YearId, GradeId, SemesterId, CourseId)
			REFERENCES OfferedCourses (YearId, GradeId, SemesterId, CourseId) ,
	CONSTRAINT [FK_CourseDisciplines_OfferedDisciplines] FOREIGN KEY ([YearId], [GradeId], [SemesterId], [DisciplineId])
			REFERENCES OfferedDisciplines ([YearId], [GradeId], [SemesterId], [DisciplineId]) ,

    CONSTRAINT [PK_CourseDisciplines] PRIMARY KEY ([YearId], [GradeId], [SemesterId], [CourseId], DisciplineId)
)
