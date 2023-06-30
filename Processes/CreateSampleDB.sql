USE [master]
GO
CREATE DATABASE [MessageData]
GO
EXEC dbo.sp_dbcmptlevel @dbname=N'MessageData', @new_cmptlevel=90
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [MessageData].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [MessageData] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [MessageData] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [MessageData] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [MessageData] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [MessageData] SET ARITHABORT OFF 
GO
ALTER DATABASE [MessageData] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [MessageData] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [MessageData] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [MessageData] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [MessageData] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [MessageData] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [MessageData] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [MessageData] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [MessageData] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [MessageData] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [MessageData] SET  ENABLE_BROKER 
GO
ALTER DATABASE [MessageData] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [MessageData] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [MessageData] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [MessageData] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [MessageData] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [MessageData] SET  READ_WRITE 
GO
ALTER DATABASE [MessageData] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [MessageData] SET  MULTI_USER 
GO
ALTER DATABASE [MessageData] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [MessageData] SET DB_CHAINING OFF 

USE [MessageData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[OrderID] [int] NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[OrderAmount] [money] NOT NULL,
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


USE [MessageData]
GO
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[StoreOrderItem]
	@OrderID [int],
	@OrderDate [datetime],
	@OrderAmount [money]
AS

INSERT INTO [Order] (OrderID, OrderDate, OrderAmount) VALUES (@OrderID, @OrderDate, @OrderAmount)
GO

CREATE PROCEDURE [dbo].[SelectOrderItem]
	@OrderID [int]
AS
BEGIN
SET NOCOUNT ON;

SELECT OrderID, OrderDate, OrderAmount
FROM [Order]
WHERE OrderID=@OrderID

SELECT @@IDENTITY
END
GO

CREATE procedure [dbo].[SelectOrderItemXml] (@OrderID int)
AS
SELECT OrderID, OrderDate, OrderAmount 
FROM [Order] 
WHERE OrderID=@OrderID
FOR XML AUTO, ELEMENTS
GO

CREATE PROCEDURE [dbo].[CreateCustomer]
	@CustomerID [int],
	@Name [nvarchar](50),
	@EmailAddress [nvarchar](50)
AS

INSERT INTO [Customer] (CustomerID, Name, EmailAddress) VALUES (@CustomerID, @Name, @EmailAddress)
GO

CREATE PROCEDURE [dbo].[DeleteCustomer]
	@CustomerID [int]
AS

DELETE FROM [Customer] WHERE CustomerID = @CustomerID
GO

CREATE PROCEDURE [dbo].[ReadCustomer]
	@CustomerID [int]
AS

SELECT *
FROM [Customer]
WHERE CustomerID=@CustomerID
GO

USE [MessageData]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customer](
	[CustomerID] [int] NULL,
	[Name] [nvarchar](50) NULL,
	[EmailAddress] [nvarchar](50) NULL
) ON [PRIMARY]

GO

INSERT INTO [MessageData].[dbo].[Customer]
           ([CustomerID]
           ,[Name]
           ,[EmailAddress])
     VALUES
           (1001,
           'Northwind Traders',
           'orders@northwind.com')
GO

INSERT INTO [MessageData].[dbo].[Customer]
           ([CustomerID]
           ,[Name]
           ,[EmailAddress])
     VALUES
           (1002,
           'LitWare Inc.',
           'accounts@litware.com')
GO

INSERT INTO [MessageData].[dbo].[Customer]
           ([CustomerID]
           ,[Name]
           ,[EmailAddress])
     VALUES
           (1003,
           'AdventureWorks Cycles',
           'admin@adventureworks.com')
GO