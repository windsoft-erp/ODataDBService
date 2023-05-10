IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEmployeesByCity]') AND type in (N'P'))
BEGIN
EXEC('CREATE PROCEDURE [dbo].[GetEmployeesByCity] AS BEGIN SELECT 1 END')
END
GO

ALTER PROCEDURE [dbo].[GetEmployeesByCity]
    @City NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get all employees from the specified city
    SELECT * FROM [dbo].[Employees] WHERE [City] = @City
END
GO