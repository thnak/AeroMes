package com.iotviet.aeromes.data.repository

import com.iotviet.aeromes.data.network.AeroMesApi
import com.iotviet.aeromes.data.network.dto.CreateMaintenanceOrderRequest
import com.iotviet.aeromes.domain.repository.MaintenanceRepository
import javax.inject.Inject

class MaintenanceRepositoryImpl @Inject constructor(private val api: AeroMesApi) : MaintenanceRepository {

    override suspend fun createMaintenanceOrder(
        machineCode: String,
        description: String,
        priority: String,
        requestedBy: String
    ): Result<Unit> = runCatching {
        val response = api.createMaintenanceOrder(
            CreateMaintenanceOrderRequest(machineCode, description, priority, requestedBy)
        )
        if (!response.isSuccessful) error("Create maintenance order failed (${response.code()})")
    }
}
