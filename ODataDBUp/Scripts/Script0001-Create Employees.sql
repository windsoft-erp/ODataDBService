IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Employees](
	[EmployeeID] [int] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Title] [nvarchar](50) NULL,
	[BirthDate] [date] NULL,
	[HireDate] [date] NULL,
	[City] [nvarchar](50) NULL,
	[Country] [nvarchar](50) NULL,
	[TotalOrders] [int] NOT NULL DEFAULT 0
 CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED 
(
	[EmployeeID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
END

INSERT INTO [dbo].[Employees] (EmployeeID, FirstName, LastName, Title, BirthDate, HireDate, City, Country)
VALUES
(1, 'Nancy', 'Davolio', 'Sales Representative', '1968-12-08', '1992-05-01', 'Seattle', 'USA'),
(2, 'Andrew', 'Fuller', 'Vice President, Sales', '1972-02-19', '1992-08-14', 'Tacoma', 'USA'),
(3, 'Janet', 'Leverling', 'Sales Representative', '1985-08-30', '1992-04-01', 'Kirkland', 'USA'),
(4, 'Margaret', 'Peacock', 'Sales Representative', '1973-09-19', '1993-05-03', 'Redmond', 'USA'),
(5, 'Steven', 'Buchanan', 'Sales Manager', '1955-03-04', '1993-10-17', 'London', 'UK'),
(6, 'Michael', 'Suyama', 'Sales Representative', '1983-07-02', '1993-10-17', 'London', 'UK'),
(7, 'Robert', 'King', 'Sales Representative', '1960-05-29', '1994-01-02', 'London', 'UK'),
(8, 'Laura', 'Callahan', 'Inside Sales Coordinator', '1978-01-09', '1994-03-05', 'Seattle', 'USA'),
(9, 'Anne', 'Dodsworth', 'Sales Representative', '1986-01-27', '1994-11-15', 'London', 'UK'),
(10, 'Andrew', 'Smith', 'Marketing Manager', '1972-05-16', '1995-01-01', 'New York', 'USA'),
(11, 'Bradley', 'Zimmer', 'Sales Representative', '1975-01-18', '1995-02-01', 'New York', 'USA'),
(12, 'David', 'Pitt', 'Sales Representative', '1981-05-29', '1995-02-05', 'New York', 'USA'),
(13, 'Peter', 'Wilson', 'Sales Representative', '1982-07-09', '1995-03-10', 'New York', 'USA'),
(14, 'Susan', 'Tingley', 'Sales Representative', '1976-12-15', '1995-03-11', 'New York', 'USA'),
(15, 'William', 'Gibson', 'Sales Representative', '1980-12-17', '1995-05-16', 'London', 'UK')