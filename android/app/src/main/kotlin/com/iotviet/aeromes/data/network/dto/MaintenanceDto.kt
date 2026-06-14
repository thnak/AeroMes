package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class CreateMaintenanceOrderRequest(
    val machineCode: String,
    val description: String,
    val priority: String,
    val requestedBy: String
)

@Serializable
data class MaintenanceOrderDto(
    val id: String? = null,
    val machineCode: String,
    val description: String,
    val priority: String,
    val status: String? = null,
    val createdAt: String? = null
)
