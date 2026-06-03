USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'CarShowroomDB')
BEGIN
    ALTER DATABASE [CarShowroomDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [CarShowroomDB];
END
GO

CREATE DATABASE [CarShowroomDB];
GO

USE [CarShowroomDB];
GO

USE [CarShowRoomDB]
GO
/****** Object:  Table [dbo].[Car_Images]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Car_Images](
	[image_id] [int] IDENTITY(1,1) NOT NULL,
	[car_id] [int] NOT NULL,
	[image_url] [nvarchar](max) NOT NULL,
	[is_main] [bit] NULL,
	[uploaded_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[image_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cars]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cars](
	[car_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[brand] [nvarchar](50) NOT NULL,
	[model] [nvarchar](50) NOT NULL,
	[year] [int] NOT NULL,
	[color] [nvarchar](30) NULL,
	[price] [decimal](18, 2) NOT NULL,
	[fuel_type] [nvarchar](20) NULL,
	[gear_type] [nvarchar](20) NULL,
	[mileage] [int] NULL,
	[description] [nvarchar](max) NULL,
	[rent_price_per_day] [decimal](18, 2) NULL,
	[status] [nvarchar](20) NULL,
	[is_approved] [bit] NULL,
	[approved_by] [int] NULL,
	[approval_notes] [nvarchar](max) NULL,
	[approval_date] [datetime] NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[car_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Favorites]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Favorites](
	[favorite_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[car_id] [int] NOT NULL,
	[added_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[favorite_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Installment_Orders]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Installment_Orders](
	[order_id] [int] NOT NULL,
	[installment_months] [int] NOT NULL,
	[monthly_payment] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[order_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[car_id] [int] NOT NULL,
	[order_type] [nvarchar](20) NOT NULL,
	[order_status] [nvarchar](20) NULL,
	[total_price] [decimal](18, 2) NOT NULL,
	[approved_by] [int] NULL,
	[approval_notes] [nvarchar](max) NULL,
	[approval_date] [datetime] NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[payment_id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[amount] [decimal](18, 2) NOT NULL,
	[payment_method] [nvarchar](50) NULL,
	[payment_status] [nvarchar](20) NULL,
	[payment_date] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[payment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rent_Orders]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rent_Orders](
	[order_id] [int] NOT NULL,
	[start_date] [datetime] NOT NULL,
	[end_date] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[full_name] [nvarchar](100) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[password] [nvarchar](max) NOT NULL,
	[phone] [nvarchar](20) NULL,
	[role] [nvarchar](20) NOT NULL,
	[address] [nvarchar](255) NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Car_Images] ON 

INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (1, 1, N'https://images.unsplash.com/photo-1603584173870-7f30df006c06', 0, CAST(N'2026-05-22T04:45:20.523' AS DateTime))
INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (2, 2, N'https://images.unsplash.com/photo-1560958089-b8a1929cea89', 0, CAST(N'2026-05-22T04:45:20.523' AS DateTime))
INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (3, 3, N'https://images.unsplash.com/photo-1584345604482-81216b38c290', 0, CAST(N'2026-05-22T04:45:20.523' AS DateTime))
INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (4, 4, N'string', 0, CAST(N'2026-05-23T17:12:16.760' AS DateTime))
INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (5, 5, N'ggggg', 0, CAST(N'2026-05-23T17:18:05.050' AS DateTime))
INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (6, 5, N'ggggg', 0, CAST(N'2026-05-23T17:18:05.050' AS DateTime))
INSERT [dbo].[Car_Images] ([image_id], [car_id], [image_url], [is_main], [uploaded_at]) VALUES (7, 5, N'hhhhh', 0, CAST(N'2026-05-23T17:18:05.050' AS DateTime))
SET IDENTITY_INSERT [dbo].[Car_Images] OFF
GO
SET IDENTITY_INSERT [dbo].[Cars] ON 

INSERT [dbo].[Cars] ([car_id], [user_id], [brand], [model], [year], [color], [price], [fuel_type], [gear_type], [mileage], [description], [rent_price_per_day], [status], [is_approved], [approved_by], [approval_notes], [approval_date], [created_at]) VALUES (1, 1, N'Audi', N'A6', 2022, N'Blue', CAST(52000.00 AS Decimal(18, 2)), N'Gasoline', N'Automatic', 12000, N'Like new, first owner.', NULL, N'Available', 1, NULL, NULL, NULL, CAST(N'2026-05-22T04:45:20.523' AS DateTime))
INSERT [dbo].[Cars] ([car_id], [user_id], [brand], [model], [year], [color], [price], [fuel_type], [gear_type], [mileage], [description], [rent_price_per_day], [status], [is_approved], [approved_by], [approval_notes], [approval_date], [created_at]) VALUES (2, 1, N'Tesla', N'Model 3', 2023, N'Red', CAST(48000.00 AS Decimal(18, 2)), N'Electric', N'Automatic', 2000, N'Full self-driving, clean history.', NULL, N'Available', 1, NULL, NULL, NULL, CAST(N'2026-05-22T04:45:20.523' AS DateTime))
INSERT [dbo].[Cars] ([car_id], [user_id], [brand], [model], [year], [color], [price], [fuel_type], [gear_type], [mileage], [description], [rent_price_per_day], [status], [is_approved], [approved_by], [approval_notes], [approval_date], [created_at]) VALUES (3, 1, N'Ford', N'Mustang', 2020, N'Yellow', CAST(38000.00 AS Decimal(18, 2)), N'Gasoline', N'Manual', 35000, N'Powerful engine, well maintained.', NULL, N'Available', 1, NULL, NULL, NULL, CAST(N'2026-05-22T04:45:20.523' AS DateTime))
INSERT [dbo].[Cars] ([car_id], [user_id], [brand], [model], [year], [color], [price], [fuel_type], [gear_type], [mileage], [description], [rent_price_per_day], [status], [is_approved], [approved_by], [approval_notes], [approval_date], [created_at]) VALUES (4, 1, N'bb', N'bb', 22, N'bb', CAST(0.00 AS Decimal(18, 2)), N'bb', N'bb', 22, N'bb', NULL, N'Available', 1, 1, N'approval', CAST(N'2026-06-01T17:44:06.593' AS DateTime), CAST(N'2026-05-23T17:12:16.760' AS DateTime))
INSERT [dbo].[Cars] ([car_id], [user_id], [brand], [model], [year], [color], [price], [fuel_type], [gear_type], [mileage], [description], [rent_price_per_day], [status], [is_approved], [approved_by], [approval_notes], [approval_date], [created_at]) VALUES (5, 1, N'bb', N'bb', 22, N'bb', CAST(0.00 AS Decimal(18, 2)), N'bb', N'bb', 22, N'bb', NULL, N'Available', 1, 1, N'approval', CAST(N'2026-06-01T17:44:54.410' AS DateTime), CAST(N'2026-05-23T17:18:05.047' AS DateTime))
SET IDENTITY_INSERT [dbo].[Cars] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([user_id], [full_name], [email], [password], [phone], [role], [address], [created_at]) VALUES (1, N'Bashir', N'bb@gmail.com', N'$2b$10$9Qj6sD7wt5ecLj72c5Txke1WYhIGZnZk.A511Okei3XQqoaj23vE2', N'0999', N'Admin', N'Aleppo', CAST(N'2026-05-18T22:21:49.953' AS DateTime))
INSERT [dbo].[Users] ([user_id], [full_name], [email], [password], [phone], [role], [address], [created_at]) VALUES (2, N'ghaith swed', N'gg@gmail.com', N'$2b$10$2.uwOLRsNK.e.Uz9NIB7su8FSjBSiKv2bz0CLKXb.0DMBy4IzAKBS', N'0998', N'Admin', N'Aleppo', CAST(N'2026-05-19T15:43:25.493' AS DateTime))
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__AB6E6164DD7F912E]    Script Date: 02/06/2026 06:39:39 م ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Car_Images] ADD  DEFAULT ((0)) FOR [is_main]
GO
ALTER TABLE [dbo].[Car_Images] ADD  DEFAULT (getdate()) FOR [uploaded_at]
GO
ALTER TABLE [dbo].[Cars] ADD  DEFAULT ('Available') FOR [status]
GO
ALTER TABLE [dbo].[Cars] ADD  DEFAULT ((0)) FOR [is_approved]
GO
ALTER TABLE [dbo].[Cars] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Favorites] ADD  DEFAULT (getdate()) FOR [added_at]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ('Pending') FOR [order_status]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT ('Pending') FOR [payment_status]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [payment_date]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Car_Images]  WITH CHECK ADD  CONSTRAINT [FK_Images_Cars] FOREIGN KEY([car_id])
REFERENCES [dbo].[Cars] ([car_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Car_Images] CHECK CONSTRAINT [FK_Images_Cars]
GO
ALTER TABLE [dbo].[Cars]  WITH CHECK ADD  CONSTRAINT [FK_Cars_Admin] FOREIGN KEY([approved_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Cars] CHECK CONSTRAINT [FK_Cars_Admin]
GO
ALTER TABLE [dbo].[Cars]  WITH CHECK ADD  CONSTRAINT [FK_Cars_Owner] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Cars] CHECK CONSTRAINT [FK_Cars_Owner]
GO
ALTER TABLE [dbo].[Favorites]  WITH CHECK ADD  CONSTRAINT [FK_Favorites_Car] FOREIGN KEY([car_id])
REFERENCES [dbo].[Cars] ([car_id])
GO
ALTER TABLE [dbo].[Favorites] CHECK CONSTRAINT [FK_Favorites_Car]
GO
ALTER TABLE [dbo].[Favorites]  WITH CHECK ADD  CONSTRAINT [FK_Favorites_User] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Favorites] CHECK CONSTRAINT [FK_Favorites_User]
GO
ALTER TABLE [dbo].[Installment_Orders]  WITH CHECK ADD  CONSTRAINT [FK_Installment_Order_Base] FOREIGN KEY([order_id])
REFERENCES [dbo].[Orders] ([order_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Installment_Orders] CHECK CONSTRAINT [FK_Installment_Order_Base]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Admin] FOREIGN KEY([approved_by])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Admin]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Car] FOREIGN KEY([car_id])
REFERENCES [dbo].[Cars] ([car_id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Car]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_User] FOREIGN KEY([user_id])
REFERENCES [dbo].[Users] ([user_id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_User]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_Order] FOREIGN KEY([order_id])
REFERENCES [dbo].[Orders] ([order_id])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_Order]
GO
ALTER TABLE [dbo].[Rent_Orders]  WITH CHECK ADD  CONSTRAINT [FK_Rent_Order_Base] FOREIGN KEY([order_id])
REFERENCES [dbo].[Orders] ([order_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Rent_Orders] CHECK CONSTRAINT [FK_Rent_Order_Base]
GO
/****** Object:  StoredProcedure [dbo].[sp_AddCarWithImages]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_AddCarWithImages]
    @user_id INT,
    @brand NVARCHAR(50),
    @model NVARCHAR(50),
    @year INT,
    @color NVARCHAR(30),
    @price DECIMAL(18, 2),
    @fuel_type NVARCHAR(20),
    @gear_type NVARCHAR(20),
    @mileage INT,
    @description NVARCHAR(MAX),
    @image_urls NVARCHAR(MAX) -- سنرسل الروابط كنص مفصول بفاصلة
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. إدخال السيارة
        INSERT INTO Cars (user_id, brand, model, year, color, price, fuel_type, gear_type, mileage, description, is_approved, status, created_at)
        VALUES (@user_id, @brand, @model, @year, @color, @price, @fuel_type, @gear_type, @mileage, @description, 0, 'Available', GETDATE());

        DECLARE @new_car_id INT = SCOPE_IDENTITY();

        -- 2. إدخال الصور (تقسيم النص لتحويله لصفوف)
        INSERT INTO Car_Images (car_id, image_url, uploaded_at)
        SELECT @new_car_id, value, GETDATE()
        FROM STRING_SPLIT(@image_urls, ',');

        COMMIT TRANSACTION;
        SELECT @new_car_id AS NewCarId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ApproveCar]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ApproveCar]
    @car_id INT,
    @approved_by INT,
    @notes NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Cars
    SET is_approved = 1,
        approved_by = @approved_by,
        approval_notes = @notes,
        approval_date = GETDATE(),
        status = 'Available' -- نضمن أنها أصبحت متاحة للعرض
    WHERE car_id = @car_id;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 50001, 'Car not found.', 1;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteCar]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DeleteCar]
    @car_id INT,
    @requested_by_user_id INT -- للتأكد أن الحاذف هو صاحب السيارة أو مسؤول
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- التأكد من الصلاحية: هل هو صاحب السيارة أم أدمن؟
        IF NOT EXISTS (SELECT 1 FROM Cars WHERE car_id = @car_id AND (user_id = @requested_by_user_id OR EXISTS (SELECT 1 FROM [Users] WHERE user_id = @requested_by_user_id AND role IN ('Admin', 'Employee'))))
        BEGIN
            THROW 50003, 'You do not have permission to delete this car.', 1;
        END

        -- 1. حذف الصور المرتبطة بالسيارة
        DELETE FROM Car_Images WHERE car_id = @car_id;

        -- 2. حذف السيارة من قوائم المفضلة لدى المستخدمين
        DELETE FROM Favorites WHERE car_id = @car_id;

        -- 3. حذف السيارة نفسها
        DELETE FROM Cars WHERE car_id = @car_id; 

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteCarImage]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DeleteCarImage]
    @image_id INT,
    @requested_by_user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    

    IF EXISTS (
        SELECT 1 FROM Car_Images ci
        JOIN Cars c ON ci.car_id = c.car_id
        WHERE ci.image_id = @image_id 
        AND (c.user_id = @requested_by_user_id OR EXISTS (SELECT 1 FROM [Users] WHERE user_id = @requested_by_user_id AND role IN ('Admin', 'Employee')))
    )
    BEGIN
        DELETE FROM Car_Images WHERE image_id = @image_id;
    END
    ELSE
    BEGIN
        THROW 50005, 'Permission denied or image not found.', 1;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteUser]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DeleteUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- ملاحظة: إذا كان للمستخدم سجلات في جداول أخرى (مثل الطلبات)، 
    -- يجب حذفها أولاً أو استخدام ON DELETE CASCADE في العلاقات.
    
    DELETE FROM Users WHERE user_id = @UserId;

    SELECT @@ROWCOUNT; -- سيعيد 1 إذا تم الحذف بنجاح
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteUserByAdmin]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DeleteUserByAdmin]
    @user_id_to_delete INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. حذف السيارات التابعة للمستخدم وصورها
        -- نحذف الصور أولاً لأنها مرتبطة بـ car_id
        DELETE FROM Car_Images 
        WHERE car_id IN (SELECT car_id FROM Cars WHERE user_id = @user_id_to_delete);

        -- حذف المفضلات المرتبطة بالمستخدم
        DELETE FROM Favorites WHERE user_id = @user_id_to_delete;

        -- حذف السيارات نفسها
        DELETE FROM Cars WHERE user_id = @user_id_to_delete;

        -- 2. حذف المستخدم النهائي
        DELETE FROM [User] WHERE user_id = @user_id_to_delete;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW 50002, 'Failed to delete user. The user might have active orders or payments.', 1;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetUserByEmail]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. إجراء مخصص لجلب بيانات المستخدم عند تسجيل الدخول
CREATE PROCEDURE [dbo].[sp_GetUserByEmail]
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- جلب كافة البيانات التحققية والصلاحيات بناءً على الإيميل
    SELECT user_id, full_name, email, password, role 
    FROM Users 
    WHERE email = @Email;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetUserProfile]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetUserProfile]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT user_id, full_name, email, phone, address, role, created_at 
    FROM Users 
    WHERE user_id = @UserId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegisterUser]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_RegisterUser]
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @Password NVARCHAR(MAX),
    @Phone NVARCHAR(20),   -- تأكد من وجود هذا السطر
    @Role NVARCHAR(20),
    @Address NVARCHAR(255) -- تأكد من وجود هذا السطر
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Users WHERE email = @Email)
    BEGIN
        RAISERROR('This email is already registered.', 16, 1);
        RETURN;
    END

    -- إدخال كافة الحقول السبعة بالتفصيل
    INSERT INTO Users (full_name, email, password, phone, role, address, created_at)
    VALUES (@FullName, @Email, @Password, @Phone, @Role, @Address, GETDATE());
    
    SELECT SCOPE_IDENTITY() AS NewUserId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_SearchCars]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_SearchCars]
    @brand NVARCHAR(50) = NULL,
    @model NVARCHAR(50) = NULL,
    @minPrice DECIMAL(18,2) = NULL,
    @maxPrice DECIMAL(18,2) = NULL,
    @year INT = NULL,
    @fuelType NVARCHAR(20) = NULL,
    @gearType NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.*, ci.image_url
    FROM Cars c
    LEFT JOIN Car_Images ci ON c.car_id = ci.car_id
    WHERE c.is_approved = 1 -- نعرض فقط السيارات الموافق عليها
      AND (@brand IS NULL OR c.brand LIKE '%' + @brand + '%')
      AND (@model IS NULL OR c.model LIKE '%' + @model + '%')
      AND (@year IS NULL OR c.year = @year)
      AND (@minPrice IS NULL OR c.price >= @minPrice)
      AND (@maxPrice IS NULL OR c.price <= @maxPrice)
      AND (@fuelType IS NULL OR c.fuel_type = @fuelType)
      AND (@gearType IS NULL OR c.gear_type = @gearType)
    ORDER BY c.created_at DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateCar]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdateCar]
    @car_id INT,
    @user_id INT, -- للتأكد من الملكية
    @brand NVARCHAR(50),
    @model NVARCHAR(50),
    @year INT,
    @color NVARCHAR(30),
    @price DECIMAL(18, 2),
    @fuel_type NVARCHAR(20),
    @gear_type NVARCHAR(20),
    @mileage INT,
    @description NVARCHAR(MAX),
    @rent_price_per_day DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- تحديث البيانات فقط إذا كان المستخدم هو صاحب السيارة
    UPDATE Cars
    SET brand = @brand,
        model = @model,
        year = @year,
        color = @color,
        price = @price,
        fuel_type = @fuel_type,
        gear_type = @gear_type,
        mileage = @mileage,
        description = @description,
        rent_price_per_day = @rent_price_per_day,
        is_approved = 0, -- إعادة السيارة لحالة "بانتظار الموافقة" بعد التعديل لضمان الرقابة
        created_at = GETDATE() 
    WHERE car_id = @car_id AND user_id = @user_id;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 50004, 'Car not found or you do not have permission to edit it.', 1;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdatePassword]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdatePassword]
    @UserId INT,
    @NewPassword NVARCHAR(MAX)
AS
BEGIN
    UPDATE Users 
    SET password = @NewPassword 
    WHERE user_id = @UserId;
    
    SELECT @@ROWCOUNT;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateUserProfile]    Script Date: 02/06/2026 06:39:39 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdateUserProfile]
    @UserId INT,
    @FullName NVARCHAR(100),
    @Phone NVARCHAR(20),
    @Address NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Users 
    SET full_name = @FullName, 
        phone = @Phone, 
        address = @Address
    WHERE user_id = @UserId;

    -- لإرجاع عدد الصفوف المتأثرة للتأكد من نجاح العملية
    SELECT @@ROWCOUNT;
END
GO
USE [master]
GO
ALTER DATABASE [CarShowRoomDB] SET  READ_WRITE 
GO
