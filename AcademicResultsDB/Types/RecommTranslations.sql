CREATE TABLE [dbo].[RecommTranslations]
(
	[ResText] NVARCHAR(50) PRIMARY KEY,
	[RecommendationTypeN] INT,

	CONSTRAINT RecommTranslations_RecommendationTypes FOREIGN KEY (RecommendationTypeN) REFERENCES RecommendationTypes(Id)
)
