create database CodeCurry;
use CodeCurry;
select*from Restaurant;
select*from Food;
select*from Users;
select*from orders;
select*from Orderdetail;

CREATE TABLE Restaurant (
    RestId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL, --will take space seperated input
    Address NVARCHAR(255) NOT NULL,
    Rating DECIMAL(2,1) NOT NULL,
    Phone VARCHAR(15),
    Email VARCHAR(100) NOT NULL,
    Password VARCHAR(100) NOT NULL,
    IsOpen BIT NOT NULL DEFAULT 1    -- 1 = Open, 0 = Closed
);

drop table Restaurant;


CREATE TABLE Food (
    FoodId INT IDENTITY(1,1) PRIMARY KEY,
    RestId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(10,2) NOT NULL,
    Category NVARCHAR(50),
    IsAvailable BIT NOT NULL DEFAULT 1,  -- 1 = Available, 0 = Not Available
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId)
        ON DELETE CASCADE
);

drop table Food;

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone VARCHAR(15),
    Address NVARCHAR(255),
    PasswordHash NVARCHAR(255) NOT NULL, -- for login
    Role NVARCHAR(20) DEFAULT 'Customer' -- Customer, Admin, Delivery
);

drop table Users;

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RestId INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Accepted, Preparing, OutForDelivery, Delivered, Cancelled
    TotalAmount DECIMAL(10,2),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId)
);


drop table Orders;

CREATE TABLE OrderDetail (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    FoodId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL, -- store at order time, not just Food.Price
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (FoodId) REFERENCES Food(FoodId)
);

drop table OrderDetail;

