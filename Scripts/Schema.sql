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
    IsOpen BIT NOT NULL DEFAULT 1
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
    FOREIGN KEY (RestId) REFERENCES Restaurant(RestId)
        ON UPDATE CASCADE -- delete handled by trigger
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
        ON UPDATE CASCADE -- delete handled by trigger
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

CREATE TRIGGER trg_DeleteRestaurant
ON Restaurant
INSTEAD OF DELETE
AS
BEGIN
    -- delete related Orders (will cascade to OrderDetail)
    DELETE FROM Orders WHERE RestId IN (SELECT RestId FROM DELETED);

    -- delete related Food (will cascade to OrderDetail)
    DELETE FROM Food WHERE RestId IN (SELECT RestId FROM DELETED);

    -- finally delete Restaurant
    DELETE FROM Restaurant WHERE RestId IN (SELECT RestId FROM DELETED);
END;

CREATE TRIGGER trg_DeleteFood
ON Food
INSTEAD OF DELETE
AS
BEGIN
    DELETE FROM OrderDetail WHERE FoodId IN (SELECT FoodId FROM DELETED);
    DELETE FROM Food WHERE FoodId IN (SELECT FoodId FROM DELETED);
END;


INSERT INTO Restaurant (Name, Address, Rating, Phone, Email, PasswordHash, IsOpen)
VALUES ('Tandoori Tales', '12 Spice Street', 4.6, '9876543210', 'tandoori@example.com', 'hashpwd1', 1);

INSERT INTO Restaurant (Name, Address, Rating, Phone, Email, PasswordHash, IsOpen)
VALUES ('Pasta Paradise', '45 Olive Road', 4.3, '9123456780', 'pasta@example.com', 'hashpwd2', 0);

INSERT INTO Food (RestId, Name, Description, Price, Category, IsAvailable)
VALUES (1, 'Chicken Tikka', 'Grilled chicken marinated in spices', 299.00, 'Indian', 1);

INSERT INTO Food (RestId, Name, Description, Price, Category, IsAvailable)
VALUES (2, 'Spaghetti Alfredo', 'Creamy Alfredo pasta', 349.00, 'Italian', 1);

INSERT INTO Users (FullName, Email, Phone, Address, PasswordHash, Role)
VALUES ('David Warner', 'david@example.com', '8888888888', '78 Sunset Blvd', 'hashpwd3', 'Customer');

INSERT INTO Users (FullName, Email, Phone, Address, PasswordHash, Role)
VALUES ('Emily Clark', 'emily@example.com', '9999999999', '90 Sunrise Ave', 'hashpwd4', 'Delivery');

INSERT INTO Orders (UserId, RestId, OrderDate, Status, TotalAmount)
VALUES (1, 1, GETDATE(), 'Pending', 299.00);

INSERT INTO Orders (UserId, RestId, OrderDate, Status, TotalAmount)
VALUES (2, 2, GETDATE(), 'Delivered', 349.00);

INSERT INTO OrderDetail (OrderId, FoodId, Quantity, Price)
VALUES (1, 1, 2, 598.00); -- 2 Chicken Tikka

INSERT INTO OrderDetail (OrderId, FoodId, Quantity, Price)
VALUES (2, 2, 1, 349.00); -- 1 Spaghetti Alfredo






-- Before: check Orders & OrderDetail for UserId = 1
SELECT * FROM Orders WHERE UserId = 1;
SELECT * FROM OrderDetail WHERE OrderId IN (SELECT OrderId FROM Orders WHERE UserId = 1);

-- Delete User
DELETE FROM Users WHERE UserId = 1;

-- After: confirm orders & orderdetails are gone
SELECT * FROM Orders WHERE UserId = 1;
SELECT * FROM OrderDetail WHERE OrderId IN (SELECT OrderId FROM Orders WHERE UserId = 1);



-- Before
SELECT * FROM Orders WHERE RestId = 1;
SELECT * FROM Food WHERE RestId = 1;
SELECT * FROM OrderDetail WHERE OrderId IN (SELECT OrderId FROM Orders WHERE RestId = 1);

-- Delete Restaurant
DELETE FROM Restaurant WHERE RestId = 1;

-- After
SELECT * FROM Orders WHERE RestId = 1;
SELECT * FROM Food WHERE RestId = 1;
SELECT * FROM OrderDetail WHERE OrderId IN (SELECT OrderId FROM Orders WHERE RestId = 1);


-- Before
SELECT * FROM Food WHERE FoodId = 2;
SELECT * FROM OrderDetail WHERE FoodId = 2;

-- Delete Food
DELETE FROM Food WHERE FoodId = 2;

-- After
SELECT * FROM Food WHERE FoodId = 2;
SELECT * FROM OrderDetail WHERE FoodId = 2;

-- Before
SELECT * FROM Orders WHERE OrderId = 2;
SELECT * FROM OrderDetail WHERE OrderId = 2;

-- Delete Order
DELETE FROM Orders WHERE OrderId = 2;

-- After
SELECT * FROM Orders WHERE OrderId = 2;
SELECT * FROM OrderDetail WHERE OrderId = 2;
