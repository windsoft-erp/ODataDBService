IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[Orders](
										[OrderID] [int] IDENTITY(1,1) NOT NULL,
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

INSERT INTO [dbo].[Orders] (EmployeeID, OrderDate, ShipName, ShipCity, ShipCountry, Value)
VALUES
	(1, '1996-07-04', 'Vins et alcools Chevalier', 'Reims', 'France', 100),
	(2, '1996-07-05', 'Toms Spezialit�ten', 'M�nster', 'Germany', 200),
	(3, '1996-07-08', 'Hanari Carnes', 'Rio de Janeiro', 'Brazil', 150),
	(4, '1996-07-08', 'Victuailles en stock', 'Lyon', 'France', 175),
	(5, '1996-07-09', 'Rattlesnake Canyon Grocery', 'Albuquerque', 'USA', 225),
	(6, '1996-07-10', 'Ernst Handel', 'Graz', 'Austria', 125),
	(7, '1996-07-11', 'Que Del�cia', 'Rio de Janeiro', 'Brazil', 75),
	(8, '1996-07-12', 'Ricardo Adocicados', 'S�o Paulo', 'Brazil', 150),
	(9, '1996-07-15', 'HILARI�N-Abastos', 'San Crist�bal', 'Venezuela', 100),
	(10, '1996-07-16', 'Franchi S.p.A.', 'Torino', 'Italy', 225),
	(11, '1996-07-17', 'Gourmet Lanchonetes', 'Campinas', 'Brazil', 175),
	(12, '1996-07-18', 'Com�rcio Mineiro', 'S�o Paulo', 'Brazil', 125),
	(13, '1996-07-19', 'Hanari Carnes', 'Rio de Janeiro', 'Brazil', 150),
	(14, '1996-07-22', 'Que Del�cia', 'Rio de Janeiro', 'Brazil', 200),
	(15, '1996-07-23', 'Vaffeljernet', '�rhus', 'Denmark', 175),
	(1, '1996-07-24', 'Toms Spezialit�ten', 'M�nster', 'Germany', 225),
	(2, '1996-07-25', 'Furia Bacalhau e Frutos do Mar', 'Lisboa', 'Portugal', 150),
	(3, '1996-07-26', 'Laughing Bacchus Wine Cellars', 'Vancouver', 'Canada', 175),
	(4, '1996-07-29', 'Let''s Stop N Shop', 'San Francisco', 'USA', 125),
	(5, '1996-07-30', 'Eastern Connection', 'London', 'UK', 225),
	(6, '1996-07-31', 'Ernst Handel', 'Graz', 'Austria', 150),
	(7, '1996-08-01', 'Wilman Kala', 'Helsinki', 'Finland', 100),
	(8, '1996-08-02', 'Folk och f� HB', 'Br�cke', 'Sweden', 125),
	(9, '1996-08-05', 'The Cracker Box', 'Butte', 'USA', 175),
	(10, '1996-08-06', 'Rancho grande', 'Rio de Janeiro', 'Brazil', 200),
	(11, '1996-08-07', 'Bottom-Dollar Markets', 'Tsawassen', 'Canada', 225),
	(12, '1996-08-08', 'Gourmet Lanchonetes', 'Campinas', 'Brazil', 100),
	(13, '1996-08-09', 'Piccolo und mehr', 'Salzburg', 'Austria', 150),
	(14, '1996-08-12', 'Rattlesnake Canyon Grocery', 'Albuquerque', 'USA', 175),
	(15, '1996-08-13', 'Queen Cozinha', 'Sao Paulo', 'Brazil', 225),
	(1, '1996-08-14', 'Simons bistro', 'K�benhavn', 'Denmark', 125),
	(2, '1996-08-15', 'QUICK-Stop', 'Cunewalde', 'Germany', 175),
	(3, '1996-08-16', 'Folk och f� HB', 'Br�cke', 'Sweden', 200),
	(4, '1996-08-19', 'Lazy K Kountry Store', 'Walla Walla', 'USA', 100),
	(5, '1996-08-20', 'Lonesome Pine Restaurant', 'Portland', 'USA', 225),
	(6, '1996-08-21', 'Laughing Bacchus Wine Cellars', 'Vancouver', 'Canada', 150),
	(7, '1996-08-22', 'QUICK-Stop', 'Cunewalde', 'Germany', 125),
	(8, '1996-08-23', 'Furia Bacalhau e Frutos do Mar', 'Lisboa', 'Portugal', 175),
	(9, '1996-08-26', 'Wartian Herkku', 'Oulu', 'Finland', 200)
