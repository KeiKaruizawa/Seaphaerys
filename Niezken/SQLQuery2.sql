ALTER TABLE Bookings ADD AccommodationType NVARCHAR(MAX) NULL;
ALTER TABLE Bookings ADD ContactNumber NVARCHAR(MAX) NULL;
ALTER TABLE Bookings ADD PassengerCount INT NOT NULL DEFAULT 1;
ALTER TABLE Bookings ADD PassengersJson NVARCHAR(MAX) NULL;

Update-Database