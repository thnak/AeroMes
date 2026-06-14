package com.iotviet.aeromes.data.repository

import com.iotviet.aeromes.data.network.AeroMesApi
import com.iotviet.aeromes.data.network.dto.CreateProductionLogRequest
import com.iotviet.aeromes.data.network.dto.FinishJobRequest
import com.iotviet.aeromes.domain.model.InspectionOrder
import com.iotviet.aeromes.domain.model.InventoryItem
import com.iotviet.aeromes.domain.model.Job
import com.iotviet.aeromes.domain.model.WorkOrder
import com.iotviet.aeromes.domain.repository.InventoryRepository
import com.iotviet.aeromes.domain.repository.QualityRepository
import com.iotviet.aeromes.domain.repository.WorkRepository
import javax.inject.Inject

class WorkRepositoryImpl @Inject constructor(private val api: AeroMesApi) : WorkRepository {

    override suspend fun getWorkOrders(status: String?): Result<List<WorkOrder>> = runCatching {
        val response = api.getWorkOrders(status = status)
        response.body()?.map {
            WorkOrder(it.woid, it.woCode, it.workCenterName ?: "-", it.targetQty, it.actualOK, it.actualNG, it.status)
        } ?: error("Empty response (${response.code()})")
    }

    override suspend fun getJobs(operatorId: String?): Result<List<Job>> = runCatching {
        val response = api.getJobs(operatorId = operatorId)
        response.body()?.map {
            Job(it.jobID, it.woid, it.machineCode, it.shiftCode, it.operatorID, it.startTime, it.endTime, it.status)
        } ?: error("Empty response (${response.code()})")
    }

    override suspend fun getJob(id: Long): Result<Job> = runCatching {
        val response = api.getJob(id)
        val it = response.body() ?: error("Empty response (${response.code()})")
        Job(it.jobID, it.woid, it.machineCode, it.shiftCode, it.operatorID, it.startTime, it.endTime, it.status)
    }

    override suspend fun finishJob(id: Long): Result<Unit> = runCatching {
        val response = api.finishJob(id, FinishJobRequest())
        if (!response.isSuccessful) error("Finish job failed (${response.code()})")
    }

    override suspend fun logProduction(jobId: Long, qtyOK: Int, qtyNG: Int, notes: String?): Result<Unit> = runCatching {
        val response = api.createProductionLog(jobId, CreateProductionLogRequest(qtyOK, qtyNG, notes))
        if (!response.isSuccessful) error("Log production failed (${response.code()})")
    }
}

class QualityRepositoryImpl @Inject constructor(private val api: AeroMesApi) : QualityRepository {

    override suspend fun getInspectionOrders(status: String?): Result<List<InspectionOrder>> = runCatching {
        val response = api.getInspectionOrders(status)
        response.body()?.map {
            InspectionOrder(it.id, it.orderCode ?: "-", it.productCode ?: "-", it.productName ?: "-", it.status, it.assignedTo)
        } ?: error("Empty response (${response.code()})")
    }

    override suspend fun getInspectionOrder(id: Int): Result<InspectionOrder> = runCatching {
        val response = api.getInspectionOrder(id)
        val it = response.body() ?: error("Empty response (${response.code()})")
        InspectionOrder(it.id, it.orderCode ?: "-", it.productCode ?: "-", it.productName ?: "-", it.status, it.assignedTo)
    }

    override suspend fun startInspection(id: Int): Result<Unit> = runCatching {
        api.startInspection(id)
    }

    override suspend fun passInspection(id: Int): Result<Unit> = runCatching {
        api.passInspection(id)
    }

    override suspend fun failInspection(id: Int): Result<Unit> = runCatching {
        api.failInspection(id)
    }
}

class InventoryRepositoryImpl @Inject constructor(private val api: AeroMesApi) : InventoryRepository {

    override suspend fun getInventory(productCode: String?, lotNumber: String?): Result<List<InventoryItem>> = runCatching {
        val response = api.getInventory(productCode = productCode, lotNumber = lotNumber)
        response.body()?.map {
            InventoryItem(it.productCode, it.productName ?: "-", it.lotNumber, it.warehouseCode, it.locationCode, it.quantity, it.unit)
        } ?: error("Empty response (${response.code()})")
    }
}
