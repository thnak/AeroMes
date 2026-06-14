package com.iotviet.aeromes.domain.repository

import com.iotviet.aeromes.domain.model.Session

interface AuthRepository {
    suspend fun login(email: String, password: String): Result<Session>
    suspend fun logout()
    suspend fun isLoggedIn(): Boolean
}
