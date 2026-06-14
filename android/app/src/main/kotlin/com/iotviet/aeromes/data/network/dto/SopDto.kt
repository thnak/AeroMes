package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.Serializable

@Serializable
data class SopDocumentDto(
    val id: String,
    val title: String,
    val productCode: String? = null,
    val version: String? = null,
    val status: String? = null
)

@Serializable
data class SopDocumentDetailDto(
    val id: String,
    val title: String,
    val productCode: String? = null,
    val version: String? = null,
    val status: String? = null,
    val items: List<SopItemDto> = emptyList()
)

@Serializable
data class SopItemDto(
    val sequence: Int,
    val itemText: String,
    val category: String? = null,
    val spec: String? = null
)
