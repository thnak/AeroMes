package com.iotviet.aeromes.data.local

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import javax.inject.Inject
import javax.inject.Singleton

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "aeromes_prefs")

@Singleton
class TokenDataStore @Inject constructor(
    @ApplicationContext private val context: Context
) {
    private val accessTokenKey = stringPreferencesKey("access_token")
    private val userEmailKey = stringPreferencesKey("user_email")
    private val userNameKey = stringPreferencesKey("user_name")

    val accessToken: Flow<String?> = context.dataStore.data.map { it[accessTokenKey] }
    val userEmail: Flow<String?> = context.dataStore.data.map { it[userEmailKey] }
    val userName: Flow<String?> = context.dataStore.data.map { it[userNameKey] }

    suspend fun saveSession(token: String, email: String, fullName: String) {
        context.dataStore.edit { prefs ->
            prefs[accessTokenKey] = token
            prefs[userEmailKey] = email
            prefs[userNameKey] = fullName
        }
    }

    suspend fun clearSession() {
        context.dataStore.edit { prefs ->
            prefs.remove(accessTokenKey)
            prefs.remove(userEmailKey)
            prefs.remove(userNameKey)
        }
    }

    suspend fun getAccessTokenOnce(): String? =
        context.dataStore.data.map { it[accessTokenKey] }.first()
}
