-- MySQL dump 10.13  Distrib 8.0.23, for Win64 (x86_64)
--
-- Host: jeffjin.net    Database: CommonApp
-- ------------------------------------------------------
-- Server version	8.0.23

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Appointments`
--

DROP TABLE IF EXISTS `Appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Appointments` (
  `Id` char(36) NOT NULL,
  `Name` longtext NOT NULL,
  `Phone` longtext NOT NULL,
  `Email` longtext NOT NULL,
  `Description` longtext,
  `StartTime` datetime(6) NOT NULL,
  `EndTime` datetime(6) NOT NULL,
  `Organizer` longtext,
  `LocationId` char(36) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Appointments_CreatedById` (`CreatedById`),
  KEY `IX_Appointments_LocationId` (`LocationId`),
  KEY `IX_Appointments_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Appointments_Locations_LocationId` FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Appointments_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Appointments_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Appointments`
--

LOCK TABLES `Appointments` WRITE;
/*!40000 ALTER TABLE `Appointments` DISABLE KEYS */;
/*!40000 ALTER TABLE `Appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Audios`
--

DROP TABLE IF EXISTS `Audios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Audios` (
  `Id` char(36) NOT NULL,
  `Duration` double NOT NULL,
  `CloudUrl` varchar(512) NOT NULL,
  `EncodedFilePath` varchar(512) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  `Title` varchar(512) DEFAULT NULL,
  `Description` varchar(512) DEFAULT NULL,
  `RawFilePath` varchar(512) NOT NULL,
  `Tags` varchar(512) DEFAULT NULL,
  `CategoryId` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_Audio_CloudUrl` (`CloudUrl`),
  UNIQUE KEY `IX_Audios_RawFilePath` (`RawFilePath`),
  UNIQUE KEY `IX_Audios_EncodedFilePath` (`EncodedFilePath`),
  KEY `IX_Audios_CategoryId` (`CategoryId`),
  KEY `IX_Audios_CreatedById` (`CreatedById`),
  KEY `IX_Audios_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Audios_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Audios_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Audios_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Audios`
--

LOCK TABLES `Audios` WRITE;
/*!40000 ALTER TABLE `Audios` DISABLE KEYS */;
/*!40000 ALTER TABLE `Audios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Categories`
--

DROP TABLE IF EXISTS `Categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Categories` (
  `Id` char(36) NOT NULL,
  `Name` varchar(256) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Categories_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Categories`
--

LOCK TABLES `Categories` WRITE;
/*!40000 ALTER TABLE `Categories` DISABLE KEYS */;
INSERT INTO `Categories` VALUES ('f221a1ba-e9f8-48a7-a333-c23fa5e8f059','Advertisement'),('3c113152-311d-4e0a-a1e3-b247155b29e2','Corals'),('6104d7ef-66f5-4d15-aa32-aedaadc2a2aa','Drama'),('31f2da68-0e1f-4747-b3b4-51457eb081c8','Education'),('61bb0dd8-bf44-4a2a-974f-983b6d613a99','Hobby'),('f0c0e9e4-263b-4070-a9af-55ee896bbc24','Miscellaneous'),('38347ea5-aa84-4e1c-be58-90585e425362','Music Videos'),('64c85aae-6504-4f73-a8d2-ed4cb98ab039','Sports');
/*!40000 ALTER TABLE `Categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `DeviceGroups`
--

DROP TABLE IF EXISTS `DeviceGroups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DeviceGroups` (
  `Id` char(36) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `OrganizationId` char(36) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_DeviceGroups_Name` (`Name`),
  KEY `IX_DeviceGroups_CreatedById` (`CreatedById`),
  KEY `IX_DeviceGroups_OrganizationId` (`OrganizationId`),
  KEY `IX_DeviceGroups_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_DeviceGroups_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_DeviceGroups_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_group_org_id` FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DeviceGroups`
--

LOCK TABLES `DeviceGroups` WRITE;
/*!40000 ALTER TABLE `DeviceGroups` DISABLE KEYS */;
INSERT INTO `DeviceGroups` VALUES ('51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','Office','71a547db-70a8-49c6-a31a-81c80ea2c079','2021-04-12 05:05:36.148992',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `DeviceGroups` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `DeviceStatuses`
--

DROP TABLE IF EXISTS `DeviceStatuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DeviceStatuses` (
  `Id` char(36) NOT NULL,
  `DeviceId` char(36) DEFAULT NULL,
  `Status` longtext,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_DeviceStatuses_CreatedById` (`CreatedById`),
  KEY `IX_DeviceStatuses_DeviceId` (`DeviceId`),
  KEY `IX_DeviceStatuses_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_DeviceStatuses_Devices_DeviceId` FOREIGN KEY (`DeviceId`) REFERENCES `Devices` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_DeviceStatuses_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_DeviceStatuses_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DeviceStatuses`
--

LOCK TABLES `DeviceStatuses` WRITE;
/*!40000 ALTER TABLE `DeviceStatuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `DeviceStatuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Devices`
--

DROP TABLE IF EXISTS `Devices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Devices` (
  `Id` char(36) NOT NULL,
  `SerialNumber` varchar(64) NOT NULL,
  `AssetTag` longtext,
  `DeviceVersion` int NOT NULL,
  `AppVersion` int NOT NULL,
  `ActivatedOn` datetime(6) DEFAULT NULL,
  `DeviceGroupId` char(36) DEFAULT NULL,
  `LocationId` char(36) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Devices_SerialNumber` (`SerialNumber`),
  KEY `IX_Devices_CreatedById` (`CreatedById`),
  KEY `IX_Devices_DeviceGroupId` (`DeviceGroupId`),
  KEY `IX_Devices_LocationId` (`LocationId`),
  KEY `IX_Devices_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Devices_DeviceGroups_DeviceGroupId` FOREIGN KEY (`DeviceGroupId`) REFERENCES `DeviceGroups` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Devices_Locations_LocationId` FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Devices_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Devices_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Devices`
--

LOCK TABLES `Devices` WRITE;
/*!40000 ALTER TABLE `Devices` DISABLE KEYS */;
INSERT INTO `Devices` VALUES ('61d8124a-2302-49dc-80d1-095375ae6937','J5909005E4MFB','AD004',1,1,'2021-04-12 05:05:36.845478','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','51aeeb7d-8573-4bda-80df-4063a0b86b4f','2021-04-12 05:05:36.845478',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('acc68490-4bc5-42e2-bee3-003e45aa7bbc','00000000954f0c90','AD002',1,1,'2021-04-12 05:05:36.845473','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','51aeeb7d-8573-4bda-80df-4063a0b86b4f','2021-04-12 05:05:36.845473',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('ad1047ca-287e-46f5-91b9-1703c700b018','0000000029fa61cc','AD001',1,1,'2021-04-12 05:05:36.845213','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','51aeeb7d-8573-4bda-80df-4063a0b86b4f','2021-04-12 05:05:36.844948',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('c65cc2c9-9aa1-48ca-a32f-8e9a748c77b9','000000004095f08a','AD003',1,1,'2021-04-12 05:05:36.845476','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','51aeeb7d-8573-4bda-80df-4063a0b86b4f','2021-04-12 05:05:36.845476',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `Devices` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Images`
--

DROP TABLE IF EXISTS `Images`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Images` (
  `Id` char(36) NOT NULL,
  `CloudUrl` varchar(512) DEFAULT NULL,
  `ProductId` char(36) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  `Title` varchar(512) DEFAULT NULL,
  `Description` varchar(512) DEFAULT NULL,
  `RawFilePath` varchar(512) NOT NULL,
  `Tags` varchar(512) DEFAULT NULL,
  `CategoryId` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Images_RawFilePath` (`RawFilePath`),
  UNIQUE KEY `IX_Images_CloudUrl` (`CloudUrl`),
  KEY `IX_Images_CategoryId` (`CategoryId`),
  KEY `IX_Images_CreatedById` (`CreatedById`),
  KEY `IX_Images_ProductId` (`ProductId`),
  KEY `IX_Images_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Images_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Images_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Images_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Images_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Images`
--

LOCK TABLES `Images` WRITE;
/*!40000 ALTER TABLE `Images` DISABLE KEYS */;
INSERT INTO `Images` VALUES ('0e34566f-a4dd-4181-95bb-fe9b131936d0','https://jeffjin.net/images/samples/op2.jpg','c7a247a0-f9d5-4e49-ad28-130995854e99','2021-04-12 05:05:38.012463',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Orange Passion','A classic piece with beautiful orange polyps that contrasts its striking blue base','/home/vsftpd/ftpuser/images/samples/op2.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('106e2031-9349-4ee2-9d46-3591b44e837e','https://jeffjin.net/images/samples/redplanet.jpg',NULL,'2021-04-12 05:05:38.013112',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Red Planet','Polyp coloration on this coral is a deep red and the new growth is light pink to white.','/home/vsftpd/ftpuser/images/samples/redplanet.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('181ffd0a-2f30-41e7-85f9-9e6abbdd7945','https://jeffjin.net/images/samples/adidas1.png',NULL,'2021-04-12 05:05:38.013568',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','adidas1','adidas1','/home/vsftpd/ftpuser/images/samples/adidas1.png',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('2125ced4-8d6b-4845-9fa4-ce08badbb8a1','https://jeffjin.net/images/samples/purple_bonsai.jpg','6abc7d47-f5f1-4202-bd83-6543f0699f1c','2021-04-12 05:05:38.012939',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Purple Bonsai','The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.','/home/vsftpd/ftpuser/images/samples/purple_bonsai.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('2bb4447a-2073-4bac-878f-eeb62c14b019','https://jeffjin.net/images/samples/loom.jpg','ad3b2422-44aa-409b-b25b-01ff6fa8af14','2021-04-12 05:05:38.012270',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Rainbow Loom','Truly cool  Reef Raft captive.  Kind of a pink/red with purple and yellow at the tips.  Definitely a standout.','/home/vsftpd/ftpuser/images/samples/loom.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('359125fb-355b-48e3-a0b4-aba23633b20f','https://jeffjin.net/images/samples/galaxea_fascicularis.jpg','591c757a-b1bb-47b6-9a6f-eee6d5e5f79b','2021-04-12 05:05:38.013266',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Galaxea fascicularis','The Galaxy Coral Galaxea fascicularis is a popular large polyp stony (LPS) coral that many reef enthusiasts have or want in their collection.','/home/vsftpd/ftpuser/images/samples/galaxea_fascicularis.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('3d21a821-342b-47ab-8fc7-9f8b6065f7a5','https://jeffjin.net/images/samples/benz1.jpeg',NULL,'2021-04-12 05:05:38.013974',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','benz1','benz1','/home/vsftpd/ftpuser/images/samples/benz1.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('433bcd68-cc4b-480b-98ce-c0b516c624bc','https://jeffjin.net/images/samples/adidas2.png',NULL,'2021-04-12 05:05:38.013654',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','adidas2','adidas2','/home/vsftpd/ftpuser/images/samples/adidas2.png',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('5aa7fb0b-80a0-41d3-9a74-ecb2be7aefd5','https://jeffjin.net/images/samples/op1.jpg','c7a247a0-f9d5-4e49-ad28-130995854e99','2021-04-12 05:05:38.012548',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Orange Passion','A classic piece with beautiful orange polyps that contrasts its striking blue base','/home/vsftpd/ftpuser/images/samples/op1.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('5df52c77-9ff7-404e-afe4-99e47c740913','https://jeffjin.net/images/samples/nike-adapt-bb.jpeg',NULL,'2021-04-12 05:05:38.013338',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','Nike Adapt BB','Nike Adapt BB','/home/vsftpd/ftpuser/images/samples/nike-adapt-bb.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('675560a0-e7f7-477f-99d7-d2f35479cab6','https://jeffjin.net/images/samples/digitata2.jpg','0d581e90-cf66-437f-89e3-9b343258b1af','2021-04-12 05:05:38.012788',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Fire Digi','The Forest Fire Montipora digitata has bright red/orange polyps and either a light green or teal base','/home/vsftpd/ftpuser/images/samples/digitata2.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('7050a89c-44e3-411b-a383-3c8afc9f6924','https://jeffjin.net/images/samples/audi2.jpeg',NULL,'2021-04-12 05:05:38.013881',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','audi2','audi2','/home/vsftpd/ftpuser/images/samples/audi2.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('705fc339-72d1-4c34-ae3e-b633ff3fee9b','https://jeffjin.net/images/samples/green_slimer.jpeg','e0a558cc-6f85-4bb8-88f8-57eea206caa6','2021-04-12 05:05:38.012635',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Green Slimer','With thick branches, separate, untidy and with colours that vary from yellowish green to pale brown acropora yongei is an easily recognizable species. This fact, coupled with its incredible abundance, makes this coral a highly ubiquitous animal','/home/vsftpd/ftpuser/images/samples/green_slimer.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('9329e7ad-3b72-4b8a-a995-659f926e4779','https://jeffjin.net/images/samples/nike2015.png',NULL,'2021-04-12 05:05:38.013416',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','Nike 2015','Nike 2015','/home/vsftpd/ftpuser/images/samples/nike2015.png',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('9474ca79-41bd-4030-a451-2e52a7c722c0','https://jeffjin.net/images/samples/adidas3.jpeg',NULL,'2021-04-12 05:05:38.013733',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','adidas3','adidas3','/home/vsftpd/ftpuser/images/samples/adidas3.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('9f8e8f07-5af6-4006-8a99-6154e282d547','https://jeffjin.net/images/samples/home_wrecker.jpg',NULL,'2021-04-12 05:05:38.012710',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Home Wrecker','Home Wrecker Description','/home/vsftpd/ftpuser/images/samples/home_wrecker.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('a8e22986-8c3f-4b52-9c2d-9454bd9d2f35','https://jeffjin.net/images/samples/nike2016.png',NULL,'2021-04-12 05:05:38.013496',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','Nike 2016','Nike 2016','/home/vsftpd/ftpuser/images/samples/nike2016.png',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('b477640b-256a-4bd8-a392-c300986dfd05','https://jeffjin.net/images/samples/wd.jpg','17ac8f61-4659-4b64-8e43-cacc1956ceaf','2021-04-12 05:05:37.994588',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Walt Disney','Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank','/home/vsftpd/ftpuser/images/samples/wd.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('c690b3c2-14d7-4c60-b3e8-2d7446e0de62','https://jeffjin.net/images/samples/cherry_blossom.jpg','da53c89b-5fe4-460d-b9ff-ab1ed33f75cb','2021-04-12 05:05:38.013187',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Cherry Blossom','This Acropora is pretty common on many inshore reefs, but it was prolific on the millie spot, were Acropora were dominant.','/home/vsftpd/ftpuser/images/samples/cherry_blossom.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('d8019f02-1496-4127-93bf-65bab0d2c621','https://jeffjin.net/images/samples/op3.jpg','c7a247a0-f9d5-4e49-ad28-130995854e99','2021-04-12 05:05:38.012385',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Orange Passion','A classic piece with beautiful orange polyps that contrasts its striking blue base','/home/vsftpd/ftpuser/images/samples/op3.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('de0bac86-29e1-4ac0-b717-1d6f488f7f38','https://jeffjin.net/images/samples/audi.jpeg',NULL,'2021-04-12 05:05:38.013804',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','audi','audi','/home/vsftpd/ftpuser/images/samples/audi.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('deb026a0-63a0-4a52-a89f-57bf1dd20999','https://jeffjin.net/images/samples/digitata3.jpg','0d581e90-cf66-437f-89e3-9b343258b1af','2021-04-12 05:05:38.012867',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Fire Digi','he Forest Fire Montipora digitata has bright red/orange polyps and either a light green or teal base','/home/vsftpd/ftpuser/images/samples/digitata3.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('ec3b8ba4-1f19-4586-a525-1ee61c4f7b76','https://jeffjin.net/images/samples/benz2.jpeg',NULL,'2021-04-12 05:05:38.014050',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','benz2','benz2','/home/vsftpd/ftpuser/images/samples/benz2.jpeg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('f10f56a0-f709-4be1-97cf-55222cb851b1','https://jeffjin.net/images/samples/purple_bonsai1.jpg','6abc7d47-f5f1-4202-bd83-6543f0699f1c','2021-04-12 05:05:38.013034',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Purple Bonsai','The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.','/home/vsftpd/ftpuser/images/samples/purple_bonsai1.jpg',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2');
/*!40000 ALTER TABLE `Images` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Licenses`
--

DROP TABLE IF EXISTS `Licenses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Licenses` (
  `Id` char(36) NOT NULL,
  `DeviceId` char(36) DEFAULT NULL,
  `Type` longtext,
  `ExpireOn` datetime(6) NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Licenses_CreatedById` (`CreatedById`),
  KEY `IX_Licenses_DeviceId` (`DeviceId`),
  KEY `IX_Licenses_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Licenses_Devices_DeviceId` FOREIGN KEY (`DeviceId`) REFERENCES `Devices` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Licenses_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Licenses_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Licenses`
--

LOCK TABLES `Licenses` WRITE;
/*!40000 ALTER TABLE `Licenses` DISABLE KEYS */;
INSERT INTO `Licenses` VALUES ('7296026f-9a62-4253-a348-a75ca61b1d30','ad1047ca-287e-46f5-91b9-1703c700b018','Trial','2021-07-12 05:05:36.349972','2021-04-12 05:05:36.349972',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('a785fdaf-db40-4fab-bfbe-0e2bdf9f2f4c','acc68490-4bc5-42e2-bee3-003e45aa7bbc','Trial','2021-07-12 05:05:36.349761','2021-04-12 05:05:36.349761',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('cf736f55-fd2c-4dd3-8614-7d99c8e64e2e','c65cc2c9-9aa1-48ca-a32f-8e9a748c77b9','Trial','2021-07-12 05:05:36.337548','2021-04-12 05:05:36.337356',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('dacdb976-8b20-4505-85c0-483df020c1b4','61d8124a-2302-49dc-80d1-095375ae6937','Trial','2021-07-12 05:05:36.349909','2021-04-12 05:05:36.349908',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `Licenses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Locations`
--

DROP TABLE IF EXISTS `Locations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Locations` (
  `Id` char(36) NOT NULL,
  `Address` longtext NOT NULL,
  `Locale` longtext,
  `TimezoneOffset` double NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Locations_CreatedById` (`CreatedById`),
  KEY `IX_Locations_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Locations_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Locations_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Locations`
--

LOCK TABLES `Locations` WRITE;
/*!40000 ALTER TABLE `Locations` DISABLE KEYS */;
INSERT INTO `Locations` VALUES ('51aeeb7d-8573-4bda-80df-4063a0b86b4f','176 District Ave, Vaughan, Ontario, Canada, L6A 0Y3','en-ca',-5,'2021-04-12 05:05:36.522036',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('61c80122-323a-445d-a770-433e66d20320','Vancouver, British Columbia, Canada, V5K 0A4','en-ca',-8,'2021-04-12 05:05:36.522263',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('621f77f4-bd4f-410f-8d9a-a6632271fcd3','Flushing, New York, USA, 11355','en-us',-5,'2021-04-12 05:05:36.522262',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `Locations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `MergeRecords`
--

DROP TABLE IF EXISTS `MergeRecords`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `MergeRecords` (
  `Id` char(36) NOT NULL,
  `MergeType` longtext NOT NULL,
  `AssetId1` char(36) NOT NULL,
  `AssetId2` char(36) NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MergeRecords_CreatedById` (`CreatedById`),
  KEY `IX_MergeRecords_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_MergeRecords_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_MergeRecords_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `MergeRecords`
--

LOCK TABLES `MergeRecords` WRITE;
/*!40000 ALTER TABLE `MergeRecords` DISABLE KEYS */;
/*!40000 ALTER TABLE `MergeRecords` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `OrderItems`
--

DROP TABLE IF EXISTS `OrderItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `OrderItems` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `ProductId` char(36) DEFAULT NULL,
  `OrderQuantity` int NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_OrderItems_CreatedById` (`CreatedById`),
  KEY `IX_OrderItems_OrderId` (`OrderId`),
  KEY `IX_OrderItems_ProductId` (`ProductId`),
  KEY `IX_OrderItems_UpdatedById` (`UpdatedById`),
  CONSTRAINT `fk_order_id` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_OrderItems_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_OrderItems_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_OrderItems_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `OrderItems`
--

LOCK TABLES `OrderItems` WRITE;
/*!40000 ALTER TABLE `OrderItems` DISABLE KEYS */;
/*!40000 ALTER TABLE `OrderItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Orders`
--

DROP TABLE IF EXISTS `Orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Orders` (
  `Id` char(36) NOT NULL,
  `Discount` double NOT NULL,
  `CustomerId` varchar(255) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Orders_CreatedById` (`CreatedById`),
  KEY `IX_Orders_CustomerId` (`CustomerId`),
  KEY `IX_Orders_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Orders_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Orders_Users_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Orders_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Orders`
--

LOCK TABLES `Orders` WRITE;
/*!40000 ALTER TABLE `Orders` DISABLE KEYS */;
/*!40000 ALTER TABLE `Orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Organizations`
--

DROP TABLE IF EXISTS `Organizations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Organizations` (
  `Id` char(36) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Organizations_Name` (`Name`),
  KEY `IX_Organizations_CreatedById` (`CreatedById`),
  KEY `IX_Organizations_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Organizations_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Organizations_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Organizations`
--

LOCK TABLES `Organizations` WRITE;
/*!40000 ALTER TABLE `Organizations` DISABLE KEYS */;
INSERT INTO `Organizations` VALUES ('71a547db-70a8-49c6-a31a-81c80ea2c079','kiosho','2021-04-12 05:05:35.801951',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `Organizations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Payments`
--

DROP TABLE IF EXISTS `Payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Payments` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) DEFAULT NULL,
  `TransactionId` varchar(255) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Payments_TransactionId` (`TransactionId`),
  KEY `IX_Payments_CreatedById` (`CreatedById`),
  KEY `IX_Payments_OrderId` (`OrderId`),
  KEY `IX_Payments_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Payments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Payments_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Payments_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Payments`
--

LOCK TABLES `Payments` WRITE;
/*!40000 ALTER TABLE `Payments` DISABLE KEYS */;
/*!40000 ALTER TABLE `Payments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PlaylistGroups`
--

DROP TABLE IF EXISTS `PlaylistGroups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `PlaylistGroups` (
  `Id` char(36) NOT NULL,
  `DeviceGroupId` char(36) NOT NULL,
  `PlaylistId` char(36) NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PlaylistGroups_CreatedById` (`CreatedById`),
  KEY `IX_PlaylistGroups_DeviceGroupId` (`DeviceGroupId`),
  KEY `IX_PlaylistGroups_PlaylistId` (`PlaylistId`),
  KEY `IX_PlaylistGroups_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_PlaylistGroups_DeviceGroups_DeviceGroupId` FOREIGN KEY (`DeviceGroupId`) REFERENCES `DeviceGroups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_PlaylistGroups_Playlists_PlaylistId` FOREIGN KEY (`PlaylistId`) REFERENCES `Playlists` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_PlaylistGroups_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_PlaylistGroups_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PlaylistGroups`
--

LOCK TABLES `PlaylistGroups` WRITE;
/*!40000 ALTER TABLE `PlaylistGroups` DISABLE KEYS */;
INSERT INTO `PlaylistGroups` VALUES ('7c2e71ca-eaf3-4969-b3fa-0b1e8a37216c','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','b6bef8d1-1ea2-4b9e-87c0-0c3825406f1d','2021-04-12 05:05:37.603286',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('9595edcb-1b2e-4d5f-badc-aac9194cce99','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','4f5c5aa5-c16a-44af-af22-e110dfb32baf','2021-04-12 05:05:37.694337',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('bb014234-c727-4053-8fa2-1c4e7f7a4abf','51d097d5-2f2d-4bd8-aa2d-bff5cd71b606','bbc678bf-0c6e-4614-9448-3bc8e4d63aa0','2021-04-12 05:05:37.693926',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `PlaylistGroups` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PlaylistItems`
--

DROP TABLE IF EXISTS `PlaylistItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `PlaylistItems` (
  `Id` char(36) NOT NULL,
  `SubPlaylistId` char(36) NOT NULL,
  `MediaAssetId` char(36) NOT NULL,
  `AssetDiscriminator` longtext NOT NULL,
  `Duration` int NOT NULL,
  `Index` int NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PlaylistItems_CreatedById` (`CreatedById`),
  KEY `IX_PlaylistItems_SubPlaylistId` (`SubPlaylistId`),
  KEY `IX_PlaylistItems_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_PlaylistItems_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_PlaylistItems_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_sub_playlist_id` FOREIGN KEY (`SubPlaylistId`) REFERENCES `SubPlaylists` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PlaylistItems`
--

LOCK TABLES `PlaylistItems` WRITE;
/*!40000 ALTER TABLE `PlaylistItems` DISABLE KEYS */;
/*!40000 ALTER TABLE `PlaylistItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Playlists`
--

DROP TABLE IF EXISTS `Playlists`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Playlists` (
  `Id` char(36) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) NOT NULL,
  `StartTime` int NOT NULL,
  `EndTime` int NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Playlists_Name` (`Name`),
  KEY `IX_Playlists_CreatedById` (`CreatedById`),
  KEY `IX_Playlists_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Playlists_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Playlists_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Playlists`
--

LOCK TABLES `Playlists` WRITE;
/*!40000 ALTER TABLE `Playlists` DISABLE KEYS */;
INSERT INTO `Playlists` VALUES ('4f5c5aa5-c16a-44af-af22-e110dfb32baf','Standard 3 Section','2021-04-22 05:05:37.694330','2022-02-12 05:05:37.694332',0,0,'2021-04-12 05:05:37.694330',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('b6bef8d1-1ea2-4b9e-87c0-0c3825406f1d','Basic (No PiP)','2021-04-22 05:05:37.602529','2022-02-12 05:05:37.602645',0,0,'2021-04-12 05:05:37.602449',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('bbc678bf-0c6e-4614-9448-3bc8e4d63aa0','Top Bottom','2021-04-22 05:05:37.693917','2022-02-12 05:05:37.693921',0,0,'2021-04-12 05:05:37.693916',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `Playlists` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Products`
--

DROP TABLE IF EXISTS `Products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Products` (
  `Id` char(36) NOT NULL,
  `Title` varchar(255) DEFAULT NULL,
  `Description` longtext,
  `Price` double NOT NULL,
  `Brand` longtext,
  `Inventory` int NOT NULL,
  `ProductCategoryId` char(36) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Products_Title` (`Title`),
  KEY `IX_Products_CreatedById` (`CreatedById`),
  KEY `IX_Products_ProductCategoryId` (`ProductCategoryId`),
  KEY `IX_Products_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Products_SubCategories_ProductCategoryId` FOREIGN KEY (`ProductCategoryId`) REFERENCES `SubCategories` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Products_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Products_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Products`
--

LOCK TABLES `Products` WRITE;
/*!40000 ALTER TABLE `Products` DISABLE KEYS */;
INSERT INTO `Products` VALUES ('0d581e90-cf66-437f-89e3-9b343258b1af','Fire Digi','The Forest Fire Montipora digitata has bright red/orange polyps and either a light green or teal base',9.9,'ORA',100,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.695967',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('17ac8f61-4659-4b64-8e43-cacc1956ceaf','Walt Disney','Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank',100,'ORA',10,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.659602',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('591c757a-b1bb-47b6-9a6f-eee6d5e5f79b','Galaxea fascicularis','The Galaxy Coral Galaxea fascicularis is a popular large polyp stony (LPS) coral that many reef enthusiasts have or want in their collection.',150,'ORA',10,'03c49bf1-c9cb-4b0c-a90f-4fb8765b5f31','2021-04-12 05:05:38.694008',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('6abc7d47-f5f1-4202-bd83-6543f0699f1c','Purple Bonsai','The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.',99,'ORA',44,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.695352',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('ad3b2422-44aa-409b-b25b-01ff6fa8af14','Rainbow Loom','Truly cool  Reef Raft captive.  Kind of a pink/red with purple and yellow at the tips.  Definitely a standout.',199,'ORA',20,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.695674',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('c7a247a0-f9d5-4e49-ad28-130995854e99','Orange Passion','A classic piece with beautiful orange polyps that contrasts its striking blue base',128,'ORA',2,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.694981',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('da53c89b-5fe4-460d-b9ff-ab1ed33f75cb','Cherry Blossom','Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank',99,'ORA',10,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.694681',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('e0a558cc-6f85-4bb8-88f8-57eea206caa6','Green Slimer','With thick branches, separate, untidy and with colours that vary from yellowish green to pale brown acropora yongei is an easily recognizable species. ',40,'ORA',10,'f65c0642-6831-45c1-a711-0e5d72f57efe','2021-04-12 05:05:38.694362',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `Products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Records`
--

DROP TABLE IF EXISTS `Records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Records` (
  `Id` char(36) NOT NULL,
  `MediaAssetId` char(36) NOT NULL,
  `DeviceSerialNumber` longtext NOT NULL,
  `IpAddress` longtext,
  `StartedOn` datetime(6) NOT NULL,
  `EndedOn` datetime(6) NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Records_CreatedById` (`CreatedById`),
  KEY `IX_Records_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Records_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Records_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Records`
--

LOCK TABLES `Records` WRITE;
/*!40000 ALTER TABLE `Records` DISABLE KEYS */;
/*!40000 ALTER TABLE `Records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `RoleClaims`
--

DROP TABLE IF EXISTS `RoleClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `RoleClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` varchar(255) NOT NULL,
  `ClaimType` longtext,
  `ClaimValue` longtext,
  PRIMARY KEY (`Id`),
  KEY `IX_RoleClaims_RoleId` (`RoleId`),
  CONSTRAINT `FK_RoleClaims_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `RoleClaims`
--

LOCK TABLES `RoleClaims` WRITE;
/*!40000 ALTER TABLE `RoleClaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `RoleClaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Roles`
--

DROP TABLE IF EXISTS `Roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Roles` (
  `Id` varchar(255) NOT NULL,
  `Name` varchar(256) DEFAULT NULL,
  `NormalizedName` varchar(256) DEFAULT NULL,
  `ConcurrencyStamp` longtext,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Roles`
--

LOCK TABLES `Roles` WRITE;
/*!40000 ALTER TABLE `Roles` DISABLE KEYS */;
INSERT INTO `Roles` VALUES ('87d737b0-3a3d-4ba8-9895-9c8f83fe2fd2','Manager','MANAGER','2cd1d42e-76f6-4f7f-8df2-95df9c501a14'),('ac3bd15e-584c-4801-8de8-3ce38f12acd1','User','USER','d37ef306-97f0-43ee-8179-3ab77110c327'),('cd801156-1616-49b7-94f5-d87d56063618','Admin','ADMIN','7be84d6c-9dd0-4f02-9787-f5d849deea4a');
/*!40000 ALTER TABLE `Roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `SubCategories`
--

DROP TABLE IF EXISTS `SubCategories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `SubCategories` (
  `Id` char(36) NOT NULL,
  `Name` varchar(256) NOT NULL,
  `CategoryId` char(36) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_SubCategories_CategoryId` (`CategoryId`),
  CONSTRAINT `fk_category_id` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `SubCategories`
--

LOCK TABLES `SubCategories` WRITE;
/*!40000 ALTER TABLE `SubCategories` DISABLE KEYS */;
INSERT INTO `SubCategories` VALUES ('03c49bf1-c9cb-4b0c-a90f-4fb8765b5f31','LPS','3c113152-311d-4e0a-a1e3-b247155b29e2'),('7f15b333-ee4c-4c7a-ab7b-81d9767ff430','Softies','3c113152-311d-4e0a-a1e3-b247155b29e2'),('f65c0642-6831-45c1-a711-0e5d72f57efe','SPS','3c113152-311d-4e0a-a1e3-b247155b29e2');
/*!40000 ALTER TABLE `SubCategories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `SubPlaylists`
--

DROP TABLE IF EXISTS `SubPlaylists`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `SubPlaylists` (
  `Id` char(36) NOT NULL,
  `PlaylistId` char(36) NOT NULL,
  `PositionX` int NOT NULL,
  `PositionY` int NOT NULL,
  `Width` int NOT NULL,
  `Height` int NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_SubPlaylists_CreatedById` (`CreatedById`),
  KEY `IX_SubPlaylists_PlaylistId` (`PlaylistId`),
  KEY `IX_SubPlaylists_UpdatedById` (`UpdatedById`),
  CONSTRAINT `fk_playlist_id` FOREIGN KEY (`PlaylistId`) REFERENCES `Playlists` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_SubPlaylists_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_SubPlaylists_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `SubPlaylists`
--

LOCK TABLES `SubPlaylists` WRITE;
/*!40000 ALTER TABLE `SubPlaylists` DISABLE KEYS */;
INSERT INTO `SubPlaylists` VALUES ('400c0af4-407d-4b22-a703-e4f560ec4989','bbc678bf-0c6e-4614-9448-3bc8e4d63aa0',20,0,80,80,'2021-04-12 05:05:37.693923',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('6c8773af-fe8d-4be4-a057-987221bc1ff3','4f5c5aa5-c16a-44af-af22-e110dfb32baf',0,0,20,80,'2021-04-12 05:05:37.694335',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('a73689f3-3110-46a0-8954-9f9007f16b40','4f5c5aa5-c16a-44af-af22-e110dfb32baf',0,80,100,20,'2021-04-12 05:05:37.694336',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('b1f289a1-2c38-4f4e-9981-8da9af2e8102','4f5c5aa5-c16a-44af-af22-e110dfb32baf',20,0,80,80,'2021-04-12 05:05:37.694333',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('d22a9ef6-c961-4ec5-acf8-d74401adf811','b6bef8d1-1ea2-4b9e-87c0-0c3825406f1d',0,0,100,100,'2021-04-12 05:05:37.602922',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78'),('da062217-baea-4900-862c-993088fd7163','bbc678bf-0c6e-4614-9448-3bc8e4d63aa0',0,80,100,20,'2021-04-12 05:05:37.693925',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78');
/*!40000 ALTER TABLE `SubPlaylists` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserClaims`
--

DROP TABLE IF EXISTS `UserClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(255) NOT NULL,
  `ClaimType` varchar(150) DEFAULT NULL,
  `ClaimValue` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserClaims_UserId` (`UserId`),
  CONSTRAINT `FK_UserClaims_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserClaims`
--

LOCK TABLES `UserClaims` WRITE;
/*!40000 ALTER TABLE `UserClaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `UserClaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserLogins`
--

DROP TABLE IF EXISTS `UserLogins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserLogins` (
  `LoginProvider` varchar(255) NOT NULL,
  `ProviderKey` varchar(255) NOT NULL,
  `ProviderDisplayName` longtext,
  `UserId` varchar(255) NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_UserLogins_UserId` (`UserId`),
  CONSTRAINT `FK_UserLogins_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserLogins`
--

LOCK TABLES `UserLogins` WRITE;
/*!40000 ALTER TABLE `UserLogins` DISABLE KEYS */;
/*!40000 ALTER TABLE `UserLogins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserRoles`
--

DROP TABLE IF EXISTS `UserRoles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserRoles` (
  `UserId` varchar(255) NOT NULL,
  `RoleId` varchar(255) NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_UserRoles_RoleId` (`RoleId`),
  CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserRoles`
--

LOCK TABLES `UserRoles` WRITE;
/*!40000 ALTER TABLE `UserRoles` DISABLE KEYS */;
INSERT INTO `UserRoles` VALUES ('8726c4d9-ff92-479a-a73e-1f7faae85b78','87d737b0-3a3d-4ba8-9895-9c8f83fe2fd2'),('1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','ac3bd15e-584c-4801-8de8-3ce38f12acd1'),('8726c4d9-ff92-479a-a73e-1f7faae85b78','ac3bd15e-584c-4801-8de8-3ce38f12acd1'),('8726c4d9-ff92-479a-a73e-1f7faae85b78','cd801156-1616-49b7-94f5-d87d56063618');
/*!40000 ALTER TABLE `UserRoles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserTokens`
--

DROP TABLE IF EXISTS `UserTokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserTokens` (
  `UserId` varchar(255) NOT NULL,
  `LoginProvider` varchar(255) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Value` longtext,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `FK_UserTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserTokens`
--

LOCK TABLES `UserTokens` WRITE;
/*!40000 ALTER TABLE `UserTokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `UserTokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `Id` varchar(255) NOT NULL,
  `ProfileLogo` longtext,
  `OrganizationId` char(36) DEFAULT NULL,
  `UserName` varchar(256) DEFAULT NULL,
  `NormalizedUserName` varchar(256) DEFAULT NULL,
  `Email` varchar(256) NOT NULL,
  `NormalizedEmail` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` varchar(500) DEFAULT NULL,
  `SecurityStamp` longtext,
  `ConcurrencyStamp` varchar(500) DEFAULT NULL,
  `PhoneNumber` varchar(50) DEFAULT NULL,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_Email` (`Email`),
  UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  KEY `EmailIndex` (`NormalizedEmail`),
  KEY `IX_Users_OrganizationId` (`OrganizationId`),
  CONSTRAINT `fk_user_org_id` FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Users`
--

LOCK TABLES `Users` WRITE;
/*!40000 ALTER TABLE `Users` DISABLE KEYS */;
INSERT INTO `Users` VALUES ('1d4f0a64-fbc1-4f44-82ac-d52de65a75a2',NULL,'71a547db-70a8-49c6-a31a-81c80ea2c079','jeff@jeffjin.com','JEFF@JEFFJIN.COM','jeff@jeffjin.com','JEFF@JEFFJIN.COM',1,'AQAAAAEAACcQAAAAELDCL0qYke4KxD3jUXd88T3b7WBJPVm8cIuqEbSVbvouPxpiurWWJQ/Aq3QGxer5EQ==','7K3IK2N74ZJJBHQSNKA2FBZHPZ7DX6BZ','26d30625-b00d-46de-b534-af3eff32fbdb',NULL,0,0,NULL,1,0),('8726c4d9-ff92-479a-a73e-1f7faae85b78',NULL,'71a547db-70a8-49c6-a31a-81c80ea2c079','admin@eworkspace.ca','ADMIN@EWORKSPACE.CA','admin@eworkspace.ca','ADMIN@EWORKSPACE.CA',1,'AQAAAAEAACcQAAAAEGm+/wAR19h8wn5gEQqFQ+bsRmzuzQ3m8gOLm0pHW78rRh4zBhImyZppEY45a4kRUw==','LTV3LDAECB3Y5A2H24XSFNMNRZMU5RMC','e6076c2a-e139-42cc-accc-c987f6e1bc07',NULL,0,0,NULL,1,0);
/*!40000 ALTER TABLE `Users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Videos`
--

DROP TABLE IF EXISTS `Videos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Videos` (
  `Id` char(36) NOT NULL,
  `Duration` double NOT NULL,
  `ThumbnailLink` varchar(512) DEFAULT NULL,
  `VodVideoUrl` varchar(512) DEFAULT NULL,
  `EncodedVideoPath` varchar(512) DEFAULT NULL,
  `ProgressiveVideoUrl` varchar(512) DEFAULT NULL,
  `ProductId` char(36) DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `UpdatedById` varchar(255) DEFAULT NULL,
  `CreatedById` varchar(255) DEFAULT NULL,
  `Title` varchar(512) DEFAULT NULL,
  `Description` varchar(512) DEFAULT NULL,
  `RawFilePath` varchar(512) NOT NULL,
  `Tags` varchar(512) DEFAULT NULL,
  `CategoryId` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Videos_RawFilePath` (`RawFilePath`),
  UNIQUE KEY `IX_Videos_ThumbnailLink` (`ThumbnailLink`),
  UNIQUE KEY `IX_Videos_VodVideoUrl` (`VodVideoUrl`),
  KEY `IX_Videos_CategoryId` (`CategoryId`),
  KEY `IX_Videos_CreatedById` (`CreatedById`),
  KEY `IX_Videos_ProductId` (`ProductId`),
  KEY `IX_Videos_UpdatedById` (`UpdatedById`),
  CONSTRAINT `FK_Videos_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Videos_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Videos_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Videos_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Videos`
--

LOCK TABLES `Videos` WRITE;
/*!40000 ALTER TABLE `Videos` DISABLE KEYS */;
INSERT INTO `Videos` VALUES ('00337672-4c85-4fee-b7fc-4c72db19f68c',200,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000002/thumbnails/Purple_Bonsai.jpg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000002/Purple_Bonsai.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000002/Purple_Bonsai.mp4',NULL,'2021-04-12 05:05:38.307312',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Purple  Bonsai','The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000002/Purple_Bonsai.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('0a612e2b-b6ba-4c96-a892-01185a91919f',130,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000005/thumbnails/nike-adapt-bb.jpeg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000005/NIKE MAG-1.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000005/NIKE MAG-1.mp4',NULL,'2021-04-12 05:05:38.307614',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','nike-adapt-bb','nike-adapt-bb','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000005/NIKE MAG-1.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('0d66a9d5-0556-46f6-ad57-7fca2e84b1f7',90,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000011/thumbnails/nike2016.png','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000011/nike1.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000011/nike1.mp4',NULL,'2021-04-12 05:05:38.308950',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','nike1','nike1','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000011/nike1.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('15b4dbb0-7560-4e99-b381-b2c70a0aa181',88,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000004/thumbnails/benz2.jpeg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000004/Mercedes-Benz1.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000004/Mercedes-Benz1.mp4',NULL,'2021-04-12 05:05:38.307527',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','Mercedes-Benz','Mercedes-Benz','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000004/Mercedes-Benz1.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('21f287b6-4312-43b0-b95f-0b862f897fdf',211,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000010/thumbnails/adidas3.jpeg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000010/adidas3.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000010/adidas3.mp4',NULL,'2021-04-12 05:05:38.308844',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','adidas3','adidas3','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000010/adidas3.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('2c37e09c-eea7-4a4e-ae49-1bc93d1b6285',80,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000003/thumbnails/Orange_Passion.jpg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000003/Orange_Passion.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000003/Orange_Passion.mp4','c7a247a0-f9d5-4e49-ad28-130995854e99','2021-04-12 05:05:38.307435',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Orange Passion','A classic piece with beautiful orange polyps that contrasts its striking blue base','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000003/Orange_Passion.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('465d9e65-4781-47ee-9e6a-9f7dcc51d1cc',300,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000008/thumbnails/adidas1.png','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000008/adidas1.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000008/adidas1.mp4',NULL,'2021-04-12 05:05:38.308668',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','adidas1','adidas1','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000008/adidas1.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('712f482a-ef7f-4341-b9a9-9604888ca195',100,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000001/thumbnails/wd.jpg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000001/wd.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000001/wd.mp4','17ac8f61-4659-4b64-8e43-cacc1956ceaf','2021-04-12 05:05:38.289521',NULL,NULL,'1d4f0a64-fbc1-4f44-82ac-d52de65a75a2','Walt Disney','Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000001/wd.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('872e4ae0-1ed3-456a-8e89-441b1a136f7b',780,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000006/thumbnails/nike2015.png','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000006/Nike Running Shoes Commercial 2015.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000006/Nike Running Shoes Commercial 2015.mp4',NULL,'2021-04-12 05:05:38.308470',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','Nike Running Shoes Commercial 2015','Nike Running Shoes Commercial 2015','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000006/Nike Running Shoes Commercial 2015.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('bd577b3f-b799-43d5-9df9-be094bdd4d2a',77,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000009/thumbnails/adidas2.png','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000009/adidas2.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000009/adidas2.mp4',NULL,'2021-04-12 05:05:38.308759',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','adidas2','adidas2','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000009/adidas2.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2'),('df34c63c-4193-40e7-9ccb-cb14b489f700',111,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000007/thumbnails/audi.jpeg','https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000007/The all-new Audi RS 5.mp4',NULL,'https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000007/The all-new Audi RS 5.mp4',NULL,'2021-04-12 05:05:38.308572',NULL,NULL,'8726c4d9-ff92-479a-a73e-1f7faae85b78','The all-new Audi RS 5','The all-new Audi RS 5','/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000007/The all-new Audi RS 5.mp4',NULL,'3c113152-311d-4e0a-a1e3-b247155b29e2');
/*!40000 ALTER TABLE `Videos` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-04-12 13:08:50
