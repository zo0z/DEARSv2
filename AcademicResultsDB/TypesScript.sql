INSERT [dbo].[ExamTypes] ([Id], [NameArabic], [NameEnglish]) 
VALUES 
(1, N'النظامي', N'Regular'),
(2, N'ملاحق و بديل', N'Subs/Supp')

INSERT [dbo].[EnrollmentTypes] ([Id], NameArabic, [NameEnglish]) 
VALUES 
(1, N'نظامي', N'Regular'),
(2, N'تحويل', N'Transfer'),
(3, N'خارجي', N'External'),
(4, N'إعادة', N'Repeat'),
(5, N'إعادة جلوس', N'Resit')

INSERT [dbo].[RecommendationTypes] ([Id], NameArabic, NameEnglish, [ShortNameEnglish], [Passed]) 
VALUES 
(1, N'مرتبة الشرف اللأولى', N'First Degree Honour', N'I', 1),
(2, N'مرتبة الشرف الثانية - القسم الأول', N'Second Degree Honour - First Division', N'II-1', 1),
(3, N'مرتبة الشرف الثانية - القسم الثاني', N'Second Degree Honour - Second Division', N'II-2', 1),
(4, N'مرتبة الشرف الثالثة', N'Third Degree Honour', N'III', 1),
(5, N'نجاح', N'Passed', N'Passed', 1),
(6, N'إعادة', N'Repeat', N'Repeat', 0),
(7, N'رسوب', N'Failed', N'Failed', 0),
(8, N'بديل', N'Substitutes', N'Subs', 0),
(9, N'ملحق', N'Supplementary', N'Supp.', 0),
(10, N'بديل\ملحق', N'Substitute/Supplementary', N'Subs./Supp.', 0),
(11, N'إعادة جلوس', N'Resit', N'Resit', 0),
(12, N'معدل ضعيف', N'Weak GPA', N'WGPA', 0),
(13, N'حالة خاصة', N'Special Case', N'Special Case', 0),
(14, N'تجميد', N'Suspend', N'Suspend', 0),
(15, N'فصل', N'Dismiss', N'Dismiss', 0),
(16, N'عام بديل', N'Substitute Year', N'Sub. Year', 0)

INSERT [dbo].[Batches] ([YearId], [GradeId]) 
VALUES 
(2010, 1),
(2010, 2),
(2010, 3),
(2010, 4),
(2010, 5)


INSERT [dbo].[SemesterBatches] ([YearId], [GradeId], [SemesterId]) 
VALUES 
(2010, 1, 1),
(2010, 2, 1),
(2010, 3, 1),
(2010, 4, 1),
(2010, 5, 1)


INSERT [dbo].BatchEnrollments ([YearId], [GradeId], [StudentId]) 
VALUES 
(2010, 5, 1),
(2010, 5, 2),
(2010, 1, 1),
(2010, 2, 1),
(2010, 3, 1),
(2010, 4, 1),
(2010, 5, 1)


INSERT [dbo].[SemesterBatchEnrollments] ([YearId], [GradeId], [SemesterId], [StudentId], [DisciplineId]) 
VALUES 
(2010, 5, 1, 1, 2),
(2010, 5, 1, 2, 3)


INSERT [dbo].[OfferedDisciplines] ([YearId], [GradeId], [SemesterId], [DisciplineId]) 
VALUES 
(2010, 1, 1, 1),
(2010, 2, 1, 1),
(2010, 3, 1, 1),
(2010, 4, 1, 3),
(2010, 5, 1, 2),
(2010, 5, 1, 3),
(2010, 5, 1, 4),
(2010, 5, 1, 5),
(2010, 5, 1, 6)

INSERT [dbo].[OfferedCourses] ([YearId], [GradeId], [SemesterId], [CourseId], [ExamFraction], [CourseWorkFraction], [CreditHours]) 
VALUES 
(2010, 1, 1, 3, 0, 0, 0) ,
(2010, 1, 1, 6, 0, 0, 0) ,
(2010, 1, 1, 7, 0, 0, 0) ,
(2010, 1, 1, 10, 0, 0, 0),
(2010, 1, 1, 11, 0, 0, 0),
(2010, 1, 1, 83, 0, 0, 0),
(2010, 1, 1, 84, 0, 0, 0),
(2010, 1, 1, 98, 0, 0, 0),
(2010, 1, 1, 99, 0, 0, 0),
(2010, 1, 1, 132, 0, 0, 0),
(2010, 2, 1, 4, 0, 0, 0) ,
(2010, 2, 1, 14, 0, 0, 0),
(2010, 2, 1, 15, 0, 0, 0),
(2010, 2, 1, 16, 0, 0, 0),
(2010, 2, 1, 60, 0, 0, 0),
(2010, 2, 1, 61, 0, 0, 0),
(2010, 2, 1, 62, 0, 0, 0),
(2010, 2, 1, 86, 0, 0, 0),
(2010, 2, 1, 95, 0, 0, 0),
(2010, 2, 1, 96, 0, 0, 0),
(2010, 5, 1, 42, 0, 0, 0),
(2010, 5, 1, 44, 0, 0, 0),
(2010, 5, 1, 45, 0, 0, 0)


INSERT [dbo].[CourseDisciplines] ([YearId], [GradeId], [SemesterId], [CourseId], [DisciplineId], [Optional]) 
VALUES 
(2010, 1, 1, 3, 1, 0),
(2010, 1, 1, 6, 1, 0) ,
(2010, 1, 1, 7, 1, 0) ,
(2010, 1, 1, 10, 1, 0),
(2010, 1, 1, 11, 1, 0),
(2010, 1, 1, 83, 1, 0),
(2010, 1, 1, 84, 1, 0),
(2010, 1, 1, 98, 1, 0),
(2010, 1, 1, 99, 1, 0),
(2010, 1, 1, 132, 1, 0),
(2010, 2, 1, 4, 1, 0),
(2010, 2, 1, 14, 1, 0),
(2010, 2, 1, 15, 1, 0),
(2010, 2, 1, 16, 1, 0),
(2010, 2, 1, 60, 1, 0),
(2010, 2, 1, 61, 1, 0),
(2010, 2, 1, 62, 1, 0),
(2010, 2, 1, 86, 1, 0),
(2010, 2, 1, 95, 1, 0),
(2010, 2, 1, 96, 1, 0),
(2010, 5, 1, 42, 2, 0),
(2010, 5, 1, 42, 3, 0),
(2010, 5, 1, 44, 2, 0),
(2010, 5, 1, 45, 2, 0)

