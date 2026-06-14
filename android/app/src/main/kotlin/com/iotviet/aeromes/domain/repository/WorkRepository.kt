package com.iotviet.aeromes.domain.repository

import com.iotviet.aeromes.domain.model.InspectionOrder
import com.iotviet.aeromes.domain.model.InventoryItem
import com.iotviet.aeromes.domain.model.Job
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
