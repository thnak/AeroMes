package com.iotviet.aeromes.data.repository

import com.iotviet.aeromes.data.network.AeroMesApi
import com.iotviet.aeromes.data.network.dto.PlaceHoldRequest
import com.iotviet.aeromes.data.network.dto.ReleaseHoldRequest
import com.iotviet.aeromes.domain.model.HoldStatus
import com.iotviet.aeromes.domain.model.LotTrace
import com.iotviet.aeromes.domain.repository.LotRepository
import javax.inject.Inject

class LotRepositoryImpl @Inject constructor(private val api: AeroMesApi) : LotRepository {

    override suspend fun getLotTrace(lotNumber: String): Result<LotTrace> = runCatching {
        val response = api.getLotTrace(lotNumber)
        val dto = response.body() ?: error("Empty response (${response.code()})")
        LotTrace(
            lotNumber = dto.lotNumber,
            productCode = dto.productCode,
            productName = dto.productName ?: "-",
            locationCode = dto.locationCode,
            warehouseCode = dto.warehouseCode,
            quantity = dto.quantity,
            unit = dto.unit,
            expiryDate = dto.expiryDate
        )
    }

    override suspend fun getLotHoldStatus(lotNumber: String): Result<HoldStatus> = runCatching {
        val response = api.getLotHoldStatus(lotNumber)
        val dto = response.body() ?: error("Empty response (${response.code()})")
        HoldStatus(
            isHeld = dto.isHeld,
            holdId = dto.holdId,
            holdType = dto.holdType,
            reason = dto.reason,
            heldAt = dto.heldAt,
            heldBy = dto.heldBy
        )
    }

    override suspend fun placeHold(lotNumber: String, reason: String, holdType: String): Result<Unit> = runCatching {
        val response = api.placeHold(PlaceHoldRequest(lotNumber, reason, holdType))
        if (!response.isSuccessful) error("Place hold failed (${response.code()})")
    }

    override suspend fun releaseHold(holdId: String, reason: String): Result<Unit> = runCatching {
        val response = api.releaseHold(holdId, ReleaseHoldRequest(reason))
        if (!response.isSuccessful) error("Release hold failed (${response.code()})")
    }
}
