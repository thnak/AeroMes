package com.iotviet.aeromes.data.network

import com.iotviet.aeromes.data.network.dto.CreateJobRequest
import com.iotviet.aeromes.data.network.dto.CreateMaintenanceOrderRequest
import com.iotviet.aeromes.data.network.dto.CreateMaterialRequisitionRequest
import com.iotviet.aeromes.data.network.dto.CreateMaterialTransferRequest
import com.iotviet.aeromes.data.network.dto.CreateProductionLogRequest
import com.iotviet.aeromes.data.network.dto.FinishJobRequest
import com.iotviet.aeromes.data.network.dto.HoldStatusDto
import com.iotviet.aeromes.data.network.dto.InspectionOrderDto
import com.iotviet.aeromes.data.network.dto.InspectionResultRequest
import com.iotviet.aeromes.data.network.dto.InventoryItemDto
import com.iotviet.aeromes.data.network.dto.JobDto
import com.iotviet.aeromes.data.network.dto.LoginRequest
import com.iotviet.aeromes.data.network.dto.LoginResponse
import com.iotviet.aeromes.data.network.dto.LotTraceDto
import com.iotviet.aeromes.data.network.dto.MaintenanceOrderDto
import com.iotviet.aeromes.data.network.dto.MaterialRequisitionDto
import com.iotviet.aeromes.data.network.dto.MaterialTransferDto
import com.iotviet.aeromes.data.network.dto.MeResponse
import com.iotviet.aeromes.data.network.dto.PlaceHoldRequest
import com.iotviet.aeromes.data.network.dto.ProductionLogDto
import com.iotviet.aeromes.data.network.dto.ReleaseHoldRequest
import com.iotviet.aeromes.data.network.dto.SopDocumentDetailDto
import com.iotviet.aeromes.data.network.dto.SopDocumentDto
import com.iotviet.aeromes.data.network.dto.StorageLocationDto
import com.iotviet.aeromes.data.network.dto.WorkOrderDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

interface AeroMesApi {

    // Auth
    @POST("api/v1/auth/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>

    @POST("api/v1/auth/logout")
    suspend fun logout(): Response<Unit>

    @GET("api/v1/auth/me")
    suspend fun me(): Response<MeResponse>

    // Work Orders
    @GET("api/v1/work-orders")
    suspend fun getWorkOrders(
        @Query("status") status: String? = null,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 50
    ): Response<List<WorkOrderDto>>

    @GET("api/v1/work-orders/{id}")
    suspend fun getWorkOrder(@Path("id") id: Int): Response<WorkOrderDto>

    @POST("api/v1/work-orders/{id}/start")
    suspend fun startWorkOrder(@Path("id") id: Int): Response<Unit>

    // Jobs
    @GET("api/v1/jobs")
    suspend fun getJobs(
        @Query("woid") woid: Int? = null,
        @Query("operatorId") operatorId: String? = null
    ): Response<List<JobDto>>

    @POST("api/v1/jobs")
    suspend fun createJob(@Body request: CreateJobRequest): Response<JobDto>

    @GET("api/v1/jobs/{id}")
    suspend fun getJob(@Path("id") id: Long): Response<JobDto>

    @POST("api/v1/jobs/{jobId}/finish")
    suspend fun finishJob(
        @Path("jobId") jobId: Long,
        @Body request: FinishJobRequest = FinishJobRequest()
    ): Response<Unit>

    // Production Logs (nested under jobs)
    @GET("api/v1/jobs/{jobId}/logs")
    suspend fun getProductionLogs(@Path("jobId") jobId: Long): Response<List<ProductionLogDto>>

    @POST("api/v1/jobs/{jobId}/logs")
    suspend fun createProductionLog(
        @Path("jobId") jobId: Long,
        @Body request: CreateProductionLogRequest
    ): Response<ProductionLogDto>

    // Quality
    @GET("api/v1/quality/inspection-orders")
    suspend fun getInspectionOrders(
        @Query("status") status: String? = null
    ): Response<List<InspectionOrderDto>>

    @GET("api/v1/quality/inspection-orders/{id}")
    suspend fun getInspectionOrder(@Path("id") id: Int): Response<InspectionOrderDto>

    @PATCH("api/v1/quality/inspection-orders/{id}/start")
    suspend fun startInspection(@Path("id") id: Int): Response<Unit>

    @PATCH("api/v1/quality/inspection-orders/{id}/pass")
    suspend fun passInspection(@Path("id") id: Int): Response<Unit>

    @PATCH("api/v1/quality/inspection-orders/{id}/fail")
    suspend fun failInspection(@Path("id") id: Int): Response<Unit>

    @POST("api/v1/quality/inspection-orders/{id}/results")
    suspend fun submitResult(
        @Path("id") id: Int,
        @Body request: InspectionResultRequest
    ): Response<Unit>

    // Inventory
    @GET("api/v1/inventory")
    suspend fun getInventory(
        @Query("productCode") productCode: String? = null,
        @Query("lotNumber") lotNumber: String? = null,
        @Query("warehouseCode") warehouseCode: String? = null
    ): Response<List<InventoryItemDto>>

    @GET("api/v1/inventory/available")
    suspend fun getAvailableInventory(
        @Query("productCode") productCode: String? = null
    ): Response<List<InventoryItemDto>>

    // Lot Trace (Feature A + F)
    @GET("api/v1/inventory/trace/{lotNumber}")
    suspend fun getLotTrace(@Path("lotNumber") lotNumber: String): Response<LotTraceDto>

    // Lot Holds (Feature A + B)
    @GET("api/v1/trace/holds/lot/{lotNumber}/status")
    suspend fun getLotHoldStatus(@Path("lotNumber") lotNumber: String): Response<HoldStatusDto>

    @POST("api/v1/trace/holds")
    suspend fun placeHold(@Body request: PlaceHoldRequest): Response<Unit>

    @POST("api/v1/trace/holds/{id}/release")
    suspend fun releaseHold(
        @Path("id") id: String,
        @Body request: ReleaseHoldRequest
    ): Response<Unit>

    // Maintenance (Feature C)
    @POST("api/v1/maintenance/orders")
    suspend fun createMaintenanceOrder(@Body request: CreateMaintenanceOrderRequest): Response<MaintenanceOrderDto>

    // SOP Documents (Feature D)
    @GET("api/v1/sop/documents")
    suspend fun getSopDocuments(
        @Query("productCode") productCode: String? = null
    ): Response<List<SopDocumentDto>>

    @GET("api/v1/sop/documents/{id}")
    suspend fun getSopDocument(@Path("id") id: String): Response<SopDocumentDetailDto>

    // Material Requisition (Feature E)
    @POST("api/v1/material-requisitions")
    suspend fun createMaterialRequisition(@Body request: CreateMaterialRequisitionRequest): Response<MaterialRequisitionDto>

    // Stock Transfer (Feature F)
    @GET("api/v1/master/storage-locations")
    suspend fun getStorageLocations(): Response<List<StorageLocationDto>>

    @POST("api/v1/material-transfer-slips")
    suspend fun createMaterialTransfer(@Body request: CreateMaterialTransferRequest): Response<MaterialTransferDto>
}
