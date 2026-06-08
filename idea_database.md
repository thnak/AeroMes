# **TÀI LIỆU THIẾT KẾ CƠ SỞ DỮ LIỆU CỐT LÕI HỆ THỐNG MES**

## **(CORE RELATIONAL DATABASE SCHEMA DESIGN)**

Tài liệu này đặc tả chi tiết cấu trúc cơ sở dữ liệu quan hệ trên **SQL Server** cho hệ thống Quản trị Sản xuất (MES). Hệ thống được thiết kế theo kiến trúc **Modular Monolith**, phân tách logic bằng các **Schema** nhằm chuẩn bị sẵn sàng cho việc phân rã sang Microservices trong tương lai nếu cần thiết.

## **1\. Sơ Đồ Thực Thể Quan Hệ Tổng Quát (Entity Relationship Map)**

Sơ đồ dưới đây thể hiện luồng liên kết dữ liệu quan hệ xuyên suốt giữa các Schema từ kế hoạch (ERP), danh mục gốc (Master Data), thực thi (Production) và chất lượng (Quality).

 \[integration.ProductionOrders (PO)\]  
         │ (1)  
         │  
         ▼ (N)  
 \[prod.WorkOrders (WO)\] ◄───────────────────────────┐ (N)  
         │ (1)                                     │  
         │                                         │  
         ▼ (N)                                     │  
 \[prod.Jobs\] ◄──────────────────────┐ (N)          │  
         │ (1)                      │              │  
         │                          │              │  
         ▼ (N)                      │              │ (1)  
 \[prod.ProductionLogs\]              │              │  
         │ (1)                      │              │  
         ├──────────────────────────┼──────────────┼───────────────────────┐  
         │ (N)                      │ (1)          │ (1)                   │ (1)  
         ▼                          │              │                       │  
 \[qual.DefectDetails\]               │              │                       │  
         │ (N)                      │              │                       │  
         │                          │              │                       │  
         ▼ (1)                      │              │                       │  
 \[qual.DefectCodes\]                 │              │                       │  
                                    │              │                       │  
 \[master.StorageLocations\] ◄────────┘              │                       │  
         ▲                                         │                       │  
         │ (1)                                     │                       │  
         │ (N)                                     │                       │  
 \[prod.InventoryStock\]                             │                       │  
                                                   │                       │  
 \[master.RoutingSteps\] ────────────────────────────┴───────────────────────┤  
         │ (N)                                                             │  
         │ (1)                                                             │  
         ▼                                                                 │  
 \[master.Operations\] ◄─────────────────────────────────────────────────────┘

## **2\. Chi Tiết Các Bảng Trong Cơ Sở Dữ Liệu (DDL SQL Server)**

### **2.1. Phân Vùng 1: master Schema (Dữ liệu danh mục gốc)**

Nhóm này lưu trữ các cấu hình tĩnh hoặc ít biến động về nhà xưởng, sản phẩm, định mức nguyên vật liệu và quy trình công nghệ (Routing).

\-- Khởi tạo Schema  
CREATE SCHEMA master;  
GO

\-- 1\. Bảng cấu hình Sơ đồ vật lý nhà xưởng (Physical Topology)  
CREATE TABLE master.WorkCenters (  
    WorkCenterID INT IDENTITY(1,1) PRIMARY KEY,  
    WorkCenterCode VARCHAR(50) NOT NULL UNIQUE,          \-- VD: LINE\_A, LINE\_B, CNC\_SECTION  
    WorkCenterName NVARCHAR(100) NOT NULL,                \-- VD: Dây chuyền lắp ráp A  
    Description NVARCHAR(255),  
    IsActive BIT DEFAULT 1 NOT NULL,  
    CreatedBy VARCHAR(50) NOT NULL,  
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
    UpdatedBy VARCHAR(50),  
    UpdatedAt DATETIME  
);

\-- 2\. Bảng Danh mục thiết bị/máy móc chi tiết thuộc từng WorkCenter  
CREATE TABLE master.Machines (  
    MachineCode VARCHAR(50) PRIMARY KEY,                \-- VD: MCH-CNC-01, MCH-PRESS-02  
    MachineName NVARCHAR(100) NOT NULL,  
    WorkCenterID INT NOT NULL FOREIGN KEY REFERENCES master.WorkCenters(WorkCenterID),  
    Brand NVARCHAR(100),                                \-- Thương hiệu (Zebra, Fanuc...)  
    Model VARCHAR(50),                                  \-- Model máy  
    Status VARCHAR(20) DEFAULT 'OFFLINE' NOT NULL,      \-- RUNNING, DOWN, IDLE, OFFLINE  
    IsActive BIT DEFAULT 1 NOT NULL,  
    CreatedBy VARCHAR(50) NOT NULL,  
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
    UpdatedBy VARCHAR(50),  
    UpdatedAt DATETIME  
);

\-- 3\. Bảng danh mục Sản phẩm/Bán thành phẩm/Nguyên vật liệu (Item/Material Master)  
CREATE TABLE master.Products (  
    ProductCode VARCHAR(50) PRIMARY KEY,                \-- Mã sản phẩm duy nhất (SKU)  
    ProductName NVARCHAR(200) NOT NULL,                 \-- Tên sản phẩm  
    ProductUnit NVARCHAR(20) NOT NULL,                  \-- Đơn vị tính (Cái, Bộ, Mét, Kg)  
    BarcodePattern VARCHAR(100),                        \-- Định dạng Regex mã vạch để PDA tự kiểm tra  
    IsFinishedGood BIT DEFAULT 1 NOT NULL,              \-- 1: Thành phẩm xuất bán, 0: Bán thành phẩm/Nguyên liệu  
    IsActive BIT DEFAULT 1 NOT NULL,  
    CreatedBy VARCHAR(50) NOT NULL,  
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
    UpdatedBy VARCHAR(50),  
    UpdatedAt DATETIME  
);

\-- 4\. Bảng Định mức nguyên vật liệu (BOM \- Bill of Materials)  
CREATE TABLE master.BOM (  
    BOMID INT IDENTITY(1,1) PRIMARY KEY,  
    ParentProductCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Products(ProductCode), \-- Sản phẩm cha  
    ChildProductCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Products(ProductCode),  \-- Nguyên liệu/Bán thành phẩm con  
    RequiredQty NUMERIC(18,4) NOT NULL CHECK (RequiredQty \> 0), \-- Định mức cần dùng để làm ra 1 đơn vị cha  
    ScrapFactor NUMERIC(5,2) DEFAULT 0.00 CHECK (ScrapFactor \>= 0), \-- Tỷ lệ hao hụt cho phép (%)  
    IsActive BIT DEFAULT 1 NOT NULL,  
    CreatedBy VARCHAR(50) NOT NULL,  
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
    CONSTRAINT UQ\_BOM\_Relation UNIQUE (ParentProductCode, ChildProductCode)  
);

\-- 5\. Bảng danh mục các Công đoạn sản xuất thô (Operations Master)  
CREATE TABLE master.Operations (  
    OperationCode VARCHAR(30) PRIMARY KEY,              \-- VD: CUT (Cắt), WELD (Hàn), PAINT (Sơn)  
    OperationName NVARCHAR(100) NOT NULL,                \-- Tên công đoạn  
    Description NVARCHAR(255),  
    IsActive BIT DEFAULT 1 NOT NULL  
);

\-- 6\. Bảng cấu hình Quy trình công nghệ tổng thể (Routing Master)  
CREATE TABLE master.Routings (  
    RoutingID INT IDENTITY(1,1) PRIMARY KEY,  
    RoutingCode VARCHAR(50) NOT NULL UNIQUE,             \-- VD: RT\_CHAIR\_STEEL\_01  
    RoutingName NVARCHAR(150) NOT NULL,  
    ProductCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Products(ProductCode), \-- Quy trình này áp dụng cho sản phẩm nào  
    IsDefault BIT DEFAULT 1 NOT NULL,                     \-- Quy trình chạy mặc định của sản phẩm  
    IsActive BIT DEFAULT 1 NOT NULL,  
    CreatedBy VARCHAR(50) NOT NULL,  
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
    UpdatedBy VARCHAR(50),  
    UpdatedAt DATETIME  
);

\-- 7\. Bảng Chi tiết các bước xử lý tuần tự trong một Quy trình công nghệ (Routing Steps)  
CREATE TABLE master.RoutingSteps (  
    RoutingStepID INT IDENTITY(1,1) PRIMARY KEY,  
    RoutingID INT NOT NULL FOREIGN KEY REFERENCES master.Routings(RoutingID),  
    StepNumber INT NOT NULL CHECK (StepNumber \> 0),      \-- Thứ tự bước (1, 2, 3...)  
    OperationCode VARCHAR(30) NOT NULL FOREIGN KEY REFERENCES master.Operations(OperationCode),  
    DefaultWorkCenterID INT NOT NULL FOREIGN KEY REFERENCES master.WorkCenters(WorkCenterID), \-- Mặc định chạy ở chuyền nào  
    StandardCycleTime FLOAT DEFAULT 0 NOT NULL,         \-- Thời gian tiêu chuẩn để làm 1 sản phẩm (giây)  
    IsQCRequired BIT DEFAULT 0 NOT NULL,                  \-- Bước này có bắt buộc QA/QC kiểm tra không?  
      
    CONSTRAINT UQ\_Routing\_Step UNIQUE (RoutingID, StepNumber)  
);

\-- 8\. Bảng cấu hình Vị trí kho vật lý (Storage Locations)  
CREATE TABLE master.StorageLocations (  
    LocationID INT IDENTITY(1,1) PRIMARY KEY,  
    LocationCode VARCHAR(50) NOT NULL UNIQUE,            \-- VD: KHO\_RM (Kho nguyên liệu), KHO\_WIP\_LINE\_A, KHO\_FG  
    LocationName NVARCHAR(100) NOT NULL,  
    LocationType VARCHAR(20) NOT NULL CHECK (LocationType IN ('RAW\_MATERIAL', 'WIP', 'FINISHED\_GOODS', 'SCRAP')),  
    WorkCenterID INT NULL FOREIGN KEY REFERENCES master.WorkCenters(WorkCenterID), \-- Nếu thuộc loại WIP thì gắn vào Trạm máy nào  
    IsActive BIT DEFAULT 1 NOT NULL  
);  
GO

### **2.2. Phân Vùng 2: integration Schema (Dữ liệu tích hợp ERP)**

Chứa các thông tin đơn hàng, kế hoạch sản xuất do ERP đẩy xuống để MES làm căn cứ triển khai chi tiết.

\-- Khởi tạo Schema  
CREATE SCHEMA integration;  
GO

\-- 1\. Bảng Đơn hàng bán của bộ phận kinh doanh (Sales Orders)  
CREATE TABLE integration.SalesOrders (  
    SOID INT IDENTITY(1,1) PRIMARY KEY,  
    SOCode VARCHAR(50) NOT NULL UNIQUE,                  \-- Mã đơn hàng bán từ ERP  
    CustomerName NVARCHAR(150),  
    OrderDate DATETIME NOT NULL,  
    DeliveryDate DATETIME,  
    Status VARCHAR(20) DEFAULT 'OPEN' NOT NULL,          \-- OPEN, CLOSED, CANCELLED  
    SyncedAt DATETIME DEFAULT GETDATE() NOT NULL  
);

\-- 2\. Bảng Lệnh sản xuất tổng thể (Production Orders)  
CREATE TABLE integration.ProductionOrders (  
    POID INT IDENTITY(1,1) PRIMARY KEY,  
    POCode VARCHAR(50) NOT NULL UNIQUE,                  \-- Mã lệnh sản xuất tổng từ ERP (VD: PO-2026-0001)  
    SOID INT FOREIGN KEY REFERENCES integration.SalesOrders(SOID), \-- Liên kết với đơn SO nào  
    ProductCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Products(ProductCode),  
    TargetQuantity INT NOT NULL CHECK (TargetQuantity \> 0),  
    PlannedStartDate DATETIME,  
    PlannedEndDate DATETIME,  
    ActualStartDate DATETIME,  
    ActualEndDate DATETIME,  
    Status VARCHAR(20) DEFAULT 'RELEASED' NOT NULL,      \-- RELEASED, RUNNING, PAUSED, COMPLETED, CANCELLED  
    SyncedAt DATETIME DEFAULT GETDATE() NOT NULL  
);  
GO

### **2.3. Phân Vùng 3: prod Schema (Trực thi và Vận hành nhà xưởng)**

Đây là trung tâm lưu vết chuyển động thực tế tại xưởng. Tất cả các giao dịch gọi API từ thiết bị PDA/Tablet sẽ được xử lý và ghi nhận tại đây.

\-- Khởi tạo Schema  
CREATE SCHEMA prod;  
GO

\-- 1\. Bảng Lệnh sản xuất công đoạn chi tiết của MES (Work Orders)  
\-- Tự động rã ra từ PO dựa trên quy trình công nghệ (RoutingSteps)  
CREATE TABLE prod.WorkOrders (  
    WOID INT IDENTITY(1,1) PRIMARY KEY,  
    WOCode VARCHAR(50) NOT NULL UNIQUE,                  \-- Mã lệnh công đoạn (VD: WO-2026-0001-CUT)  
    POID INT NOT NULL FOREIGN KEY REFERENCES integration.ProductionOrders(POID),  
    RoutingStepID INT NOT NULL FOREIGN KEY REFERENCES master.RoutingSteps(RoutingStepID),  
    WorkCenterID INT NOT NULL FOREIGN KEY REFERENCES master.WorkCenters(WorkCenterID), \-- Chuyền nhận việc  
    TargetQuantity INT NOT NULL CHECK (TargetQuantity \> 0), \-- Số lượng công đoạn cần hoàn thành  
    ActualQtyOK INT DEFAULT 0 NOT NULL CHECK (ActualQtyOK \>= 0), \-- Sản lượng đạt tích lũy  
    ActualQtyNG INT DEFAULT 0 NOT NULL CHECK (ActualQtyNG \>= 0), \-- Sản lượng lỗi tích lũy  
    Status VARCHAR(20) DEFAULT 'PREPARED' NOT NULL,      \-- PREPARED, RUNNING, PAUSED, COMPLETED, CANCELLED  
    ActualStartDate DATETIME,  
    ActualEndDate DATETIME,  
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
    UpdatedAt DATETIME  
);

\-- 2\. Bảng Phân bổ ca kíp/lượt chạy máy chi tiết (Jobs)  
\-- Được sinh ra khi công nhân đăng nhập trạm máy và bắt đầu nhận lệnh  
CREATE TABLE prod.Jobs (  
    JobID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    WOID INT NOT NULL FOREIGN KEY REFERENCES prod.WorkOrders(WOID),  
    MachineCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Machines(MachineCode),  
    ShiftCode VARCHAR(20) NOT NULL,                       \-- Ca làm việc (VD: SHIFT\_A, SHIFT\_B)  
    OperatorID VARCHAR(50) NOT NULL,                      \-- Mã nhân viên thao tác máy  
    StartTime DATETIME DEFAULT GETDATE() NOT NULL,  
    EndTime DATETIME,                                     \-- NULL nếu chưa đóng ca/kết thúc việc  
    Status VARCHAR(20) DEFAULT 'ACTIVE' NOT NULL,         \-- ACTIVE, SUSPENDED, FINISHED  
      
    INDEX IX\_Jobs\_WOID (WOID),  
    INDEX IX\_Jobs\_MachineCode (MachineCode)  
);

\-- 3\. Bảng Nhật ký ghi nhận sản lượng thực tế (Production Logs)  
\-- Được gọi liên tục qua API từ các trạm máy báo sản lượng  
CREATE TABLE prod.ProductionLogs (  
    LogID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    JobID BIGINT NOT NULL FOREIGN KEY REFERENCES prod.Jobs(JobID),  
    Timestamp DATETIME DEFAULT GETDATE() NOT NULL,        \-- Thời điểm quét ghi nhận sản phẩm  
    QtyOK INT NOT NULL CHECK (QtyOK \>= 0),                \-- Số lượng đạt trong lượt này  
    QtyNG INT NOT NULL CHECK (QtyNG \>= 0),                \-- Số lượng lỗi trong lượt này  
    DeviceIP VARCHAR(30),                                 \-- IP của thiết bị PDA gửi API (để audit trace)  
    Notes NVARCHAR(255),  
      
    INDEX IX\_ProductionLogs\_Job (JobID),  
    INDEX IX\_ProductionLogs\_Timestamp (Timestamp)  
);

\-- 4\. Bảng Quản lý Nhật ký dừng máy thực tế (Downtime Logs)  
CREATE TABLE prod.DowntimeLogs (  
    DowntimeLogID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    MachineCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Machines(MachineCode),  
    ReasonCode VARCHAR(50) NOT NULL,                      \-- Mã lỗi dừng máy (VD: POWER\_LOSS, NO\_MATERIAL)  
    ReasonName NVARCHAR(150),  
    StartTime DATETIME NOT NULL,  
    EndTime DATETIME,                                     \-- NULL nếu máy vẫn đang kẹt chưa sửa xong  
    DurationMinutes AS (DATEDIFF(MINUTE, StartTime, EndTime)), \-- Cột tự tính toán  
    OperatorID VARCHAR(50) NOT NULL,                      \-- Người khai báo dừng máy  
    Notes NVARCHAR(255),  
      
    INDEX IX\_DowntimeLogs\_Machine (MachineCode),  
    INDEX IX\_DowntimeLogs\_StartTime (StartTime)  
);

\-- 5\. Bảng Quản lý lượng tồn kho vật tư thực tế tại các kho và trạm WIP  
CREATE TABLE prod.InventoryStock (  
    StockID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    LocationID INT NOT NULL FOREIGN KEY REFERENCES master.StorageLocations(LocationID),  
    ProductCode VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES master.Products(ProductCode),  
    LotNumber VARCHAR(50) NOT NULL,                        \-- Bắt buộc lưu theo Lot/Batch để truy vết nguồn gốc  
    Quantity NUMERIC(18,4) NOT NULL DEFAULT 0 CHECK (Quantity \>= 0), \-- Số lượng tồn thực tế  
    UpdatedAt DATETIME DEFAULT GETDATE() NOT NULL,  
      
    CONSTRAINT UQ\_Inventory\_Location\_Product\_Lot UNIQUE (LocationID, ProductCode, LotNumber),  
    INDEX IX\_InventoryStock\_Product (ProductCode, LotNumber)  
);  
GO

### **2.4. Phân Vùng 4: qual Schema (Quản lý chất lượng sản phẩm)**

Nơi tiếp nhận thông tin lỗi chi tiết để xử lý khắc phục hoặc đưa ra các phân tích về chất lượng (ví dụ: Biểu đồ Pareto nguyên nhân lỗi).

\-- Khởi tạo Schema  
CREATE SCHEMA qual;  
GO

\-- 1\. Bảng Danh mục mã lỗi quy chuẩn (Defect Codes)  
CREATE TABLE qual.DefectCodes (  
    DefectCodeID INT IDENTITY(1,1) PRIMARY KEY,  
    DefectCode VARCHAR(20) NOT NULL UNIQUE,               \-- VD: ERR\_SCRATCH (Xước bề mặt), ERR\_DEFORM (Biến dạng)  
    DefectName NVARCHAR(150) NOT NULL,                    \-- Tên lỗi tiếng Việt  
    DefectCategory NVARCHAR(100) NOT NULL,                \-- Nhóm lỗi (Lỗi bao bì, Lỗi gia công khuôn...)  
    IsActive BIT DEFAULT 1 NOT NULL  
);

\-- 2\. Bảng Chi tiết danh sách sản lượng lỗi phát sinh theo từng lượt Production Log  
\-- Bảng này kết nối chặt chẽ với prod.ProductionLogs để biết QtyNG của log đó bao gồm những lỗi gì  
CREATE TABLE qual.DefectDetails (  
    DefectDetailID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    LogID BIGINT NOT NULL FOREIGN KEY REFERENCES prod.ProductionLogs(LogID), \-- Log sản xuất phát sinh lỗi  
    DefectCodeID INT NOT NULL FOREIGN KEY REFERENCES qual.DefectCodes(DefectCodeID), \-- Thuộc mã lỗi nào  
    Quantity INT NOT NULL CHECK (Quantity \> 0),           \-- Số lượng sản phẩm bị dính loại lỗi này  
      
    INDEX IX\_DefectDetails\_Log (LogID)  
);  
GO

## **3\. Các Chỉ Mục Hiệu Năng Tối Ưu Cho Truy Vấn MES (Performance Indexes)**

Bản chất của dữ liệu xưởng sản xuất là **ghi nhận liên tục theo từng giây**. Để SQL Server không bị khóa hoặc tắc nghẽn (Lock/Timeout) khi hàng trăm thiết bị gửi API cùng lúc, các chỉ mục (Indexes) dưới đây đã được tối ưu sẵn:

\-- 1\. Index hỗ trợ việc kiểm tra tồn kho nguyên vật liệu nhanh chóng tại trạm WIP khi chuẩn bị sản xuất  
CREATE NONCLUSTERED INDEX IX\_InventoryStock\_WIP\_Check   
ON prod.InventoryStock (LocationID, ProductCode)   
INCLUDE (Quantity);

\-- 2\. Index hỗ trợ việc tính toán chỉ số OEE tức thời cho thiết bị  
\-- Giúp tính nhanh thời gian dừng máy (Downtime) trong khoảng ca  
CREATE NONCLUSTERED INDEX IX\_Downtime\_OEE\_Calc   
ON prod.DowntimeLogs (MachineCode, StartTime, EndTime);

\-- 3\. Index hỗ trợ việc truy xuất ngược nguồn gốc (Traceability)  
\-- Giúp tìm nhanh các log sản xuất phát sinh lỗi của một lệnh WO  
CREATE NONCLUSTERED INDEX IX\_ProductionLogs\_Query\_Trace  
ON prod.ProductionLogs (JobID)  
INCLUDE (Timestamp, QtyOK, QtyNG);

## **4\. Các Ràng Buộc Kiểm Soát Luồng Dữ Liệu Thực Tế (Data Integrity Rules)**

Để tránh trường hợp dữ liệu bị sai lệch hoặc không nhất quán khi người dùng cố tình cập nhật dữ liệu thủ công hoặc API của client (PDA/Tablet) gửi sai tham số, cơ sở dữ liệu này được thiết kế dựa trên các nguyên tắc ràng buộc chặt chẽ sau:

1. **Khóa liên thông 3 tầng (integration.ProductionOrders ![][image1] prod.WorkOrders ![][image1] prod.Jobs):** Một Job muốn khởi chạy bắt buộc phải chỉ định một WOID đang chạy hợp lệ. WOID đó bắt buộc phải nằm trong một POID đang ở trạng thái kích hoạt (RELEASED hoặc RUNNING). Ràng buộc này ngăn chặn việc chạy máy cho các đơn hàng đã bị hủy bỏ trên ERP.  
2. **Quy tắc ghi nhận tồn kho Lô (LotNumber):**  
   Trong bảng prod.InventoryStock, ràng buộc khóa duy nhất UQ\_Inventory\_Location\_Product\_Lot ép buộc hệ thống không được gom chung các mẻ hàng khác nhau lại một chỗ. Khi nhập kho, bắt buộc phải chia nhỏ theo số Lô (LotNumber). Điều này rất quan trọng để khi có một chiếc xe hơi hay một chiếc điện thoại bị lỗi pin, nhà máy có thể khoanh vùng chính xác 100 chiếc điện thoại khác dùng chung lô pin đó để thu hồi.  
3. **Mối ràng buộc giữa Sản lượng hỏng (QtyNG) và Chi tiết lỗi (DefectDetails):**  
   Mặc dù ở tầng Database là mối quan hệ lỏng (để đảm bảo hiệu năng ghi logs tối đa), ở tầng xử lý API (Kotlin Backend), bạn cần thiết lập một Validation Rule: **Nếu một API gửi lượng sản phẩm lỗi ![][image2], hệ thống bắt buộc phải kiểm tra mảng danh sách lỗi gửi kèm**. Tổng số lượng lỗi khai báo trong bảng qual.DefectDetails bắt buộc phải bằng đúng trị số QtyNG ghi nhận trong prod.ProductionLogs.

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABUAAAAZCAYAAADe1WXtAAAAeUlEQVR4XmNgGAWjYOCBvLx8K7oYxUBBQcFDSUmJH12cYgB07UVFRUV5dHGKANDQWUC8B10cDoCS06CKSMJycnILgPQvIO5DN5M2hpIDxMXFuYGGLZaWlpZBlyMbAA28QtWIAiUnoKFB6OIUAXkaJX4FdLFRMApoCADLri0q8MCj7gAAAABJRU5ErkJggg==>

[image2]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGkAAAAaCAYAAAC0NHJVAAAFZUlEQVR4Xu1ZTWhcVRR+Q6IoFf9qGpqfuTNJFEwFkVTFn5W2kCDapYKu6kILglBFQSoUSjcuXJRSoRR/VoJmoUiwSJDSLirNQhcJilBoRS20lFKxhaQ49ft6z5mcnPfe5E1nIhHeB4d57zvn3nffueece9+dJClRokSJEiXWPyohhLeq1epX+J2FvO8Nuo3BwcGNeM5mFa/v6+u7gzaezwLGvQ19fAA5ATkAqof88PDw1lqttsuZ/78wNDR0L17qOl7ka8uPjY3dSZ4TZnnFwMDAfSMjIw94vh2Mjo5ukgnicziGl6yek4Tx3S/6Gdon4nwC3E7IEvWYpHFQFaP7HvID5Ap0E8r/l0CADeH5lyF7IJ9DGhMTE7d4u5aAA8bQcB7O2ZvVmDo6IDEvb3QHIEc8fxPohROPoq9F/mJMt3sD6M6AH7Qcx8uxcRKQLS9YHSHv9idkDv3e4/VrDQTwXXj2bL1ef0yoCsbxDoXX1jYX0gmj97jXKaCfEpvXLM/IpHO6EaHo42lOALNInH7Q6sHfBm635fiiMq4XLe8Bu3dh97LnLWCzBTa/Q+pe1wlCzGIGuEUvOY7L8Zng+rMP8g8abPdKhU5GcBnDF5eHdRyhDABOBCKun33SYVbPksGJtBxsFmmblf0WdEaRQJK18Srk26RolK8C8ZufJPJ8xxnPpwCjgzT2a4BHNS7GDdh9xntcn5OHrBC1x/VPkHm0+xVOD4bn8xp6r+C6hr5P6X2IEX2dWW649/DTq/ewn6QNnnFUuTzA7kHPrQb0+ybaXcZzdiRm/WsX4psrOfxFz6egzvB13oNRLp1yt2QXekYyI6W5K6NjpfwwS1eUSNyfgVzTewUzBPyC3ofl4JnkPTMsrIw6lgsuwKkS3E3IeneWslq25oFjpI+K8imIYSoVPWCzILY2Im9MQjAlUJz5Da/x+wTkEhbzh1Qv9qkUBzdt6zM3DcwQbS+Z/KPqTfldYBYqv5bAs57hGCB7uOP1+jzIO6cmI49PQQyLTFJD7JrlhuuQtLcLcoVbeV6A3w+Z4cTxXiaQ9ixbKwDdKb/e1GQDgd+90te06rT8Qmb7+/s32HaEtD3sZMrb3QQYmCchfxfNLHnn1GTk8SmIYctJkqhl7f/I8iFuGjK3tVK+rgWTeSHuEM9BN2JtRdecTMffGB/kF2TUo8pjnXsY3F8hTlwzcDxC/Dzoyu4zSCZxrcoKjDzI+K/m8Kc9n4IYNieJ6wkG8WwiOxt+RMJ538Fm1i7i1MPuE4rawu5uVcJ+N/u1LxNcZhnw+yhzKwr7SzLGeVvWeB1iCc4MEkIyfY52HZbEHox5B/o5D9nplashxGBKJYK8V6r0p4CHvy7Gj+CltvCrnjwbQ1cT3c++XYgbhbMa3bjeZzMN90fswNDV4yFG9DbljG6S22vPEyGua4u1jM2BfMVzfJknIRyT6Ft+H+WBGRPiKcFzSWe7uzfEF80tPTdq4C7Q58Y0G7J7Yb3micLbyjNDcH8RzvlC1xgL+ZY5LZPEOj1nt9p0PLiGnrfRlgO1k8EsDfGYhI74uL78Rd6EbiBsqbPgi7JfBkFinCDfO3+g7W/tljo5BuO4Tkq/HUFPHDCOp3gvPv+w2s6JA8GyhEbPhzhhrOMsE/tVH2KmZK0lT8KBA55XQL+ZE0pH05le3y2g/1fQ/yEZ/5QrzYUg6xxLWldPHBRSTXjw+2rRw+KWQEfHwnKZ64ETPk1aLNAOzCx+zDZ3UyF+T12wRusQlfHx8Vs9uW4Bh04z8lWKbjcJTg5kiZnJdrW4hT7cTh8lCqC2fMhJ+dLrW0FOI7jxYB/8m2CpnKASJUqUKFGiRIk1xr+XsLqG7bMj5AAAAABJRU5ErkJggg==>
