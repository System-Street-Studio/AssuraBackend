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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416003421_AddPhoneNumberToUser') THEN

    ALTER TABLE `Users` ADD `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260416003421_AddPhoneNumberToUser') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260416003421_AddPhoneNumberToUser', '8.0.13');

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

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422071329_AddCommentsToPurchasingOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422071329_AddCommentsToPurchasingOrder', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422074655_AddCommentsToPurchasingOrder2') THEN

    ALTER TABLE `PurchasingOrders` ADD `Comments` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422074655_AddCommentsToPurchasingOrder2') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422074655_AddCommentsToPurchasingOrder2', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422081356_RemoveCommentsFromPurchasingOrder') THEN

    ALTER TABLE `PurchasingOrders` DROP COLUMN `Comments`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422081356_RemoveCommentsFromPurchasingOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422081356_RemoveCommentsFromPurchasingOrder', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422105019_UpdateTransferEntity') THEN

    ALTER TABLE `Transfers` ADD `ReturnDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422105019_UpdateTransferEntity') THEN

    ALTER TABLE `Transfers` ADD `Status` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422105019_UpdateTransferEntity') THEN

    ALTER TABLE `Transfers` ADD `TargetUserId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422105019_UpdateTransferEntity') THEN

    CREATE INDEX `IX_Transfers_TargetUserId` ON `Transfers` (`TargetUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422105019_UpdateTransferEntity') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_TargetUserId` FOREIGN KEY (`TargetUserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422105019_UpdateTransferEntity') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422105019_UpdateTransferEntity', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422151310_AddCommentsToPO') THEN

    ALTER TABLE `PurchasingOrders` ADD `Comments` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422151310_AddCommentsToPO') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422151310_AddCommentsToPO', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422155746_AddCommentsToPO1') THEN

    ALTER TABLE `PurchasingOrders` MODIFY COLUMN `Comments` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422155746_AddCommentsToPO1') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422155746_AddCommentsToPO1', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422181746_AddDiscardedNotes') THEN

    CREATE TABLE `DiscardedNotes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Time` time(6) NOT NULL,
        `AssetType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SpecialNote` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_DiscardedNotes` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422181746_AddDiscardedNotes') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422181746_AddDiscardedNotes', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    ALTER TABLE `DiscardedNotes` MODIFY COLUMN `Status` int NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `AccDiscardedItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `AssetType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CurrentUser` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SpecialNote` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ValueAtPurchasing` decimal(65,30) NOT NULL,
        `CurrentValue` decimal(65,30) NOT NULL,
        `Time` time(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AccDiscardedItems` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `AccDiscardNotes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `Note` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AssetType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CurrentUser` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Time` time(6) NOT NULL,
        `ValueAtPurchasing` decimal(65,30) NOT NULL,
        `CurrentValue` decimal(65,30) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AccDiscardNotes` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `AccPendingItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Category` int NOT NULL,
        `Time` time(6) NOT NULL,
        `AssetType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CurrentUser` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SpecialNote` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ValueAtPurchasing` decimal(65,30) NOT NULL,
        `CurrentValue` decimal(65,30) NOT NULL,
        `IsHighlighted` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AccPendingItems` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `Buyers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Contact` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Category` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Buyers` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `LostItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `ReportedBy` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` int NOT NULL,
        `AssetType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Time` time(6) NOT NULL,
        `ValueAtPurchasing` decimal(65,30) NOT NULL,
        `CurrentValue` decimal(65,30) NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_LostItems` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `QueueItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `Status` int NOT NULL,
        `Time` time(6) NOT NULL,
        `AssetType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SpecialNote` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ReviewNote` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_QueueItems` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    CREATE TABLE `Receipts` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssetName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Division` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Date` datetime(6) NOT NULL,
        `Amount` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Receipts` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422190455_AddFullBackendEntities') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422190455_AddFullBackendEntities', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260422230919_RomoveCommentsFromPO1') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260422230919_RomoveCommentsFromPO1', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423110054_AddPriorityToPo') THEN

    ALTER TABLE `PurchasingOrders` DROP COLUMN `Comments`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423110054_AddPriorityToPo') THEN

    ALTER TABLE `PurchasingOrders` ADD `Priority` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423110054_AddPriorityToPo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260423110054_AddPriorityToPo', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423113824_RemovePriorityFromPo') THEN

    ALTER TABLE `PurchasingOrders` DROP COLUMN `Priority`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423113824_RemovePriorityFromPo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260423113824_RemovePriorityFromPo', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423180231_UpdateTransferEntityAndStatus') THEN

    ALTER TABLE `Transfers` ADD `AssignedToId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423180231_UpdateTransferEntityAndStatus') THEN

    CREATE INDEX `IX_Transfers_AssignedToId` ON `Transfers` (`AssignedToId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423180231_UpdateTransferEntityAndStatus') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_AssignedToId` FOREIGN KEY (`AssignedToId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423180231_UpdateTransferEntityAndStatus') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260423180231_UpdateTransferEntityAndStatus', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260423204221_InitialFullCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260423204221_InitialFullCreate', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424070957_FixTransferIssues') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260424070957_FixTransferIssues', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_AssignedToId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    ALTER TABLE `Transfers` RENAME COLUMN `AssignedToId` TO `AssetRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    ALTER TABLE `Transfers` RENAME INDEX `IX_Transfers_AssignedToId` TO `IX_Transfers_AssetRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    ALTER TABLE `Transfers` ADD `CurrentHolderId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    CREATE INDEX `IX_Transfers_CurrentHolderId` ON `Transfers` (`CurrentHolderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_AssetRequests_AssetRequestId` FOREIGN KEY (`AssetRequestId`) REFERENCES `AssetRequests` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_CurrentHolderId` FOREIGN KEY (`CurrentHolderId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424074818_UpdateTransferEntityWithAssetRequest') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260424074818_UpdateTransferEntityWithAssetRequest', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260424082341_UpdateTransferStatusEnumWithNewValues') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260424082341_UpdateTransferStatusEnumWithNewValues', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425090014_AddTransferPeriodToTransfer') THEN

    ALTER TABLE `Transfers` ADD `TransferPeriod` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425090014_AddTransferPeriodToTransfer') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425090014_AddTransferPeriodToTransfer', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425093625_AddTransferPeriodColumn') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425093625_AddTransferPeriodColumn', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425111752_ClearTransferTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425111752_ClearTransferTable', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425123931_UpdateFromDivisionIdToAllowZero') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425123931_UpdateFromDivisionIdToAllowZero', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425124611_AllowFromDivisionIdZero') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425124611_AllowFromDivisionIdZero', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425125122_AllowFromDivisionIdZeroWithoutNullable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425125122_AllowFromDivisionIdZeroWithoutNullable', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425125348_AddZeroDivisionRecord') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425125348_AddZeroDivisionRecord', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260425131201_CleanupOldTransfersWithZeroFromDivisionId') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260425131201_CleanupOldTransfersWithZeroFromDivisionId', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428080840_UpdateTransfercolumns') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260428080840_UpdateTransfercolumns', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Divisions_FromDivisionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Divisions_ToDivisionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_TargetUserId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_TransferById`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `TransferById` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `ToDivisionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `TargetUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `FromDivisionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` ADD `AssetName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` ADD `AssetTag` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Divisions_FromDivisionId` FOREIGN KEY (`FromDivisionId`) REFERENCES `Divisions` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Divisions_ToDivisionId` FOREIGN KEY (`ToDivisionId`) REFERENCES `Divisions` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_TargetUserId` FOREIGN KEY (`TargetUserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_TransferById` FOREIGN KEY (`TransferById`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260428150801_UpdateTransferTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260428150801_UpdateTransferTable', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260430071906_AddUserWorkflowFields') THEN

    ALTER TABLE `Users` MODIFY COLUMN `PhoneNumber` varchar(30) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260430071906_AddUserWorkflowFields') THEN

    ALTER TABLE `Users` ADD `AssignedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260430071906_AddUserWorkflowFields') THEN

    ALTER TABLE `Users` ADD `EmploymentStatus` varchar(40) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260430071906_AddUserWorkflowFields') THEN

    ALTER TABLE `Users` ADD `JobTitle` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260430071906_AddUserWorkflowFields') THEN

    ALTER TABLE `Users` ADD `RequestedRole` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260430071906_AddUserWorkflowFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260430071906_AddUserWorkflowFields', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    ALTER TABLE `Assets` ADD `LastVerifiedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    ALTER TABLE `Assets` ADD `LastVerifiedByUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    CREATE TABLE `UserDivisionRoles` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `DivisionId` int NOT NULL,
        `Role` int NOT NULL,
        `JobTitle` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `AssignedAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_UserDivisionRoles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_UserDivisionRoles_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_UserDivisionRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    CREATE INDEX `IX_Assets_LastVerifiedByUserId` ON `Assets` (`LastVerifiedByUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    CREATE INDEX `IX_UserDivisionRoles_DivisionId` ON `UserDivisionRoles` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    CREATE INDEX `IX_UserDivisionRoles_UserId` ON `UserDivisionRoles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    ALTER TABLE `Assets` ADD CONSTRAINT `FK_Assets_Users_LastVerifiedByUserId` FOREIGN KEY (`LastVerifiedByUserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260501050817_AddUserDivisionRole') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260501050817_AddUserDivisionRole', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_AssetRequests_AssetRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_TargetUserId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` DROP COLUMN `AssetName`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` DROP COLUMN `AssetTag`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Computer_Display`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Computer_GPU`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Computer_OS`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Computer_RAM`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Computer_Storage`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Furniture_Adjustability`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Furniture_Color`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Furniture_Height`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Furniture_Length`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Furniture_Material`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Furniture_Width`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Networking_DataRate`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Networking_FormFactor`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Networking_MACAddress`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Networking_PortCount`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Printing_Connectivity`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Printing_PrintResolution`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Printing_PrintingTechnology`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Printing_Type`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Server_CPU`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Server_IPAddress`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Server_OS`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Server_RAM`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Assets` DROP COLUMN `Specifications_Server_Storage`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `TargetUserId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `AssetRequestId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Products` ADD `ImageUrl` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    CREATE TABLE `Specifications` (
        `AssetId` int NOT NULL,
        `Computer_Display` longtext CHARACTER SET utf8mb4 NULL,
        `Computer_RAM` longtext CHARACTER SET utf8mb4 NULL,
        `Computer_GPU` longtext CHARACTER SET utf8mb4 NULL,
        `Computer_Storage` longtext CHARACTER SET utf8mb4 NULL,
        `Computer_OS` longtext CHARACTER SET utf8mb4 NULL,
        `Server_OS` longtext CHARACTER SET utf8mb4 NULL,
        `Server_RAM` longtext CHARACTER SET utf8mb4 NULL,
        `Server_CPU` longtext CHARACTER SET utf8mb4 NULL,
        `Server_IPAddress` longtext CHARACTER SET utf8mb4 NULL,
        `Server_Storage` longtext CHARACTER SET utf8mb4 NULL,
        `Networking_PortCount` longtext CHARACTER SET utf8mb4 NULL,
        `Networking_DataRate` longtext CHARACTER SET utf8mb4 NULL,
        `Networking_FormFactor` longtext CHARACTER SET utf8mb4 NULL,
        `Networking_MACAddress` longtext CHARACTER SET utf8mb4 NULL,
        `Printing_Type` longtext CHARACTER SET utf8mb4 NULL,
        `Printing_PrintingTechnology` longtext CHARACTER SET utf8mb4 NULL,
        `Printing_Connectivity` longtext CHARACTER SET utf8mb4 NULL,
        `Printing_PrintResolution` longtext CHARACTER SET utf8mb4 NULL,
        `Furniture_Material` longtext CHARACTER SET utf8mb4 NULL,
        `Furniture_Length` longtext CHARACTER SET utf8mb4 NULL,
        `Furniture_Width` longtext CHARACTER SET utf8mb4 NULL,
        `Furniture_Height` longtext CHARACTER SET utf8mb4 NULL,
        `Furniture_Color` longtext CHARACTER SET utf8mb4 NULL,
        `Furniture_Adjustability` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Specifications` PRIMARY KEY (`AssetId`),
        CONSTRAINT `FK_Specifications_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE CASCADE
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_AssetRequests_AssetRequestId` FOREIGN KEY (`AssetRequestId`) REFERENCES `AssetRequests` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_TargetUserId` FOREIGN KEY (`TargetUserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260507055103_AddAssetPoolEndpoints') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260507055103_AddAssetPoolEndpoints', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_AssetRequests_AssetRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_CurrentHolderId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `CurrentHolderId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `AssetRequestId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_AssetRequests_AssetRequestId` FOREIGN KEY (`AssetRequestId`) REFERENCES `AssetRequests` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_CurrentHolderId` FOREIGN KEY (`CurrentHolderId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160100_updateTransferEntityNullableColumns') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508160100_updateTransferEntityNullableColumns', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160950_updateNotificationNullubleValue') THEN

    ALTER TABLE `Notifications` DROP FOREIGN KEY `FK_Notifications_Users_UserId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160950_updateNotificationNullubleValue') THEN

    ALTER TABLE `Notifications` MODIFY COLUMN `UserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160950_updateNotificationNullubleValue') THEN

    ALTER TABLE `Notifications` ADD CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508160950_updateNotificationNullubleValue') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508160950_updateNotificationNullubleValue', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508161500_updatetransferNullubleValue') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_TargetUserId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508161500_updatetransferNullubleValue') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `TargetUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508161500_updatetransferNullubleValue') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_TargetUserId` FOREIGN KEY (`TargetUserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508161500_updatetransferNullubleValue') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508161500_updatetransferNullubleValue', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508162702_updatetransferNullubleValue3') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_CurrentHolderId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508162702_updatetransferNullubleValue3') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `CurrentHolderId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508162702_updatetransferNullubleValue3') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_CurrentHolderId` FOREIGN KEY (`CurrentHolderId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260508162702_updatetransferNullubleValue3') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260508162702_updatetransferNullubleValue3', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `ApprovedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `ApprovedByUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `CompletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `EscalatedToProcurementAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `IssueType` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `Notes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `OriginalRequestId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `Priority` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `ReplacementAssetId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `RequestedByUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `StartedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD `StorekeeperUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    CREATE INDEX `IX_Maintenances_ApprovedByUserId` ON `Maintenances` (`ApprovedByUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    CREATE INDEX `IX_Maintenances_OriginalRequestId` ON `Maintenances` (`OriginalRequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    CREATE INDEX `IX_Maintenances_ReplacementAssetId` ON `Maintenances` (`ReplacementAssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    CREATE INDEX `IX_Maintenances_RequestedByUserId` ON `Maintenances` (`RequestedByUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    CREATE INDEX `IX_Maintenances_StorekeeperUserId` ON `Maintenances` (`StorekeeperUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_Assets_ReplacementAssetId` FOREIGN KEY (`ReplacementAssetId`) REFERENCES `Assets` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_Requests_OriginalRequestId` FOREIGN KEY (`OriginalRequestId`) REFERENCES `Requests` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_Users_ApprovedByUserId` FOREIGN KEY (`ApprovedByUserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_Users_RequestedByUserId` FOREIGN KEY (`RequestedByUserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    ALTER TABLE `Maintenances` ADD CONSTRAINT `FK_Maintenances_Users_StorekeeperUserId` FOREIGN KEY (`StorekeeperUserId`) REFERENCES `Users` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260510111058_EnterpriseMaintenanceWorkflowFix') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260510111058_EnterpriseMaintenanceWorkflowFix', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260525062317_AddAssetAttachments') THEN

    ALTER TABLE `AssetRequests` DROP COLUMN `Attachments`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260525062317_AddAssetAttachments') THEN

    CREATE TABLE `AssetAttachment` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FileName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FileUrl` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FileSize` bigint NOT NULL,
        `FileType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `UploadedDate` datetime(6) NOT NULL,
        `AssetRequestId` int NULL,
        CONSTRAINT `PK_AssetAttachment` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AssetAttachment_AssetRequests_AssetRequestId` FOREIGN KEY (`AssetRequestId`) REFERENCES `AssetRequests` (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260525062317_AddAssetAttachments') THEN

    CREATE INDEX `IX_AssetAttachment_AssetRequestId` ON `AssetAttachment` (`AssetRequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260525062317_AddAssetAttachments') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260525062317_AddAssetAttachments', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260601183158_AddReceiptFileUrl') THEN

    ALTER TABLE `Receipts` ADD `FileUrl` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260601183158_AddReceiptFileUrl') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260601183158_AddReceiptFileUrl', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Divisions_FromDivisionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Divisions_ToDivisionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_CurrentHolderId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` DROP FOREIGN KEY `FK_Transfers_Users_TargetUserId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `ToDivisionId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `TargetUserId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `FromDivisionId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` MODIFY COLUMN `CurrentHolderId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Divisions_FromDivisionId` FOREIGN KEY (`FromDivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Divisions_ToDivisionId` FOREIGN KEY (`ToDivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_CurrentHolderId` FOREIGN KEY (`CurrentHolderId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    ALTER TABLE `Transfers` ADD CONSTRAINT `FK_Transfers_Users_TargetUserId` FOREIGN KEY (`TargetUserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611180047_changetransfer.csfilenotnullablevalues4') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260611180047_changetransfer.csfilenotnullablevalues4', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612192008_SetTransferByIdasNullablevalue') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612192008_SetTransferByIdasNullablevalue', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260613041404_AddAssetRequestAttachmentUrls') THEN

    ALTER TABLE `AssetRequests` ADD `AttachmentUrls` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260613041404_AddAssetRequestAttachmentUrls') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260613041404_AddAssetRequestAttachmentUrls', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260615125516_AddTransferAuditAndOverdue') THEN

    CREATE TABLE `TransferApprovals` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TransferId` int NOT NULL,
        `ApprovedByUserId` int NOT NULL,
        `FromStatus` int NOT NULL,
        `ToStatus` int NOT NULL,
        `Comments` longtext CHARACTER SET utf8mb4 NULL,
        `ApprovedAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_TransferApprovals` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TransferApprovals_Transfers_TransferId` FOREIGN KEY (`TransferId`) REFERENCES `Transfers` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_TransferApprovals_Users_ApprovedByUserId` FOREIGN KEY (`ApprovedByUserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260615125516_AddTransferAuditAndOverdue') THEN

    CREATE INDEX `IX_TransferApprovals_ApprovedByUserId` ON `TransferApprovals` (`ApprovedByUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260615125516_AddTransferAuditAndOverdue') THEN

    CREATE INDEX `IX_TransferApprovals_TransferId` ON `TransferApprovals` (`TransferId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260615125516_AddTransferAuditAndOverdue') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260615125516_AddTransferAuditAndOverdue', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260616123102_MakeAssetFKsOptional') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260616123102_MakeAssetFKsOptional', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617075551_AddCustomReports') THEN

    CREATE TABLE `CustomReports` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ReportIdCode` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Owner` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Period` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Size` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsScheduled` tinyint(1) NOT NULL,
        `ScheduleFrequency` longtext CHARACTER SET utf8mb4 NULL,
        `NextRunDate` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CustomReports` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617075551_AddCustomReports') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260617075551_AddCustomReports', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Users` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `UserDivisionRoles` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Transfers` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `TransferApprovals` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `TINs` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Suppliers` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Requests` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `RepairingFirms` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Receipts` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `QueueItems` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `QRNs` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `PurchasingOrders` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `PurchasingOrderItems` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Products` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Notifications` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Maintenances` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `LostItems` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `GRNs` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `GINs` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Divisions` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `DiscountInfos` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `DiscardedNotes` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `CustomReports` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Categories` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Buyers` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `AuditLogs` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `Assets` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `AssetRequests` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `AssetInformings` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `AccPendingItems` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `AccDiscardNotes` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    ALTER TABLE `AccDiscardedItems` ADD `Version` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260617235245_AddConcurrencyVersion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260617235245_AddConcurrencyVersion', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260814114951_AddQueueItemIdToAccPendingItem') THEN

    ALTER TABLE `AccPendingItems` ADD `QueueItemId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260814114951_AddQueueItemIdToAccPendingItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260814114951_AddQueueItemIdToAccPendingItem', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815030215_AddRequestedByToQueueAndAccPendingItems') THEN

    ALTER TABLE `QueueItems` ADD `RequestedById` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815030215_AddRequestedByToQueueAndAccPendingItems') THEN

    ALTER TABLE `QueueItems` ADD `RequestedByName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815030215_AddRequestedByToQueueAndAccPendingItems') THEN

    ALTER TABLE `AccPendingItems` ADD `RequestedById` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815030215_AddRequestedByToQueueAndAccPendingItems') THEN

    ALTER TABLE `AccPendingItems` ADD `RequestedByName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815030215_AddRequestedByToQueueAndAccPendingItems') THEN

    ALTER TABLE `AccDiscardedItems` ADD `RequestedByName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815030215_AddRequestedByToQueueAndAccPendingItems') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260815030215_AddRequestedByToQueueAndAccPendingItems', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815032401_ChangeReceiptAmountToDecimal') THEN

    ALTER TABLE `Receipts` MODIFY COLUMN `Amount` decimal(18,2) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815032401_ChangeReceiptAmountToDecimal') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260815032401_ChangeReceiptAmountToDecimal', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815073000_AddCategoryDepreciationRate') THEN

    ALTER TABLE `Categories` ADD `DepreciationRate` decimal(65,30) NOT NULL DEFAULT 10.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815073000_AddCategoryDepreciationRate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260815073000_AddCategoryDepreciationRate', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815133903_AddAssetRequestProcessorInfo') THEN

    ALTER TABLE `AssetRequests` ADD `ProcessedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815133903_AddAssetRequestProcessorInfo') THEN

    ALTER TABLE `AssetRequests` ADD `ProcessedByName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815133903_AddAssetRequestProcessorInfo') THEN

    ALTER TABLE `AssetRequests` ADD `ProcessedByUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815133903_AddAssetRequestProcessorInfo') THEN

    ALTER TABLE `AssetRequests` ADD `ProcessorRemarks` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815133903_AddAssetRequestProcessorInfo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260815133903_AddAssetRequestProcessorInfo', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815152128_AddAssetRequestRejectionReason') THEN

    ALTER TABLE `AssetRequests` ADD `RejectionReason` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815152128_AddAssetRequestRejectionReason') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260815152128_AddAssetRequestRejectionReason', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815170000_AddDivisionIdToPurchasingOrder') THEN

    ALTER TABLE `PurchasingOrders` ADD `DivisionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815170000_AddDivisionIdToPurchasingOrder') THEN

    CREATE INDEX `IX_PurchasingOrders_DivisionId` ON `PurchasingOrders` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815170000_AddDivisionIdToPurchasingOrder') THEN

    ALTER TABLE `PurchasingOrders` ADD CONSTRAINT `FK_PurchasingOrders_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260815170000_AddDivisionIdToPurchasingOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260815170000_AddDivisionIdToPurchasingOrder', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN

    ALTER TABLE `DiscardedNotes` ADD COLUMN IF NOT EXISTS `RequestedByName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN

    ALTER TABLE `DiscardedNotes` ADD COLUMN IF NOT EXISTS `RequestedByUserId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN

    ALTER TABLE `AssetInformings` ADD COLUMN IF NOT EXISTS `Remarks` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN

    ALTER TABLE `AssetInformings` ADD COLUMN IF NOT EXISTS `TargetEmployeeId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN

    CREATE INDEX IF NOT EXISTS `IX_AssetInformings_TargetEmployeeId` ON `AssetInformings` (`TargetEmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN


                    SET @fk_exists = (
                        SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                        WHERE CONSTRAINT_SCHEMA = DATABASE()
                        AND CONSTRAINT_NAME = 'FK_AssetInformings_Users_TargetEmployeeId'
                        AND TABLE_NAME = 'AssetInformings'
                    );
                    SET @sql = IF(@fk_exists = 0,
                        'ALTER TABLE `AssetInformings` ADD CONSTRAINT `FK_AssetInformings_Users_TargetEmployeeId` FOREIGN KEY (`TargetEmployeeId`) REFERENCES `Users` (`Id`);',
                        'SELECT 1;'
                    );
                    PREPARE stmt FROM @sql;
                    EXECUTE stmt;
                    DEALLOCATE PREPARE stmt;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816104933_AddRequestedByToDiscardedNote') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260816104933_AddRequestedByToDiscardedNote', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816123919_RemoveMaintenanceOriginalRequestFk') THEN

    ALTER TABLE `Maintenances` DROP FOREIGN KEY `FK_Maintenances_Requests_OriginalRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816123919_RemoveMaintenanceOriginalRequestFk') THEN

    ALTER TABLE `Maintenances` DROP INDEX `IX_Maintenances_OriginalRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816123919_RemoveMaintenanceOriginalRequestFk') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260816123919_RemoveMaintenanceOriginalRequestFk', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816140818_AddAssetIdToDiscardedNoteAndAccPendingItem') THEN

    ALTER TABLE `DiscardedNotes` ADD `AssetId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816140818_AddAssetIdToDiscardedNoteAndAccPendingItem') THEN

    ALTER TABLE `AccPendingItems` ADD `AssetId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816140818_AddAssetIdToDiscardedNoteAndAccPendingItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260816140818_AddAssetIdToDiscardedNoteAndAccPendingItem', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816144919_AddQueueItemIdToDiscardedNote') THEN

    ALTER TABLE `DiscardedNotes` ADD `QueueItemId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816144919_AddQueueItemIdToDiscardedNote') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260816144919_AddQueueItemIdToDiscardedNote', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816155115_AddReceiptIdToAccDiscardedItem') THEN

    ALTER TABLE `AccDiscardedItems` ADD `ReceiptId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260816155115_AddReceiptIdToAccDiscardedItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260816155115_AddReceiptIdToAccDiscardedItem', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260817021201_AddExpectedReturnDateToTransfer') THEN

    ALTER TABLE `Transfers` ADD `ExpectedReturnDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260817021201_AddExpectedReturnDateToTransfer') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260817021201_AddExpectedReturnDateToTransfer', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260819012923_AddAssetIdToAssetInforming') THEN

    ALTER TABLE `AssetInformings` ADD `AssetId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260819012923_AddAssetIdToAssetInforming') THEN

    CREATE INDEX `IX_AssetInformings_AssetId` ON `AssetInformings` (`AssetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260819012923_AddAssetIdToAssetInforming') THEN

    ALTER TABLE `AssetInformings` ADD CONSTRAINT `FK_AssetInformings_Assets_AssetId` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260819012923_AddAssetIdToAssetInforming') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260819012923_AddAssetIdToAssetInforming', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    ALTER TABLE `Requests` ADD `PurchasingOrderId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    ALTER TABLE `AssetRequests` ADD `PurchasingOrderId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    CREATE INDEX `IX_Requests_PurchasingOrderId` ON `Requests` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    CREATE INDEX `IX_AssetRequests_PurchasingOrderId` ON `AssetRequests` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    ALTER TABLE `AssetRequests` ADD CONSTRAINT `FK_AssetRequests_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    ALTER TABLE `Requests` ADD CONSTRAINT `FK_Requests_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820084842_AddPurchasingOrderLinkToRequestsAndAssets') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260820084842_AddPurchasingOrderLinkToRequestsAndAssets', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820101517_AddPurchasingOrderIdToAssetInforming') THEN

    ALTER TABLE `AssetInformings` ADD `PurchasingOrderId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820101517_AddPurchasingOrderIdToAssetInforming') THEN

    CREATE INDEX `IX_AssetInformings_PurchasingOrderId` ON `AssetInformings` (`PurchasingOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820101517_AddPurchasingOrderIdToAssetInforming') THEN

    ALTER TABLE `AssetInformings` ADD CONSTRAINT `FK_AssetInformings_PurchasingOrders_PurchasingOrderId` FOREIGN KEY (`PurchasingOrderId`) REFERENCES `PurchasingOrders` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260820101517_AddPurchasingOrderIdToAssetInforming') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260820101517_AddPurchasingOrderIdToAssetInforming', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260823033211_AddAssetIdToLostItemAndAccDiscardedItemIdToBuyer') THEN

    ALTER TABLE `Users` ADD `RequiresOnboarding` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260823033211_AddAssetIdToLostItemAndAccDiscardedItemIdToBuyer') THEN

    ALTER TABLE `LostItems` ADD `AssetId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260823033211_AddAssetIdToLostItemAndAccDiscardedItemIdToBuyer') THEN

    ALTER TABLE `Buyers` ADD `AccDiscardedItemId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260823033211_AddAssetIdToLostItemAndAccDiscardedItemIdToBuyer') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260823033211_AddAssetIdToLostItemAndAccDiscardedItemIdToBuyer', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260823040417_AddBuyerIdToAccPendingItem') THEN

    ALTER TABLE `AccPendingItems` ADD `BuyerId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260823040417_AddBuyerIdToAccPendingItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260823040417_AddBuyerIdToAccPendingItem', '8.0.13');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachment` DROP FOREIGN KEY `FK_AssetAttachment_AssetRequests_AssetRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    CALL POMELO_BEFORE_DROP_PRIMARY_KEY(NULL, 'AssetAttachment');
    ALTER TABLE `AssetAttachment` DROP PRIMARY KEY;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachment` RENAME `AssetAttachments`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachments` RENAME INDEX `IX_AssetAttachment_AssetRequestId` TO `IX_AssetAttachments_AssetRequestId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `AssetCategory` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `AssetName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `DivisionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `ProcessedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `ProcessedByName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `ProcessorRemarks` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `Quantity` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `Reason` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `RejectionReason` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD `SubmittedDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AccPendingItems` ADD `SoldPrice` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AccDiscardedItems` ADD `BuyerId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AccDiscardedItems` ADD `SoldPrice` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachments` ADD `RequestId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachments` ADD CONSTRAINT `PK_AssetAttachments` PRIMARY KEY (`Id`);
    CALL POMELO_AFTER_ADD_PRIMARY_KEY(NULL, 'AssetAttachments', 'Id');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    CREATE INDEX `IX_Requests_DivisionId` ON `Requests` (`DivisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    CREATE INDEX `IX_AssetAttachments_RequestId` ON `AssetAttachments` (`RequestId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachments` ADD CONSTRAINT `FK_AssetAttachments_AssetRequests_AssetRequestId` FOREIGN KEY (`AssetRequestId`) REFERENCES `AssetRequests` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `AssetAttachments` ADD CONSTRAINT `FK_AssetAttachments_Requests_RequestId` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    ALTER TABLE `Requests` ADD CONSTRAINT `FK_Requests_Divisions_DivisionId` FOREIGN KEY (`DivisionId`) REFERENCES `Divisions` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260914115759_SyncModelAndFixDrift') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260914115759_SyncModelAndFixDrift', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

DROP PROCEDURE `POMELO_BEFORE_DROP_PRIMARY_KEY`;

DROP PROCEDURE `POMELO_AFTER_ADD_PRIMARY_KEY`;

