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
    Rating DECIMAL(2,1) NOT NULL,
    Phone VARCHAR(15),
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsOpen BIT NOT NULL DEFAULT 1,
    RestImageUrl NVARCHAR(255) NULL
);

drop table restaurant;

CREATE TABLE Food (
    FoodId INT IDENTITY(1,1) PRIMARY KEY,
    RestId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(10,2) NOT NULL,
    Category NVARCHAR(50),
    IsAvailable BIT NOT NULL DEFAULT 1,
    FoodImageUrl NVARCHAR(255) NULL,
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId)
        ON UPDATE CASCADE -- delete handled manually
);

drop table food;

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone VARCHAR(15),
    Address NVARCHAR(255),
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'Customer'
);

drop table users;

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RestId INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending',
    TotalAmount DECIMAL(10,2),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
        ON UPDATE CASCADE ON DELETE CASCADE,
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId)
        ON UPDATE CASCADE ON DELETE CASCADE
);

drop table orders;

CREATE TABLE OrderDetail (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    FoodId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
        ON UPDATE CASCADE ON DELETE CASCADE,
    FOREIGN KEY (FoodId) REFERENCES Food(FoodId)
        ON UPDATE NO ACTION ON DELETE NO ACTION
);

drop table orderdetail;






