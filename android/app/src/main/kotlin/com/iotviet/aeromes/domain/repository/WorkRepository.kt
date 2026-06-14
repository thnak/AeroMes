package com.iotviet.aeromes.domain.repository

import com.iotviet.aeromes.domain.model.HoldStatus
import com.iotviet.aeromes.domain.model.InspectionOrder
import com.iotviet.aeromes.domain.model.InventoryItem
import com.iotviet.aeromes.domain.model.Job
import com.iotviet.aeromes.domain.model.LotTrace
import com.iotviet.aeromes.domain.model.SopDocument
import com.iotviet.aeromes.domain.model.SopDocumentDetail
import com.iotviet.aeromes.domain.model.StorageLocation
import com.iotviet.aeromes.domain.model.WorkOrder

interface WorkRepository {
    suspend fun getWorkOrders(status: String? = null): Result<List<WorkOrder>>
    suspend fun getJobs(operatorId: String? = null): Result<List<Job>>
    suspend fun getJob(id: Long): Result<Job>
    suspend fun finishJob(id: Long): Result<Unit>
    suspend fun logProduction(jobId: Long, qtyOK: Int, qtyNG: Int, notes: String?): Result<Unit>
}

interface QualityRepository {
    suspend fun getInspectionOrders(status: String? = null): Result<List<InspectionOrder>>
    suspend fun getInspectionOrder(id: Int): Result<InspectionOrder>
    suspend fun startInspection(id: Int): Result<Unit>
    suspend fun passInspection(id: Int): Result<Unit>
    suspend fun failInspection(id: Int): Result<Unit>
}

interface InventoryRepository {
    suspend fun getInventory(productCode: String? = null, lotNumber: String? = null): Result<List<InventoryItem>>
}

interface LotRepository {
    suspend fun getLotTrace(lotNumber: String): Result<LotTrace>
    suspend fun getLotHoldStatus(lotNumber: String): Result<HoldStatus>
    suspend fun placeHold(lotNumber: String, reason: String, holdType: String): Result<Unit>
    suspend fun releaseHold(holdId: String, reason: String): Result<Unit>
}

interface MaintenanceRepository {
    suspend fun createMaintenanceOrder(
        machineCode: String,
        description: String,
        priority: String,
        requestedBy: String
    ): Result<Unit>
}

interface SopRepository {
    suspend fun getSopDocuments(productCode: String? = null): Result<List<SopDocument>>
    suspend fun getSopDocument(id: String): Result<SopDocumentDetail>
}

interface MaterialRepository {
    suspend fun issueMaterial(
        woId: Int,
        productCode: String,
        lotNumber: String,
        quantity: Double,
        notes: String?
    ): Result<Unit>
    suspend fun getStorageLocations(): Result<List<StorageLocation>>
    suspend fun transferStock(
        lotNumber: String,
        sourceLocationId: String,
        destinationLocationId: String,
        quantity: Double
    ): Result<Unit>
}
