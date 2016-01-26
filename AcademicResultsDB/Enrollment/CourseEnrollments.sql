CREATE TABLE [dbo].[CourseEnrollments]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[StudentId] INT NOT NULL,
	[CourseId] INT NOT NULL, 
    CONSTRAINT [PK_CourseEnrollments] PRIMARY KEY ([YearId], [GradeId], [SemesterId], [StudentId], [CourseId]),

	CONSTRAINT [FK_CourseEnrollments_OfferedCourses] FOREIGN KEY (YearId, GradeId, SemesterId, CourseId)
			REFERENCES OfferedCourses (YearId, GradeId, SemesterId, CourseId) ,
	CONSTRAINT [FK_CourseEnrollments_SemesterBatchEnrollments] FOREIGN KEY (YearId, GradeId, SemesterId, StudentId)
			REFERENCES SemesterBatchEnrollments (YearId, GradeId, SemesterId, StudentId) ,

	CONSTRAINT [FK_CourseEnrollments_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_CourseEnrollments_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_CourseEnrollments_Students] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ,
	CONSTRAINT [FK_CourseEnrollments_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_CourseEnrollments_Courses] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id]) 
)
