package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class WorkOrderDto(
    val woid: Int,
    val woCode: String,
    val poid: Int,
    val workCenterID: Int,
    val workCenterName: String? = null,
    val targetQty: Int,
    val actualOK: Int,
    val actualNG: Int,
    val status: String
)

@Serializable
data class JobDto(
    val jobID: Long,
    val woid: Int,
    val machineCode: String,
    val shiftCode: String,
    val operatorID: String,
    val startTime: String,
    val endTime: String? = null,
    val status: String
)

@Serializable
data class CreateJobRequest(
    val woid: Int,
    val machineCode: String,
    val shiftCode: String
)

@Serializable
data class FinishJobRequest(
    val notes: String? = null
)

@Serializable
data class ProductionLogDto(
    val logID: Long,
    val timestamp: String,
    val qtyOK: Int,
    val qtyNG: Int,
    val deviceIP: String? = null,
    val notes: String? = null
)

@Serializable
data class CreateProductionLogRequest(
    val qtyOK: Int,
    val qtyNG: Int,
    val notes: String? = null
)
