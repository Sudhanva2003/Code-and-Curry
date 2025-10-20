create database CodeCurry;
use CodeCurry;
select*from Restaurant;
select*from Food;
select*from Users;
select*from orders;
select*from Orderdetail;

CREATE TABLE Restaurant (
    RestId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Rating DECIMAL(2,1) NOT NULL DEFAULT 4.0,
    Cuisine VARCHAR(100) NULL,
    Phone VARCHAR(15) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    GstNo VARCHAR(15) NOT NULL,          -- GST numbers are alphanumeric (15 characters)
    FssaiNo VARCHAR(14) NOT NULL,        -- FSSAI License numbers are 14 digits
    RestStatus NVARCHAR(20) NOT NULL DEFAULT 'Open',  -- Open, Closed, Deleted
    RestImageUrl NVARCHAR(255) NULL
);


drop table restaurant;

CREATE TABLE Food (
    FoodId INT IDENTITY(1,1) PRIMARY KEY,
    RestId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    Price MONEY NOT NULL,
    Category NVARCHAR(50) NULL,
    FoodStatus NVARCHAR(20) NOT NULL DEFAULT 'Available', -- Available, NotAvailable, Deleted
    FoodImageUrl NVARCHAR(255) NULL,
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId)
);


drop table food;

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone VARCHAR(15) NULL,
    Rating decimal(2,1) null default 4.0,
    Address NVARCHAR(255) NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    UserStatus NVARCHAR(20) NOT NULL DEFAULT 'Active',
    Role NVARCHAR(20) DEFAULT 'Customer',   -- Can be 'Customer' or 'Deliverer'
    LicenseNumber NVARCHAR(50) NULL,        -- Optional, used only for deliverers
    VehicleNumber NVARCHAR(20) NULL         -- Optional, used only for deliverers
);



drop table users;

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RestId INT NOT NULL,
    DelivererId INT NULL,                          -- Will be NULL until a deliverer accepts
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    Rating Decimal(2,1) NULL Default 4.0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    TotalAmount MONEY NOT NULL,
    Discount MONEY NULL,           -- Discount on the order
    HandlingFee MONEY NOT NULL,        -- Handling charges
    PlatformFee MONEY NOT NULL,        -- Platform/service fee
    DeliveryFee MONEY NOT NULL,        -- Delivery charges
    GST MONEY NOT NULL,                -- Tax amount
    FinalPrice MONEY NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId),
    FOREIGN KEY (DelivererId) REFERENCES Users(UserId)
);



drop table orders;

CREATE TABLE OrderDetail (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    FoodId INT NOT NULL,
    Quantity INT NOT NULL,
    Price MONEY NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (FoodId) REFERENCES Food(FoodId)
);


drop table orderdetail;






