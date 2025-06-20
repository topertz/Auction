PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS `User`;
CREATE TABLE `User` (
    `UserID` INTEGER NOT NULL PRIMARY KEY,
    `UserName` TEXT NOT NULL UNIQUE,
    `PasswordHash` TEXT NOT NULL,
    `PasswordSalt` TEXT NOT NULL,
);

drop table if EXISTS `Session`;
CREATE TABLE `Session` (
    `SessionID` integer NOT NULL PRIMARY KEY,
    `SessionCookie` text not null UNIQUE,
    `UserID` integer not null,
    `ValidUntil` integer not null,
    `LoginTime` integer not null,
    FOREIGN KEY (`UserID`) REFERENCES `User`(`UserID`)
);

DROP TABLE IF EXISTS `Auction`;
CREATE TABLE `Auction` (
    `AuctionID` INTEGER NOT NULL PRIMARY KEY,
    `UserID` INTEGER NOT NULL,
    `ItemName` TEXT NOT NULL,
    `DescriptionName` TEXT NOT NULL,
    `StartingBid` INTEGER NOT NULL,
    `BidEndTime` INTEGER NOT NULL,
    FOREIGN KEY (`UserID`) REFERENCES `User`(`UserID`)
);

DROP TABLE IF EXISTS `Bid`;
CREATE TABLE `Bid` (
    `BidID` INTEGER NOT NULL PRIMARY KEY,
    `AuctionID` INTEGER NOT NULL,
    `UserID` INTEGER NOT NULL,
    `BidAmount` INTEGER NOT NULL,
    `BidTime` INTEGER NOT NULL,
    FOREIGN KEY (`AuctionID`) REFERENCES `Auction`(`AuctionID`),
    FOREIGN KEY (`UserID`) REFERENCES `User`(`UserID`)
);