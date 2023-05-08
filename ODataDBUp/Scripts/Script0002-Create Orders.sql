IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[Orders](
									   [OrderID] [int] NOT NULL,
									   [EmployeeID] [int] NOT NULL,
									   [OrderDate] [date] NULL,
									   [ShipName] [nvarchar](50) NOT NULL,
									   [ShipCity] [nvarchar](50) NOT NULL,
									   [ShipCountry] [nvarchar](50) NOT NULL,
									   [Value] [int] NOT NULL,
									   CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED
										   (
											[OrderID] ASC
											   ) ON [PRIMARY]
		) ON [PRIMARY]
	END

INSERT INTO [dbo].[Orders] (OrderID, EmployeeID, OrderDate, ShipName, ShipCity, ShipCountry, Value)
VALUES
	(1, 1, '1996-07-04', 'Vins et alcools Chevalier', 'Reims', 'France', 100),
	(2, 2, '1996-07-05', 'Toms Spezialit�ten', 'M�nster', 'Germany', 200),
	(3, 3, '1996-07-08', 'Hanari Carnes', 'Rio de Janeiro', 'Brazil', 150),
	(4, 4, '1996-07-08', 'Victuailles en stock', 'Lyon', 'France', 175),
	(5, 5, '1996-07-09', 'Rattlesnake Canyon Grocery', 'Albuquerque', 'USA', 225),
	(6, 6, '1996-07-10', 'Ernst Handel', 'Graz', 'Austria', 125),
	(7, 7, '1996-07-11', 'Que Del�cia', 'Rio de Janeiro', 'Brazil', 75),
	(8, 8, '1996-07-12', 'Ricardo Adocicados', 'S�o Paulo', 'Brazil', 150),
	(9, 9, '1996-07-15', 'HILARI�N-Abastos', 'San Crist�bal', 'Venezuela', 100),
	(10, 10, '1996-07-16', 'Franchi S.p.A.', 'Torino', 'Italy', 225),
	(11, 11, '1996-07-17', 'Gourmet Lanchonetes', 'Campinas', 'Brazil', 175),
	(12, 12, '1996-07-18', 'Com�rcio Mineiro', 'S�o Paulo', 'Brazil', 125),
	(13, 13, '1996-07-19', 'Hanari Carnes', 'Rio de Janeiro', 'Brazil', 150),
	(14, 14, '1996-07-22', 'Que Del�cia', 'Rio de Janeiro', 'Brazil', 200),
	(15, 15, '1996-07-23', 'Vaffeljernet', '�rhus', 'Denmark', 175),
	(16, 1, '1996-07-24', 'Toms Spezialit�ten', 'M�nster', 'Germany', 225),
	(17, 2, '1996-07-25', 'Furia Bacalhau e Frutos do Mar', 'Lisboa', 'Portugal', 150),
	(18, 3, '1996-07-26', 'Laughing Bacchus Wine Cellars', 'Vancouver', 'Canada', 175),
	(19, 4, '1996-07-29', 'Let''s Stop N Shop', 'San Francisco', 'USA', 125),
	(20, 5, '1996-07-30', 'Eastern Connection', 'London', 'UK', 225),
	(21, 6, '1996-07-31', 'Ernst Handel', 'Graz', 'Austria', 150),
	(22, 7, '1996-08-01', 'Wilman Kala', 'Helsinki', 'Finland', 100),
	(23, 8, '1996-08-02', 'Folk och f� HB', 'Br�cke', 'Sweden', 125),
	(24, 9, '1996-08-05', 'The Cracker Box', 'Butte', 'USA', 175),
	(25, 10, '1996-08-06', 'Rancho grande', 'Rio de Janeiro', 'Brazil', 200),
	(26, 11, '1996-08-07', 'Bottom-Dollar Markets', 'Tsawassen', 'Canada', 225),
	(27, 12, '1996-08-08', 'Gourmet Lanchonetes', 'Campinas', 'Brazil', 100),
	(28, 13, '1996-08-09', 'Piccolo und mehr', 'Salzburg', 'Austria', 150),
	(29, 14, '1996-08-12', 'Rattlesnake Canyon Grocery', 'Albuquerque', 'USA', 175),
	(30, 15, '1996-08-13', 'Queen Cozinha', 'Sao Paulo', 'Brazil', 225),
	(31, 1, '1996-08-14', 'Simons bistro', 'K�benhavn', 'Denmark', 125),
	(32, 2, '1996-08-15', 'QUICK-Stop', 'Cunewalde', 'Germany', 175),
	(33, 3, '1996-08-16', 'Folk och f� HB', 'Br�cke', 'Sweden', 200),
	(34, 4, '1996-08-19', 'Lazy K Kountry Store', 'Walla Walla', 'USA', 100),
	(35, 5, '1996-08-20', 'Lonesome Pine Restaurant', 'Portland', 'USA', 225),
	(36, 6, '1996-08-21', 'Laughing Bacchus Wine Cellars', 'Vancouver', 'Canada', 150),
	(37, 7, '1996-08-22', 'QUICK-Stop', 'Cunewalde', 'Germany', 125),
	(38, 8, '1996-08-23', 'Furia Bacalhau e Frutos do Mar', 'Lisboa', 'Portugal', 175),
	(39, 9, '1996-08-26', 'Wartian Herkku', 'Oulu', 'Finland', 200)
