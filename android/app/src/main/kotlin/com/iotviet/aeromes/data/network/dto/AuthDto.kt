package com.iotviet.aeromes.data.network.dto

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

@Serializable
data class LoginRequest(
    val email: String,
    val password: String
)

@Serializable
data class LoginResponse(
    val accessToken: String,
    val tokenType: String,
    val expiresIn: Int,
    val email: String,
    val fullName: String,
    val roles: List<String>,
    val forcePasswordChange: Boolean
)

@Serializable
data class RefreshRequest(
    @SerialName("refreshToken") val refreshToken: String
)

@Serializable
data class MeResponse(
    val id: String,
    val email: String,
    val fullName: String,
    val roles: List<String>
)
