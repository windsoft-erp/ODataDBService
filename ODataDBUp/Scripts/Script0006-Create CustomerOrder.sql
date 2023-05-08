IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerOrder]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[CustomerOrder] (
											   [CustomerName] [nvarchar](50) NOT NULL,
											   [OrderDate] [date] NOT NULL,
											   [OrderTotal] [int] NOT NULL
		) ON [PRIMARY]
	END
