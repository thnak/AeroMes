package com.iotviet.aeromes.data.repository

import com.iotviet.aeromes.data.local.TokenDataStore
import com.iotviet.aeromes.data.network.AeroMesApi
import com.iotviet.aeromes.data.network.dto.LoginRequest
import com.iotviet.aeromes.domain.model.Session
import com.iotviet.aeromes.domain.repository.AuthRepository
import kotlinx.coroutines.flow.first
import javax.inject.Inject

class AuthRepositoryImpl @Inject constructor(
    private val api: AeroMesApi,
    private val tokenDataStore: TokenDataStore
) : AuthRepository {

    override suspend fun login(email: String, password: String): Result<Session> = runCatching {
        val response = api.login(LoginRequest(email, password))
        val body = response.body() ?: error("Empty response (${response.code()})")
        tokenDataStore.saveSession(body.accessToken, body.email, body.fullName)
        Session(
            accessToken = body.accessToken,
            email = body.email,
            fullName = body.fullName,
            roles = body.roles
        )
    }

    override suspend fun logout() {
        runCatching { api.logout() }
        tokenDataStore.clearSession()
    }

    override suspend fun isLoggedIn(): Boolean =
        tokenDataStore.accessToken.first() != null
}
