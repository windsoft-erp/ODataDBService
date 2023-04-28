IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[EmployeeOrdersView]'))
BEGIN
    EXECUTE ('CREATE VIEW [dbo].[EmployeeOrdersView] AS
    SELECT 
        o.OrderID,
        o.OrderDate,
        o.ShipName,
        o.ShipCity,
        o.ShipCountry,
        o.Value,
        e.EmployeeID,
        e.FirstName,
        e.LastName,
        e.Title
    FROM Orders o
    JOIN Employees e
    ON o.EmployeeID = e.EmployeeID')
END