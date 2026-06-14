package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class LotTraceDto(
    val lotNumber: String,
    val productCode: String,
    val productName: String? = null,
    val locationCode: String? = null,
    val warehouseCode: String? = null,
    val quantity: Double,
    val unit: String? = null,
    val expiryDate: String? = null
)

@Serializable
data class HoldStatusDto(
    val isHeld: Boolean,
    val holdId: String? = null,
    val holdType: String? = null,
    val reason: String? = null,
    val heldAt: String? = null,
    val heldBy: String? = null
)

@Serializable
data class PlaceHoldRequest(
    val lotNumber: String,
    val reason: String,
    val holdType: String
)

@Serializable
data class ReleaseHoldRequest(
    val reason: String
)
