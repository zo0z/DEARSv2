CREATE TABLE [dbo].[BatchEnrollments]
(
    [YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [StudentId] INT NOT NULL,
	EnrollmentTypeId INT NOT NULL DEFAULT 0,
	CONSTRAINT [FK_BatchEnrollments_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_BatchEnrollments_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_BatchEnrollments_Students] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ,
	CONSTRAINT [FK_BatchEnrollments_EnrollmentType] FOREIGN KEY ([EnrollmentTypeId]) REFERENCES EnrollmentTypes([Id]) ,
	CONSTRAINT [FK_BatchEnrollments_Batch] FOREIGN KEY (YearId, GradeId) REFERENCES [Batches](YearId, GradeId) , 
    CONSTRAINT [PK_BatchEnrollments] PRIMARY KEY ([YearId], [GradeId], [StudentId])
)
