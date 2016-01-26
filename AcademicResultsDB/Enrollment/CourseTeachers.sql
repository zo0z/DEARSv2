CREATE TABLE [dbo].[CourseTeachers]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[CourseId] INT NOT NULL,
	[TeacherId] INT NOT NULL,
	[TuitionTypeId] INT NOT NULL,
	CONSTRAINT [FK_CourseTeachers_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_CourseTeachers_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_CourseTeachers_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_CourseTeachers_Disciplines] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id]) ,
	CONSTRAINT [FK_CourseTeachers_Teachers] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers]([Id]) ,
	CONSTRAINT [FK_CourseTeachers_TuitionType] FOREIGN KEY ([TuitionTypeId]) REFERENCES [TuitionTypes]([Id]) ,
	CONSTRAINT [FK_CourseTeachers_OfferedCourses] FOREIGN KEY ([YearId], [GradeId], [SemesterId], [CourseId])
			REFERENCES OfferedCourses ([YearId], [GradeId], [SemesterId], [CourseId]) , 
    CONSTRAINT [PK_CourseTeachers] PRIMARY KEY ([YearId], [GradeId], [SemesterId], [CourseId], [TeacherId], [TuitionTypeId])
)
