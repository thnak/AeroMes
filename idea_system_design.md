# **TÀI LIỆU THIẾT KẾ HỆ THỐNG MES (SYSTEM DESIGN DOCUMENT)**

Tài liệu này mô tả thiết kế hệ thống tổng quan cho Hệ thống Quản trị Sản xuất (MES). Hệ thống sử dụng cơ sở dữ liệu quan hệ **SQL Server**, giao tiếp thông qua **Web API (RESTful)**. Toàn bộ dữ liệu sản xuất từ nhà xưởng sẽ được các thiết bị đầu cuối (Tablet của tổ trưởng, màn hình PC tại trạm máy, máy quét mã vạch cầm tay) gửi trực tiếp về API để ghi nhận theo thời gian thực thay vì kết nối IoT trực tiếp.

## **1\. Kiến Trúc Hệ Thống Tổng Quan (System Architecture)**

Hệ thống được thiết kế theo mô hình 4 tầng tiêu chuẩn nhằm đảm bảo tính mở rộng, bảo mật và hiệu năng cao khi xử lý lượng lớn giao dịch đồng thời từ xưởng sản xuất.

┌────────────────────────────────────────────────────────────────────────┐  
│                        TẦNG TRÌNH DIỄN (CLIENTS)                       │  
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐  │  
│  │     Web App      │    │  Tablet App (OS) │    │  Handheld PDA/   │  │  
│  │ (Quản lý/Báo cáo)│    │(Trạm máy/Line PC)│    │ Barcode Scanner  │  │  
│  └────────▲─────────┘    └────────▲─────────┘    └────────▲─────────┘  │  
└───────────┼───────────────────────┼───────────────────────┼────────────┘  
            │                       │                       │  
            └───────────────┬───────┴───────────────────────┘  
                            │ (HTTPS / JSON)  
                            ▼  
┌────────────────────────────────────────────────────────────────────────┐  
│                     TẦNG API CỔNG VÀO (API GATEWAY)                    │  
│  ┌──────────────────────────────────────────────────────────────────┐  │  
│  │     Load Balancer / API Gateway (Nginx / Ocelot / AWS API GW)     │  │  
│  │     \- Authentication (JWT), Rate Limiting, Request Routing       │  │  
│  └─────────────────────────────────┬────────────────────────────────┘  │  
└────────────────────────────────────┼───────────────────────────────────┘  
                                     ▼  
┌────────────────────────────────────────────────────────────────────────┐  
│                   TẦNG DỊCH VỤ NGHIỆP VỤ (SERVICES)                    │  
│  ┌──────────────────────────────────────────────────────────────────┐  │  
│  │                       Web API App Service                        │  │  
│  │ (Được xây dựng bằng .NET Core / Java Spring Boot / Node.js NestJS)│  │  
│  │                                                                  │  │  
│  │  ┌──────────────────────┐ ┌──────────────────────┐               │  │  
│  │  │  Production Service  │ │  Work Order Service  │               │  │  
│  │  └──────────────────────┘ └──────────────────────┘               │  │  
│  │  ┌──────────────────────┐ ┌──────────────────────┐               │  │  
│  │  │   Quality Service    │ │   Resource Service   │               │  │  
│  │  └──────────────────────┘ └──────────────────────┘               │  │  
│  │  ┌──────────────────────┐ ┌──────────────────────┐               │  │  
│  │  │    Report Service    │ │  Integration Service │               │  │  
│  │  └──────────────────────┘ └──────────────────────┘               │  │  
│  └─────────────────────────────────┬────────────────────────────────┘  │  
└────────────────────────────────────┼───────────────────────────────────┘  
                                     ▼  
┌────────────────────────────────────────────────────────────────────────┐  
│                       TẦNG DỮ LIỆU (DATABASE LAYER)                    │  
│  ┌──────────────────────────────────────────────────────────────────┐  │  
│  │                         SQL Server (Primary)                     │  │  
│  │  \- Lưu trữ dữ liệu nghiệp vụ, cấu hình, lịch sử sản xuất         │  │  
│  │  \- Chạy các Stored Procedure tính toán OEE nhanh                 │  │  
│  └─────────────────────────────────┬────────────────────────────────┘  │  
│                                    │ (Replication)                     │  
│                                    ▼                                   │  
│  ┌──────────────────────────────────────────────────────────────────┐  │  
│  │                     SQL Server (Read Replica)                    │  │  
│  │  \- Phục vụ riêng cho các báo cáo phân tích, Dashboard thời gian thực│  │  
│  └──────────────────────────────────────────────────────────────────┘  │  
└────────────────────────────────────────────────────────────────────────┘

### **Chi tiết các tầng:**

1. **Tầng Client:**  
   * **Web App:** Dành cho quản lý, kế toán xưởng thực hiện lên kế hoạch, quản lý danh mục cấu hình và xem báo cáo động.  
   * **Tablet App (Operator Interface):** Đặt tại các trạm máy để công nhân bấm chọn Lệnh sản xuất (Work Order \- WO), bắt đầu ca, kết thúc ca, báo cáo sản lượng hỏng/đạt và báo cáo trạng thái dừng máy (Downtime).  
   * **Handheld PDA / Barcode / RFID:** Dùng tại các khu vực nhập/xuất kho vật tư, phân bổ vật tư vào chuyền hoặc kiểm tra chất lượng (QA/QC) ở cuối chuyền.  
2. **Tầng API Gateway & Web API:**  
   * **API Gateway:** Đảm bảo tính bảo mật (Validate JWT Token), định tuyến yêu cầu, ngăn chặn tấn công DDoS bằng cơ chế Rate Limit.  
   * **Web API:** Sử dụng kiến trúc Microservices hoặc Monolith gọn gàng tùy thuộc quy mô nhà máy. Sử dụng kết nối không trạng thái (Stateless Web API), hỗ trợ lưu bộ nhớ đệm (Caching) cho các dữ liệu danh mục tĩnh (như danh sách mã lỗi, danh sách máy).  
3. **Tầng Database:**  
   * **SQL Server:** Lựa chọn hoàn hảo cho sự toàn vẹn dữ liệu (ACID). Sử dụng cơ chế phân chia phân vùng dữ liệu (Partitioning) theo Tháng/Năm cho các bảng lưu nhật ký sản xuất lớn (Production logs).

## **2\. Luồng Xử Lý Ghi Nhận Sản Lượng Thực Tế (Data Flow)**

Dưới đây là luồng xử lý tuần tự (Sequence) khi một người vận hành (Operator) gửi báo cáo sản lượng đạt/lỗi từ một trạm máy thông qua thiết bị Tablet (API-driven).

\[Operator (Tablet)\]          \[Web API\]             \[SQL Server\]          \[ERP (Nếu có)\]  
        │                        │                       │                     │  
        │─── 1\. Gửi sản lượng ──\>│                       │                     │  
        │    (Qty, Defect,...)   │                       │                     │  
        │                        │── 2\. Validate dữ liệu │                     │  
        │                        │   (Kiểm tra WO có     │                     │  
        │                        │    đang chạy không)   │                     │  
        │                        │──────────────────────\>│                     │  
        │                        │                       │                     │  
        │                        │\<─ 3\. Trả về hợp lệ ───│                     │  
        │                        │                       │                     │  
        │                        │── 4\. Ghi Transaction ─│                     │  
        │                        │   (Bắt đầu Transaction│                     │  
        │                        │   ghi Log, cộng dồn)  │                     │  
        │                        │──────────────────────\>│                     │  
        │                        │                       │                     │  
        │                        │\<─ 5\. Thành công ──────│                     │  
        │                        │                       │                     │  
        │                        │─── 6\. \[Bất đồng bộ\] ───────────────────────\>│  
        │                        │    Đồng bộ dữ liệu sản lượng lên ERP        │  
        │\<── 7\. Trả kết quả ─────│                                             │  
        │    (Thành công/Lỗi)    │                                             │

## **3\. Thiết Kế Cơ Sở Dữ Liệu (Database Schema)**

Dưới đây là mô hình thực thể mối quan hệ cơ bản cho các tính năng nghiệp vụ chính của MES trên **SQL Server**. Thiết kế tuân thủ các quy tắc ràng buộc toàn vẹn dữ liệu, hỗ trợ lưu vết lịch sử (Audit Trail).

   ┌──────────────────┐           ┌──────────────────┐  
   │    Users (User)  │           │   WorkCenters    │  
   └────────┬─────────┘           │   (Trạm/Chuyền)  │  
            │ (1)                 └────────┬─────────┘  
            │                              │ (1)  
            ▼ (N)                          │  
   ┌──────────────────┐                    │  
   │    WorkOrders    │◄───────────────────┘ (N)  
   │ (Lệnh sản xuất)  │  
   └────────┬─────────┘  
            │ (1)  
            ├──────────────────────────────┐  
            ▼ (N)                          ▼ (N)  
   ┌──────────────────┐           ┌──────────────────┐  
   │ ProductionLogs   │           │   DowntimeLogs   │  
   │ (Nhật ký sx/API) │           │(Nhật ký dừng máy)│  
   └────────┬─────────┘           └──────────────────┘  
            │ (1)  
            │  
            ▼ (N)  
   ┌──────────────────┐           ┌──────────────────┐  
   │  DefectDetails   │──────────►│   DefectCodes    │  
   │ (Chi tiết lỗi)   │ (N)   (1) │    (Mã lỗi)      │  
   └──────────────────┘           └──────────────────┘

### **Các Script tạo bảng (DDL) chuẩn hóa trên SQL Server:**

\-- 1\. BẢNG DANH MỤC TRẠM MÁY / TỔ SẢN XUẤT (WORK CENTERS)  
CREATE TABLE WorkCenters (  
    WorkCenterID INT IDENTITY(1,1) PRIMARY KEY,  
    WorkCenterCode VARCHAR(50) NOT NULL UNIQUE,  
    WorkCenterName NVARCHAR(100) NOT NULL,  
    Description NVARCHAR(255),  
    IsActive BIT DEFAULT 1,  
    CreatedBy VARCHAR(50),  
    CreatedAt DATETIME DEFAULT GETDATE(),  
    UpdatedBy VARCHAR(50),  
    UpdatedAt DATETIME  
);

\-- 2\. BẢNG LỆNH SẢN XUẤT (WORK ORDERS)  
\-- Nhận kế hoạch từ ERP hoặc phòng kế hoạch để chuyển xuống xưởng sản xuất  
CREATE TABLE WorkOrders (  
    WorkOrderID INT IDENTITY(1,1) PRIMARY KEY,  
    WorkOrderNo VARCHAR(50) NOT NULL UNIQUE,          \-- Mã lệnh sản xuất  
    ProductCode VARCHAR(50) NOT NULL,                 \-- Mã bán thành phẩm/thành phẩm  
    ProductName NVARCHAR(200) NOT NULL,  
    TargetQuantity INT NOT NULL CHECK (TargetQuantity \> 0), \-- Số lượng yêu cầu  
    ActualQtyOK INT DEFAULT 0 CHECK (ActualQtyOK \>= 0),     \-- Sản lượng đạt thực tế  
    ActualQtyNG INT DEFAULT 0 CHECK (ActualQtyNG \>= 0),     \-- Sản lượng lỗi thực tế  
    WorkCenterID INT FOREIGN KEY REFERENCES WorkCenters(WorkCenterID),  
    Status VARCHAR(20) DEFAULT 'RELEASED',             \-- RELEASED, RUNNING, PAUSED, COMPLETED, CANCELLED  
    PlannedStartDate DATETIME,  
    PlannedEndDate DATETIME,  
    ActualStartDate DATETIME,  
    ActualEndDate DATETIME,  
    CreatedBy VARCHAR(50),  
    CreatedAt DATETIME DEFAULT GETDATE(),  
    UpdatedBy VARCHAR(50),  
    UpdatedAt DATETIME  
);

\-- 3\. BẢNG NHẬT KÝ SẢN XUẤT GHI NHẬN TỪ API (PRODUCTION LOGS)  
\-- Ghi nhận mỗi lần gọi API báo cáo sản phẩm hoàn thành từ các trạm  
CREATE TABLE ProductionLogs (  
    LogID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    WorkOrderID INT NOT NULL FOREIGN KEY REFERENCES WorkOrders(WorkOrderID),  
    Timestamp DATETIME DEFAULT GETDATE() NOT NULL,    \-- Thời điểm ghi nhận sản xuất  
    QtyOK INT NOT NULL CHECK (QtyOK \>= 0),            \-- Số lượng đạt trong lượt này  
    QtyNG INT NOT NULL CHECK (QtyNG \>= 0),            \-- Số lượng lỗi trong lượt này  
    OperatorID VARCHAR(50) NOT NULL,                  \-- Mã nhân viên thao tác máy  
    MachineCode VARCHAR(50),                          \-- Mã thiết bị/máy cụ thể  
    ShiftCode VARCHAR(20),                            \-- Mã ca (Ca 1, Ca 2, Ca 3\)  
    Notes NVARCHAR(255),  
      
    INDEX IX\_ProductionLogs\_WorkOrder (WorkOrderID),  
    INDEX IX\_ProductionLogs\_Timestamp (Timestamp)  
);

\-- 4\. BẢNG DANH MỤC MÃ LỖI (DEFECT CODES)  
CREATE TABLE DefectCodes (  
    DefectCodeID INT IDENTITY(1,1) PRIMARY KEY,  
    DefectCode VARCHAR(20) NOT NULL UNIQUE,           \-- VD: DF001, DF002  
    DefectName NVARCHAR(150) NOT NULL,                \-- VD: Trầy xước, Thiếu kích thước  
    DefectCategory NVARCHAR(100),                     \-- Nhóm lỗi (Bao bì, Cơ khí, Linh kiện)  
    IsActive BIT DEFAULT 1  
);

\-- 5\. BẢNG CHI TIẾT SẢN LƯỢNG LỖI (DEFECT DETAILS)  
\-- Lưu chi tiết lỗi tương ứng với các bản ghi ProductionLogs phát sinh QtyNG \> 0  
CREATE TABLE DefectDetails (  
    DefectDetailID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    LogID BIGINT NOT NULL FOREIGN KEY REFERENCES ProductionLogs(LogID),  
    DefectCodeID INT NOT NULL FOREIGN KEY REFERENCES DefectCodes(DefectCodeID),  
    Quantity INT NOT NULL CHECK (Quantity \> 0),       \-- Số lượng lỗi cụ thể của loại mã này  
      
    INDEX IX\_DefectDetails\_Log (LogID)  
);

\-- 6\. BẢNG NHẬT KÝ TRẠNG THÁI DỪNG MÁY (DOWNTIME LOGS)  
\-- Ghi nhận thời gian dừng máy thông qua API báo trạng thái máy  
CREATE TABLE DowntimeLogs (  
    DowntimeLogID BIGINT IDENTITY(1,1) PRIMARY KEY,  
    WorkCenterID INT NOT NULL FOREIGN KEY REFERENCES WorkCenters(WorkCenterID),  
    MachineCode VARCHAR(50) NOT NULL,  
    ReasonCode VARCHAR(50) NOT NULL,                  \-- Mã nguyên nhân (Hỏng điện, Chờ phôi, Vệ sinh)  
    ReasonName NVARCHAR(150),  
    StartTime DATETIME NOT NULL,  
    EndTime DATETIME,                                 \-- NULL nếu máy vẫn đang dừng  
    DurationMinutes AS (DATEDIFF(MINUTE, StartTime, EndTime)), \-- Trường tự tính  
    OperatorID VARCHAR(50),  
    Notes NVARCHAR(255),  
      
    INDEX IX\_DowntimeLogs\_WorkCenter (WorkCenterID),  
    INDEX IX\_DowntimeLogs\_StartTime (StartTime)  
);

## **4\. Thiết Kế Các Web API Chính (API Spec)**

Dưới đây là các API chính được xây dựng để các trạm làm việc (Workstation) hoặc thiết bị cầm tay gọi về hệ thống máy chủ để cập nhật dữ liệu.

### **4.1. API Bắt đầu / Kích hoạt Lệnh sản xuất (Work Order)**

* Công nhân tại trạm máy nhấn "Bắt đầu" để chuyển trạng thái lệnh sản xuất từ RELEASED sang RUNNING.  
* **Endpoint:** POST /api/v1/work-orders/{id}/start  
* **Request Body:**

{  
  "operatorId": "OP-1042",  
  "machineCode": "MCH-CNC-04",  
  "timestamp": "2026-06-08T13:30:00Z"  
}

* **Response (200 OK):**

{  
  "success": true,  
  "message": "Work Order WO-2026-0089 started successfully.",  
  "data": {  
    "workOrderId": 12,  
    "status": "RUNNING",  
    "actualStartDate": "2026-06-08T13:30:00Z"  
  }  
}

### **4.2. API Ghi nhận sản lượng đạt và lỗi (Production Log & Defect)**

* Gọi định kỳ (sau mỗi 5-10 phút hoặc sau mỗi mẻ sản phẩm) để khai báo sản phẩm đạt và sản phẩm lỗi kèm chi tiết lỗi cụ thể.  
* **Endpoint:** POST /api/v1/production/submit-output  
* **Request Body:**

{  
  "workOrderId": 12,  
  "operatorId": "OP-1042",  
  "machineCode": "MCH-CNC-04",  
  "shiftCode": "SHIFT\_A",  
  "qtyOk": 45,  
  "qtyNg": 5,  
  "timestamp": "2026-06-08T14:00:00Z",  
  "defects": \[  
    {  
      "defectCode": "DF001",  
      "qty": 3  
    },  
    {  
      "defectCode": "DF003",  
      "qty": 2  
    }  
  \]  
}

* **Response (201 Created):**

{  
  "success": true,  
  "message": "Production output recorded successfully.",  
  "data": {  
    "logId": 998242,  
    "currentWorkOrderActualOk": 185,  
    "currentWorkOrderActualNg": 12  
  }  
}

### **4.3. API Khai báo bắt đầu dừng máy (Start Downtime)**

* Gọi khi trạm máy gặp sự cố (thiếu nguyên liệu, lỗi cơ khí, bảo dưỡng) làm máy ngừng chạy.  
* **Endpoint:** POST /api/v1/downtime/start  
* **Request Body:**

{  
  "workCenterId": 3,  
  "machineCode": "MCH-CNC-04",  
  "reasonCode": "WAIT\_MATERIAL",  
  "reasonName": "Chờ cấp nguyên liệu chính",  
  "startTime": "2026-06-08T14:15:00Z",  
  "operatorId": "OP-1042"  
}

* **Response (201 Created):**

{  
  "success": true,  
  "downtimeLogId": 451,  
  "status": "DOWNTIME\_ACTIVE"  
}

### **4.4. API Khai báo kết thúc dừng máy (End Downtime)**

* Gọi khi khắc phục xong sự cố để máy quay trở lại trạng thái sản xuất bình thường.  
* **Endpoint:** POST /api/v1/downtime/{downtimeLogId}/end  
* **Request Body:**

{  
  "endTime": "2026-06-08T14:35:00Z",  
  "operatorId": "OP-1042"  
}

* **Response (200 OK):**

{  
  "success": true,  
  "downtimeLogId": 451,  
  "durationMinutes": 20,  
  "status": "RESOLVED"  
}

## **5\. Các Giải Pháp Đảm Bảo Tính Toàn Vẹn & Hiệu Năng Cho MES (API-driven)**

Do không dùng IoT tự động mà dùng API để gửi dữ liệu sản lượng, hệ thống cần có các cơ chế giải quyết rủi ro thực tế sau:

### **5.1. Cơ chế Lưu dữ liệu Ngoại tuyến tại Client (Offline Buffer)**

* **Thách thức:** Mạng không dây (Wi-Fi) dưới nhà xưởng thường xuyên không ổn định. Nếu mất kết nối mạng đúng lúc Client gửi API sản lượng, dữ liệu có thể bị mất hoặc công việc bị gián đoạn.  
* **Giải pháp:** Thiết kế ứng dụng Client (Tablet/HMI) có cơ chế **Offline Storage** (Sử dụng IndexedDB trên Trình duyệt hoặc SQLite Local trên App Android/Windows).  
  * Khi thiết bị mất mạng, dữ liệu sản lượng sẽ được lưu tạm tại Local.  
  * Thiết lập một tiến trình ngầm (Background Sync) liên tục kiểm tra trạng thái mạng. Khi có kết nối mạng trở lại, Client sẽ tự động đẩy tuần tự các API sản lượng chưa đồng bộ lên Server.

### **5.2. Chống ghi trùng lặp dữ liệu (Idempotency API Key)**

* **Thách thức:** Do mạng chập chờn, nút "Gửi sản lượng" trên Client có thể bị bấm nhiều lần hoặc cơ chế tự động đồng bộ gửi lại 2 lần một bản ghi dẫn đến sai lệch số lượng.  
* **Giải pháp:** API ghi nhận sản lượng yêu cầu một Header X-Idempotency-Key (Sử dụng chuỗi UUID sinh ngẫu nhiên duy nhất cho mỗi giao dịch trên Client).  
  * Khi nhận được API, Server kiểm tra xem Key này đã được xử lý thành công trong 24 giờ qua chưa. Nếu rồi thì chỉ trả về kết quả thành công trước đó mà không ghi nhận thêm số lượng vào database lần 2\.

### **5.3. Sử dụng Database Transaction và Concurrency Control**

* **Thách thức:** Nhiều trạm máy cùng cập nhật sản lượng thực tế cho một Lệnh sản xuất (Work Order) chạy đồng thời dẫn đến hiện tượng tranh chấp bản ghi (Race Condition).  
* **Giải pháp:**  
  * Sử dụng Stored Procedure trên SQL Server hoặc cấu trúc ORM có cơ chế khóa lạc quan (Optimistic Concurrency).  
  * Sử dụng mệnh đề UPDATE WorkOrders SET ActualQtyOK \= ActualQtyOK \+ @QtyOK WHERE WorkOrderID \= @WorkOrderID thay vì thực hiện đọc ra cộng dồn bằng code rồi lưu lại.

## **6\. Tính toán Hiệu suất thiết bị tổng thể (OEE) qua SQL Server**

Từ cấu trúc dữ liệu lưu log ở trên, bạn có thể dễ dàng viết Stored Procedure tính toán nhanh chỉ số OEE (Gồm Availability, Performance, Quality) của một trạm máy trong một ca sản xuất:

CREATE PROCEDURE GetMachineOEE  
    @WorkCenterID INT,  
    @MachineCode VARCHAR(50),  
    @ShiftStartDate DATETIME,  
    @ShiftEndDate DATETIME,  
    @DesignedCycleTimeSeconds FLOAT \-- Thời gian thiết kế tối thiểu để sản xuất 1 sản phẩm  
AS  
BEGIN  
    SET NOCOUNT ON;

    \-- 1\. Tính tổng thời gian ca (Tồng thời gian có thể chạy sản xuất)  
    DECLARE @TotalShiftMinutes FLOAT \= DATEDIFF(MINUTE, @ShiftStartDate, @ShiftEndDate);  
      
    \-- 2\. Tính tổng thời gian dừng máy (Downtime)  
    DECLARE @TotalDowntimeMinutes FLOAT;  
    SELECT @TotalDowntimeMinutes \= ISNULL(SUM(DurationMinutes), 0\)  
    FROM DowntimeLogs  
    WHERE WorkCenterID \= @WorkCenterID   
      AND MachineCode \= @MachineCode  
      AND StartTime \>= @ShiftStartDate   
      AND (EndTime \<= @ShiftEndDate OR EndTime IS NULL);

    \-- 3\. Tính lượng sản phẩm đạt (OK) và lỗi (NG)  
    DECLARE @TotalOK INT \= 0, @TotalNG INT \= 0;  
    SELECT   
        @TotalOK \= ISNULL(SUM(QtyOK), 0),  
        @TotalNG \= ISNULL(SUM(QtyNG), 0\)  
    FROM ProductionLogs  
    WHERE MachineCode \= @MachineCode  
      AND Timestamp BETWEEN @ShiftStartDate AND @ShiftEndDate;

    \-- 4\. Tính toán các thành phần OEE  
    DECLARE @Availability FLOAT \= 0.0;  
    DECLARE @Performance FLOAT \= 0.0;  
    DECLARE @Quality FLOAT \= 0.0;  
    DECLARE @OEE FLOAT \= 0.0;

    \-- A. Availability (Khả dụng): Tỷ lệ thời gian máy chạy thực tế trên thời gian ca  
    IF @TotalShiftMinutes \> 0  
        SET @Availability \= (@TotalShiftMinutes \- @TotalDowntimeMinutes) / @TotalShiftMinutes;

    \-- B. Performance (Hiệu suất hoạt động): Tốc độ sản xuất thực tế so với tốc độ thiết kế  
    DECLARE @ActualRunTimeSeconds FLOAT \= (@TotalShiftMinutes \- @TotalDowntimeMinutes) \* 60;  
    DECLARE @TotalProduced INT \= @TotalOK \+ @TotalNG;

    IF @ActualRunTimeSeconds \> 0  
        SET @Performance \= (@TotalProduced \* @DesignedCycleTimeSeconds) / @ActualRunTimeSeconds;

    \-- Giới hạn tối đa Performance là 100% (1.0) phòng trường hợp cycle time đầu vào bị ước lượng sai lệch  
    IF @Performance \> 1.0 SET @Performance \= 1.0;

    \-- C. Quality (Chất lượng): Tỷ lệ sản phẩm đạt tiêu chuẩn trên tổng sản phẩm làm ra  
    IF @TotalProduced \> 0  
        SET @Quality \= CAST(@TotalOK AS FLOAT) / @TotalProduced;

    \-- D. Chỉ số OEE tổng hợp  
    SET @OEE \= @Availability \* @Performance \* @Quality;

    \-- Trả kết quả dưới dạng bảng  
    SELECT   
        @MachineCode AS MachineCode,  
        @TotalShiftMinutes AS TotalPlannedMinutes,  
        @TotalDowntimeMinutes AS DowntimeMinutes,  
        @TotalProduced AS TotalProducedCount,  
        @TotalOK AS OKCount,  
        @TotalNG AS NGCount,  
        ROUND(@Availability \* 100, 2\) AS AvailabilityPercent,  
        ROUND(@Performance \* 100, 2\) AS PerformancePercent,  
        ROUND(@Quality \* 100, 2\) AS QualityPercent,  
        ROUND(@OEE \* 100, 2\) AS OEEPercent;  
END;  
