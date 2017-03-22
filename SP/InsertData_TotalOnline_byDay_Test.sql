

alter PROCEDURE[dbo].[InsertData_TotalOnline_byDay_Test]
	@Date smalldatetime = null
AS

SET NOCOUNT ON

BEGIN

	IF(@Date is null)
		set @Date = CONVERT(char(8), DATEADD (day , -1 , CURRENT_TIMESTAMP ) , 112)    

	IF @Date <= Getdate () -30
	Begin
		Return
	End

	DELETE [TotalOnline_byDay] WHERE LogDate >=Convert(char(8),@Date,112) and logdate < Convert(char(8),@Date +1 ,112)
--- A site  ---
	INSERT INTO [dbo].[TotalOnline_byDay](LogDate, Users, SiteName)
	SELECT *
	FROM
	(	SELECT Convert(char(10),logdate,120) as logdate, 
						 max([Total]) as [Total], max([Today]) as [Today], max([Live]) as [Live]
		  			,max([SiteA]) as [SiteA], max([SiteASmart]) as [SiteASmart]
						,max([SiteB]) as [SiteB], max([SiteC]) as [SiteC], max([SiteD]) as [SiteD], max([SiteE]) as [SiteE]

		FROM
		(	SELECT  logdate ,
				sum(case when ServerID = 100001  THEN isnull(users,0) else 0 end) as [Total],
				sum(case when ServerID = 100002  THEN isnull(users,0) else 0 end) as [Today],
				sum(case when ServerID = 100003  THEN isnull(users,0) else 0 end) as [Live],
				sum(case when left(ServerID,3) = 101 and  
							ServerID NOT IN(101111,101112) THEN isnull(users,0) else 0 end) as [SiteA],
				sum(case when ServerID IN (101111,101112) THEN isnull(users,0) else 0 end) as [SiteASmart],
				sum(case when left(ServerID,3) = 102 THEN isnull(users,0) else 0 end) as [SiteB],
				sum(case when left(ServerID,3) = 103 THEN isnull(users,0) else 0 end) as [SiteC],
				sum(case when left(ServerID,3) = 104 THEN isnull(users,0) else 0 end) as [SiteD], 
				sum(case when left(ServerID,3) = 105 THEN isnull(users,0) else 0 end) as [SiteE]
			FROM TotalOnline nolock
			WHERE serverid < 200000 and LogDate >=Convert(char(8),@Date,112) and logdate < Convert(char(8),@Date +1 ,112)
			GROUP BY logdate
		) as a
		GROUP BY Convert(char(10),logdate,120)	
	) as b
	UNPIVOT
	(
		Users FOR SiteName IN ([Total],[Today],[Live],[SiteA],[SiteASmart],[SiteB],[SiteC],[SiteD],[SiteE])
	) as pv
	
--- B site ---
	INSERT INTO [dbo].[TotalOnline_byDay](LogDate, Users, SiteName)
	SELECT Convert(char(10),x.LogDate, 120) as LogDate, max(x.users) as users, s.[site] as SiteName 
	FROM (	SELECT LogDate,  SUM(users) as users, left(ServerID ,3) as serverID
			FROM TotalOnline nolock
			WHERE serverid > 199999 and LogDate >=Convert(char(8),@Date,112) and logdate < Convert(char(8),@Date +1 ,112)
			GROUP BY logdate ,left(serverID ,3) 
		  ) x 
	INNER JOIN bodb02.dbo.site s ON left(s.serverID ,3) = x.serverID
	WHERE s.isActive = 1
	GROUP BY s.[site] , Convert(char(10),x.logdate,120)	
END

exec Dot_Admin_InsertData_TotalOnline_byDay_Test
