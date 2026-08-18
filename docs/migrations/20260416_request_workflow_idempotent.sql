DROP PROCEDURE IF EXISTS `POMELO_BEFORE_DROP_PRIMARY_KEY`;
DELIMITER //
CREATE PROCEDURE `POMELO_BEFORE_DROP_PRIMARY_KEY`(IN `SCHEMA_NAME_ARGUMENT` VARCHAR(255), IN `TABLE_NAME_ARGUMENT` VARCHAR(255))
BEGIN
	DECLARE HAS_AUTO_INCREMENT_ID TINYINT(1);
	DECLARE PRIMARY_KEY_COLUMN_NAME VARCHAR(255);
	DECLARE PRIMARY_KEY_TYPE VARCHAR(255);
	DECLARE SQL_EXP VARCHAR(1000);
	SELECT COUNT(*)
		INTO HAS_AUTO_INCREMENT_ID
		FROM `information_schema`.`COLUMNS`
		WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
			AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
			AND `Extra` = 'auto_increment'
			AND `COLUMN_KEY` = 'PRI'
			LIMIT 1;
	IF HAS_AUTO_INCREMENT_ID THEN
		SELECT `COLUMN_TYPE`
			INTO PRIMARY_KEY_TYPE
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_KEY` = 'PRI'
			LIMIT 1;
		SELECT `COLUMN_NAME`
			INTO PRIMARY_KEY_COLUMN_NAME
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_KEY` = 'PRI'
			LIMIT 1;
		SET SQL_EXP = CONCAT('ALTER TABLE `', (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA())), '`.`', TABLE_NAME_ARGUMENT, '` MODIFY COLUMN `', PRIMARY_KEY_COLUMN_NAME, '` ', PRIMARY_KEY_TYPE, ' NOT NULL;');
		SET @SQL_EXP = SQL_EXP;
		PREPARE SQL_EXP_EXECUTE FROM @SQL_EXP;
		EXECUTE SQL_EXP_EXECUTE;
		DEALLOCATE PREPARE SQL_EXP_EXECUTE;
	END IF;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS `POMELO_AFTER_ADD_PRIMARY_KEY`;
DELIMITER //
CREATE PROCEDURE `POMELO_AFTER_ADD_PRIMARY_KEY`(IN `SCHEMA_NAME_ARGUMENT` VARCHAR(255), IN `TABLE_NAME_ARGUMENT` VARCHAR(255), IN `COLUMN_NAME_ARGUMENT` VARCHAR(255))
BEGIN
	DECLARE HAS_AUTO_INCREMENT_ID INT(11);
	DECLARE PRIMARY_KEY_COLUMN_NAME VARCHAR(255);
	DECLARE PRIMARY_KEY_TYPE VARCHAR(255);
	DECLARE SQL_EXP VARCHAR(1000);
	SELECT COUNT(*)
		INTO HAS_AUTO_INCREMENT_ID
		FROM `information_schema`.`COLUMNS`
		WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
			AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
			AND `COLUMN_NAME` = COLUMN_NAME_ARGUMENT
			AND `COLUMN_TYPE` LIKE '%int%'
			AND `COLUMN_KEY` = 'PRI';
	IF HAS_AUTO_INCREMENT_ID THEN
		SELECT `COLUMN_TYPE`
			INTO PRIMARY_KEY_TYPE
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_NAME` = COLUMN_NAME_ARGUMENT
				AND `COLUMN_TYPE` LIKE '%int%'
				AND `COLUMN_KEY` = 'PRI';
		SELECT `COLUMN_NAME`
			INTO PRIMARY_KEY_COLUMN_NAME
			FROM `information_schema`.`COLUMNS`
			WHERE `TABLE_SCHEMA` = (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA()))
				AND `TABLE_NAME` = TABLE_NAME_ARGUMENT
				AND `COLUMN_NAME` = COLUMN_NAME_ARGUMENT
				AND `COLUMN_TYPE` LIKE '%int%'
				AND `COLUMN_KEY` = 'PRI';
		SET SQL_EXP = CONCAT('ALTER TABLE `', (SELECT IFNULL(SCHEMA_NAME_ARGUMENT, SCHEMA())), '`.`', TABLE_NAME_ARGUMENT, '` MODIFY COLUMN `', PRIMARY_KEY_COLUMN_NAME, '` ', PRIMARY_KEY_TYPE, ' NOT NULL AUTO_INCREMENT;');
		SET @SQL_EXP = SQL_EXP;
		PREPARE SQL_EXP_EXECUTE FROM @SQL_EXP;
		EXECUTE SQL_EXP_EXECUTE;
		DEALLOCATE PREPARE SQL_EXP_EXECUTE;
	END IF;
END //
DELIMITER ;

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `AuditLogs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `EntityName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EntityId` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Action` longtext CHARACTER SET utf8mb4 NOT NULL,
        `OldValues` longtext CHARACTER SET utf8mb4 NULL,
        `NewValues` longtext CHARACTER SET utf8mb4 NULL,
        `IpAddress` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Categories` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Categories` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Divisions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Divisions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Products` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ModelNumber` longtext CHARACTER SET utf8mb4 NULL,
        `Manufacturer` longtext CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Products` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `RepairingFirms` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ContactPerson` longtext CHARACTER SET utf8mb4 NULL,
        `Email` longtext CHARACTER SET utf8mb4 NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NULL,
        `Address` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_RepairingFirms` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Suppliers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ContactPerson` longtext CHARACTER SET utf8mb4 NULL,
        `Email` longtext CHARACTER SET utf8mb4 NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NULL,
        `Address` longtext CHARACTER SET utf8mb4 NULL,
        `Website` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Suppliers` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Users` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Username` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `FirstName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `LastName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RefreshToken` longtext CHARACTER SET utf8mb4 NULL,
        `RefreshTokenExpiryTime` datetime(6) NULL,
        `IsLocked` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `DivisionId` int NOT NULL,
        `Role` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Users_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `PurchasingOrders` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `OrderNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `OrderDate` datetime(6) NOT NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        `SupplierId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PurchasingOrders` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PurchasingOrders_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Notifications` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Message` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsRead` tinyint(1) NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NULL,
        `ReferenceId` longtext CHARACTER SET utf8mb4 NULL,
        `UserId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Notifications` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Assets` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `AssetTag` longtext CHARACTER SET utf8mb4 NULL,
        `AssetDate` datetime(6) NOT NULL,
        `Status` int NOT NULL,
        `SerialNumber` longtext CHARACTER SET utf8mb4 NULL,
        `PurchaseValue` decimal(65,30) NOT NULL,
        `Warranty` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Computer_Display` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Computer_RAM` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Computer_GPU` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Computer_Storage` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Computer_OS` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Server_OS` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Server_RAM` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Server_CPU` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Server_IPAddress` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Server_Storage` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Networking_PortCount` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Networking_DataRate` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Networking_FormFactor` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Networking_MACAddress` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Printing_Type` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Printing_PrintingTechnology` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Printing_Connectivity` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Printing_PrintResolution` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Furniture_Material` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Furniture_Length` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Furniture_Width` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Furniture_Height` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Furniture_Color` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications_Furniture_Adjustability` longtext CHARACTER SET utf8mb4 NULL,
        `CategoryId` int NOT NULL,
        `DivisionId` int NOT NULL,
        `ProductId` int NOT NULL,
        `SupplierId` int NOT NULL,
        `AssignedUserId` int NULL,
        `PurchasingOrderId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Assets` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Assets_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Assets_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Assets_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Assets_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`),
        CONSTRAINT `FK_Assets_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Assets_Users_AssignedUserId` FOREIGN KEY (`AssignedUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `DiscountInfos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DiscountAmount` decimal(65,30) NOT NULL,
        `DiscountPercentage` decimal(65,30) NOT NULL,
        `PurchasingOrderId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_DiscountInfos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DiscountInfos_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `PurchasingOrderItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ItemName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Model` longtext CHARACTER SET utf8mb4 NULL,
        `Warranty` longtext CHARACTER SET utf8mb4 NULL,
        `Quantity` int NOT NULL,
        `UnitPrice` decimal(65,30) NOT NULL,
        `Amount` decimal(65,30) NOT NULL,
        `Discount` decimal(65,30) NOT NULL,
        `DiscountedPrice` decimal(65,30) NOT NULL,
        `VatPercentage` decimal(65,30) NOT NULL,
        `VatAmount` decimal(65,30) NOT NULL,
        `TotalPrice` decimal(65,30) NOT NULL,
        `SpecialNote` longtext CHARACTER SET utf8mb4 NULL,
        `PurchasingOrderId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PurchasingOrderItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PurchasingOrderItems_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `GRNs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `GrnNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ReceivedDate` datetime(6) NOT NULL,
        `ReceivedBy` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `PurchasingOrderId` int NOT NULL,
        `AssetId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_GRNs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_GRNs_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_GRNs_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `MaintenanceRecords` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `MaintenanceNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` int NOT NULL,
        `MaintenanceDate` datetime(6) NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Cost` decimal(65,30) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        `AssetId` int NOT NULL,
        `RepairingFirmId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_MaintenanceRecords` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MaintenanceRecords_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_MaintenanceRecords_RepairingFirms_RepairingFirmId` FOREIGN KEY (`RepairingFirmId`) REFERENCES `RepairingFirms` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `QRNs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `QrnNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `InspectionDate` datetime(6) NOT NULL,
        `InspectedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsPassed` tinyint(1) NOT NULL,
        `Remarks` longtext CHARACTER SET utf8mb4 NULL,
        `AssetId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_QRNs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_QRNs_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Requests` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `RequestNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` int NOT NULL,
        `Priority` int NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Remarks` longtext CHARACTER SET utf8mb4 NULL,
        `Specifications` longtext CHARACTER SET utf8mb4 NULL,
        `SpecialNote` longtext CHARACTER SET utf8mb4 NULL,
        `RequesterId` int NOT NULL,
        `AssetId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Requests` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Requests_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`),
        CONSTRAINT `FK_Requests_Users_RequesterId` FOREIGN KEY (`RequesterId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `TINs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TinNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `TransferInDate` datetime(6) NOT NULL,
        `ReceivedBy` longtext CHARACTER SET utf8mb4 NULL,
        `AssetId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_TINs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TINs_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `Transfers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TransferNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `TransferDate` datetime(6) NOT NULL,
        `Reason` longtext CHARACTER SET utf8mb4 NULL,
        `AssetId` int NOT NULL,
        `FromDivisionId` int NOT NULL,
        `ToDivisionId` int NOT NULL,
        `TransferById` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Transfers` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Transfers_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Transfers_Divisions_FromDivisionId` FOREIGN KEY (`FromDivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Transfers_Divisions_ToDivisionId` FOREIGN KEY (`ToDivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Transfers_Users_TransferById` FOREIGN KEY (`TransferById`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE TABLE `GINs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `GinNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AssignedDate` datetime(6) NOT NULL,
        `Condition` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `AssetId` int NOT NULL,
        `GRNId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_GINs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_GINs_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_GINs_GRNs_GRNId` FOREIGN KEY (`GRNId`) REFERENCES `GRNs` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Assets_AssetCode` ON `Assets` (`AssetCode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Assets_AssignedUserId` ON `Assets` (`AssignedUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Assets_CategoryId` ON `Assets` (`CategoryId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Assets_DivisionId` ON `Assets` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Assets_ProductId` ON `Assets` (`ProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Assets_PurchasingOrderId` ON `Assets` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Assets_SupplierId` ON `Assets` (`SupplierId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_DiscountInfos_PurchasingOrderId` ON `DiscountInfos` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_GINs_AssetId` ON `GINs` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_GINs_GRNId` ON `GINs` (`GRNId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_GRNs_AssetId` ON `GRNs` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_GRNs_PurchasingOrderId` ON `GRNs` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_MaintenanceRecords_AssetId` ON `MaintenanceRecords` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_MaintenanceRecords_RepairingFirmId` ON `MaintenanceRecords` (`RepairingFirmId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Notifications_UserId` ON `Notifications` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_PurchasingOrderItems_PurchasingOrderId` ON `PurchasingOrderItems` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_PurchasingOrders_SupplierId` ON `PurchasingOrders` (`SupplierId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_QRNs_AssetId` ON `QRNs` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Requests_AssetId` ON `Requests` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Requests_RequesterId` ON `Requests` (`RequesterId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_TINs_AssetId` ON `TINs` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Transfers_AssetId` ON `Transfers` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Transfers_FromDivisionId` ON `Transfers` (`FromDivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Transfers_ToDivisionId` ON `Transfers` (`ToDivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Transfers_TransferById` ON `Transfers` (`TransferById`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE INDEX `IX_Users_DivisionId` ON `Users` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Username` ON `Users` (`Username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260304234905_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260304234905_InitialCreate', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305025107_AddAssetInforming') THEN

    CREATE TABLE `AssetInformings` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ItemName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Model` longtext CHARACTER SET utf8mb4 NULL,
        `Warranty` longtext CHARACTER SET utf8mb4 NULL,
        `Quantity` int NOT NULL,
        `PurchasedDate` datetime(6) NOT NULL,
        `PurchasedPrice` decimal(65,30) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DivisionId` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AssetInformings` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AssetInformings_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305025107_AddAssetInforming') THEN

    CREATE INDEX `IX_AssetInformings_DivisionId` ON `AssetInformings` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305025107_AddAssetInforming') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260305025107_AddAssetInforming', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309095605_AddPasswordResetTokens') THEN

    ALTER TABLE `Users` ADD `PasswordResetToken` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309095605_AddPasswordResetTokens') THEN

    ALTER TABLE `Users` ADD `ResetTokenExpiryTime` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309095605_AddPasswordResetTokens') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309095605_AddPasswordResetTokens', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309100652_MakeDivisionAndRoleNullable') THEN

    ALTER TABLE `Users` MODIFY COLUMN `Role` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309100652_MakeDivisionAndRoleNullable') THEN

    ALTER TABLE `Users` MODIFY COLUMN `DivisionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309100652_MakeDivisionAndRoleNullable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309100652_MakeDivisionAndRoleNullable', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `MaintenanceRecords` DROP FOREIGN KEY `FK_MaintenanceRecords_Assets_AssetId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `MaintenanceRecords` DROP FOREIGN KEY `FK_MaintenanceRecords_RepairingFirms_RepairingFirmId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    CALL POMELO_BEFORE_DROP_PRIMARY_KEY(NULL, 'MaintenanceRecords');
    ALTER TABLE `MaintenanceRecords` DROP PRIMARY KEY;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `MaintenanceRecords` RENAME `Maintenances`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `Maintenances` RENAME INDEX `IX_MaintenanceRecords_RepairingFirmId` TO `IX_Maintenances_RepairingFirmId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `Maintenances` RENAME INDEX `IX_MaintenanceRecords_AssetId` TO `IX_Maintenances_AssetId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `PK_Maintenances` PRIMARY KEY (`Id`);
    CALL POMELO_AFTER_ADD_PRIMARY_KEY(NULL, 'Maintenances', 'Id');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_RepairingFirms_RepairingFirmId` FOREIGN KEY (`RepairingFirmId`) REFERENCES `RepairingFirms` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260313030201_AddMaintenanceAndRepairingFirms') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260313030201_AddMaintenanceAndRepairingFirms', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260326162431_AddEmployeeAssetRequests') THEN

    CREATE TABLE `AssetRequests` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AssetCategory` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Priority` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Quantity` int NULL,
        `Reason` longtext CHARACTER SET utf8mb4 NULL,
        `Attachments` longtext CHARACTER SET utf8mb4 NULL,
        `Status` int NOT NULL,
        `RequesterId` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RequesterName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RequestType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SubmittedDate` datetime(6) NOT NULL,
        `AssetId` int NULL,
        `UserId` int NULL,
        CONSTRAINT `PK_AssetRequests` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AssetRequests_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`),
        CONSTRAINT `FK_AssetRequests_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260326162431_AddEmployeeAssetRequests') THEN

    CREATE INDEX `IX_AssetRequests_AssetId` ON `AssetRequests` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260326162431_AddEmployeeAssetRequests') THEN

    CREATE INDEX `IX_AssetRequests_UserId` ON `AssetRequests` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260326162431_AddEmployeeAssetRequests') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260326162431_AddEmployeeAssetRequests', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD `CreatedBy` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD `DivisionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD `UpdatedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    CREATE INDEX `IX_AssetRequests_DivisionId` ON `AssetRequests` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    ALTER TABLE `AssetRequests` ADD CONSTRAINT `FK_AssetRequests_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327153823_AddDivisionIdToAssetRequest') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260327153823_AddDivisionIdToAssetRequest', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `DivisionHeadReviewedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `DivisionHeadReviewerId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `PickupConfirmedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `RequiresDivisionHeadApproval` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `StorekeeperProcessedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `StorekeeperProcessorId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Requests` ADD `TemporarilyAssignedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Assets` ADD `ReservedByRequestId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Assets` ADD `ReservedForUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    ALTER TABLE `Assets` ADD `ReservedUntilUtc` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416052050_AddRequestWorkflowStagesAndAssetReservation') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416052050_AddRequestWorkflowStagesAndAssetReservation', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

DROP PROCEDURE `POMELO_BEFORE_DROP_PRIMARY_KEY`;

DROP PROCEDURE `POMELO_AFTER_ADD_PRIMARY_KEY`;

