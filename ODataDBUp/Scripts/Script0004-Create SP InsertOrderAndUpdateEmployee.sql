IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertOrderAndUpdateEmployee]') AND type in (N'P'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[InsertOrderAndUpdateEmployee] AS BEGIN SELECT 1 END')
END
GO

ALTER PROCEDURE [dbo].[InsertOrderAndUpdateEmployee]
    @EmployeeID INT,
    @OrderDate DATE,
    @ShipName NVARCHAR(50),
    @ShipCity NVARCHAR(50),
    @ShipCountry NVARCHAR(50),
    @Value INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if the employee exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [EmployeeID] = @EmployeeID)
    BEGIN
        RAISERROR('Employee not found.', 16, 1)
        RETURN;    
    END

    -- Insert the order
    INSERT INTO [dbo].[Orders] ([EmployeeID], [OrderDate], [ShipName], [ShipCity], [ShipCountry], [Value])
    VALUES (@EmployeeID, @OrderDate, @ShipName, @ShipCity, @ShipCountry, @Value)

    -- Update the employee's total orders
    UPDATE [dbo].[Employees]
    SET [TotalOrders] = [TotalOrders] + 1
    WHERE [EmployeeID] = @EmployeeID

END
GO