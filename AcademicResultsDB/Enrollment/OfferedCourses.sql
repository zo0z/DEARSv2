CREATE TABLE [dbo].[OfferedCourses]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[CourseId] INT NOT NULL,
	ExamFraction INT NOT NULL,
	CourseWorkFraction INT NOT NULL,
	CreditHours INT NOT NULL,

	CONSTRAINT [FK_OfferedCourses_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_OfferedCourses_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_OfferedCourses_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_OfferedCourses_Courses] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id]) ,
	CONSTRAINT [FK_OfferedCourses_SemesterBatch] FOREIGN KEY (YearId, GradeId, SemesterId) REFERENCES [SemesterBatches](YearId, GradeId, SemesterId) , 
    CONSTRAINT [PK_OfferedCourses] PRIMARY KEY ([YearId], [GradeId], [SemesterId], [CourseId])
)
