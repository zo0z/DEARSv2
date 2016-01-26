CREATE TABLE [dbo].[GPAwRecomm]
(
	[YearId] INT NOT NULL, 
    [GradeId] INT NOT NULL, 
    [StudentId] INT NOT NULL,
	GPA DECIMAL(5,3),
	YearRecommId INT NOT NULL,
	CGPA DECIMAL(5,3) ,
	CumulativeRecommId INT,
	Comment NVARCHAR(50),
	CONSTRAINT [FK_GPAwRecomm_TimeYears] FOREIGN KEY ([YearId]) REFERENCES [TimeYears]([Id]) ,
    CONSTRAINT [FK_GPAwRecomm_Grades] FOREIGN KEY ([GradeId]) REFERENCES [Grades]([Id]) ,
	CONSTRAINT [FK_GPAwRecomm_Students] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ,
	CONSTRAINT [FK_GPAwRecomm_YearRecommendations] FOREIGN KEY (YearRecommId) REFERENCES RecommendationTypes(Id),
	CONSTRAINT [FK_GPAwRecomm_CumulativeRecommendations] FOREIGN KEY (CumulativeRecommId) REFERENCES RecommendationTypes(Id),
	CONSTRAINT [FK_GPAwRecomm_Batch] FOREIGN KEY (YearId, GradeId) REFERENCES [Batches](YearId, GradeId) , 
	CONSTRAINT [FK_GPAwRecomm_BatchEnrollments] FOREIGN KEY ([YearId], [GradeId], [StudentId]) REFERENCES BatchEnrollments,
	CONSTRAINT [PK_GPAwRecomm] PRIMARY KEY ([YearId], [GradeId], [StudentId])
)
