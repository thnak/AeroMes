package com.iotviet.aeromes

import com.iotviet.aeromes.domain.model.Session
import com.iotviet.aeromes.domain.repository.AuthRepository
import com.iotviet.aeromes.ui.auth.LoginViewModel
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class LoginViewModelTest {

    private val testDispatcher = StandardTestDispatcher()

    private val fakeAuth = object : AuthRepository {
        var shouldFail = false
        override suspend fun login(email: String, password: String): Result<Session> {
            return if (shouldFail) Result.failure(Exception("Invalid credentials"))
            else Result.success(Session("token", email, "Test User", listOf("Operator")))
        }
        override suspend fun logout() {}
        override suspend fun isLoggedIn() = false
    }

    private lateinit var vm: LoginViewModel

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
        vm = LoginViewModel(fakeAuth)
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `login success sets isSuccess`() = runTest {
        vm.onEmailChange("operator@aeromes.com")
        vm.onPasswordChange("password")
        vm.login()
        testDispatcher.scheduler.advanceUntilIdle()
        assertTrue(vm.uiState.value.isSuccess)
        assertFalse(vm.uiState.value.isLoading)
    }

    @Test
    fun `login failure sets error`() = runTest {
        fakeAuth.shouldFail = true
        vm.onEmailChange("bad@user.com")
        vm.onPasswordChange("wrong")
        vm.login()
        testDispatcher.scheduler.advanceUntilIdle()
        assertFalse(vm.uiState.value.isSuccess)
        assertNotNull(vm.uiState.value.error)
    }

    @Test
    fun `login with blank email sets error without calling api`() = runTest {
        vm.onPasswordChange("password")
        vm.login()
        assertEquals("Email and password are required.", vm.uiState.value.error)
        assertFalse(vm.uiState.value.isLoading)
    }
}
