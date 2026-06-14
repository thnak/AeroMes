package com.iotviet.aeromes.data.repository

import com.iotviet.aeromes.data.network.AeroMesApi
import com.iotviet.aeromes.domain.model.SopDocument
import com.iotviet.aeromes.domain.model.SopDocumentDetail
import com.iotviet.aeromes.domain.model.SopItem
import com.iotviet.aeromes.domain.repository.SopRepository
import javax.inject.Inject

class SopRepositoryImpl @Inject constructor(private val api: AeroMesApi) : SopRepository {

    override suspend fun getSopDocuments(productCode: String?): Result<List<SopDocument>> = runCatching {
        val response = api.getSopDocuments(productCode = productCode)
        response.body()?.map {
            SopDocument(it.id, it.title, it.productCode, it.version, it.status)
        } ?: error("Empty response (${response.code()})")
    }

    override suspend fun getSopDocument(id: String): Result<SopDocumentDetail> = runCatching {
        val response = api.getSopDocument(id)
        val dto = response.body() ?: error("Empty response (${response.code()})")
        SopDocumentDetail(
            id = dto.id,
            title = dto.title,
            productCode = dto.productCode,
            version = dto.version,
            status = dto.status,
            items = dto.items.map { SopItem(it.sequence, it.itemText, it.category, it.spec) }
        )
    }
}
