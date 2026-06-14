package com.iotviet.aeromes.domain.model

data class Session(
    val accessToken: String,
    val email: String,
    val fullName: String,
    val roles: List<String>
)

data class WorkOrder(
    val id: Int,
    val code: String,
    val workCenterName: String,
    val targetQty: Int,
    val actualOK: Int,
    val actualNG: Int,
    val status: String
) {
    val progress: Float get() = if (targetQty > 0) actualOK.toFloat() / targetQty else 0f
}

data class Job(
    val id: Long,
    val woid: Int,
    val machineCode: String,
    val shiftCode: String,
    val operatorId: String,
    val startTime: String,
    val endTime: String?,
    val status: String
)

data class InspectionOrder(
    val id: Int,
    val orderCode: String,
    val productCode: String,
    val productName: String,
    val status: String,
    val assignedTo: String?
)

data class InventoryItem(
    val productCode: String,
    val productName: String,
    val lotNumber: String?,
    val warehouseCode: String?,
    val locationCode: String?,
    val quantity: Double,
    val unit: String?
)
