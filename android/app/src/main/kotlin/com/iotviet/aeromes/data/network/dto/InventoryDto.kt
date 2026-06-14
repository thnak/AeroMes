package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class InventoryItemDto(
    val productCode: String,
    val productName: String? = null,
    val lotNumber: String? = null,
    val warehouseCode: String? = null,
    val locationCode: String? = null,
    val quantity: Double,
    val unit: String? = null,
    val expiryDate: String? = null
)
