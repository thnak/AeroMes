package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class InspectionOrderDto(
    val id: Int,
    val orderCode: String? = null,
    val productCode: String? = null,
    val productName: String? = null,
    val status: String,
    val assignedTo: String? = null,
    val createdAt: String? = null
)

@Serializable
data class InspectionResultRequest(
    val charId: Int,
    val measuredValue: Double? = null,
    val pass: Boolean,
    val notes: String? = null
)
