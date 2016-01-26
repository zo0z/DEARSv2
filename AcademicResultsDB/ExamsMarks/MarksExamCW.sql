CREATE TABLE [dbo].[MarksExamCW]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [SemesterId] INT NOT NULL,
	[CourseId] INT NOT NULL,
	[StudentId] INT NOT NULL,

    [CWMark] DECIMAL(5,2) NULL,
	Present BIT NULL,
	Excuse BIT NULL,
	ExamMark DECIMAL(5,2) NULL,

	CONSTRAINT [FK_MarksExamCW_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_MarksExamCW_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_MarksExamCW_Students] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ,
	CONSTRAINT [FK_MarksExamCW_Semesters] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id]) ,
	CONSTRAINT [FK_MarksExamCW_Courses] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id]),
	--CONSTRAINT [FK_MarksExamCW_ExamTypes] FOREIGN KEY ([ExamTypeId]) REFERENCES [ExamTypes]([Id]),

	CONSTRAINT [FK_MarksExamCW_CourseEnrollment] FOREIGN KEY ([YearId], [GradeId], [SemesterId], [StudentId], [CourseId]) 
			REFERENCES CourseEnrollments ([YearId], [GradeId], [SemesterId], [StudentId], [CourseId]),
	CONSTRAINT [FK_MarksExamCW_OfferedCourse] FOREIGN KEY ([YearId], [GradeId], [SemesterId], [CourseId])
			REFERENCES OfferedCourses ([YearId], [GradeId], [SemesterId], [CourseId]),
	CONSTRAINT [PK_MarksExamCW] PRIMARY KEY ([YearId], [GradeId], [SemesterId], [StudentId], [CourseId]),

	--CONSTRAINT NoMarkIfAbsent CHECK ( 
	--			NOT ((Present = 0) AND (ExamMark Is NULL)))
)
