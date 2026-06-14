package com.iotviet.aeromes.data.repository

import com.iotviet.aeromes.data.network.AeroMesApi
import com.iotviet.aeromes.data.network.dto.CreateMaterialRequisitionRequest
import com.iotviet.aeromes.data.network.dto.CreateMaterialTransferRequest
import com.iotviet.aeromes.domain.model.StorageLocation
import com.iotviet.aeromes.domain.repository.MaterialRepository
import javax.inject.Inject

class MaterialRepositoryImpl @Inject constructor(private val api: AeroMesApi) : MaterialRepository {

    override suspend fun issueMaterial(
        woId: Int,
        productCode: String,
        lotNumber: String,
        quantity: Double,
        notes: String?
    ): Result<Unit> = runCatching {
        val response = api.createMaterialRequisition(
            CreateMaterialRequisitionRequest(woId, productCode, lotNumber, quantity, notes)
        )
        if (!response.isSuccessful) error("Issue material failed (${response.code()})")
    }

    override suspend fun getStorageLocations(): Result<List<StorageLocation>> = runCatching {
        val response = api.getStorageLocations()
        response.body()?.map {
            StorageLocation(it.id, it.code, it.name, it.warehouseCode)
        } ?: error("Empty response (${response.code()})")
    }

    override suspend fun transferStock(
        lotNumber: String,
        sourceLocationId: String,
        destinationLocationId: String,
        quantity: Double
    ): Result<Unit> = runCatching {
        val response = api.createMaterialTransfer(
            CreateMaterialTransferRequest(lotNumber, sourceLocationId, destinationLocationId, quantity)
        )
        if (!response.isSuccessful) error("Stock transfer failed (${response.code()})")
    }
}
