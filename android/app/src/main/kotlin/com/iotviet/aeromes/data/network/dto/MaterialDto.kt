package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class CreateMaterialRequisitionRequest(
    val woId: Int,
    val productCode: String,
    val lotNumber: String,
    val quantity: Double,
    val notes: String? = null
)

@Serializable
data class MaterialRequisitionDto(
    val id: String? = null,
    val woId: Int,
    val productCode: String,
    val lotNumber: String,
    val quantity: Double,
    val status: String? = null
)

@Serializable
data class StorageLocationDto(
    val id: String,
    val code: String,
    val name: String? = null,
    val warehouseCode: String? = null
)

@Serializable
data class CreateMaterialTransferRequest(
    val lotNumber: String,
    val sourceLocationId: String,
    val destinationLocationId: String,
    val quantity: Double
)

@Serializable
data class MaterialTransferDto(
    val id: String? = null,
    val lotNumber: String,
    val sourceLocationId: String,
    val destinationLocationId: String,
    val quantity: Double,
    val status: String? = null
)
