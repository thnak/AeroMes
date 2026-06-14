package com.iotviet.aeromes

import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextInput
import com.iotviet.aeromes.ui.auth.LoginScreen
import com.iotviet.aeromes.ui.auth.LoginUiState
import com.iotviet.aeromes.ui.theme.AeroMesTheme
import org.junit.Rule
import org.junit.Test

class LoginScreenTest {

    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun loginButton_enabledWhenNotLoading() {
        composeRule.setContent {
            AeroMesTheme {
                LoginScreen(
                    state = LoginUiState(email = "user@test.com", password = "pass"),
                    onEmailChange = {}, onPasswordChange = {}, onLoginClick = {}, onLoginSuccess = {}
                )
            }
        }
        composeRule.onNodeWithText("Log In").assertIsEnabled()
    }

    @Test
    fun loginButton_disabledWhenLoading() {
        composeRule.setContent {
            AeroMesTheme {
                LoginScreen(
                    state = LoginUiState(isLoading = true),
                    onEmailChange = {}, onPasswordChange = {}, onLoginClick = {}, onLoginSuccess = {}
                )
            }
        }
        composeRule.onNodeWithText("Log In").assertIsNotEnabled()
    }

    @Test
    fun errorMessage_shownOnError() {
        composeRule.setContent {
            AeroMesTheme {
                LoginScreen(
                    state = LoginUiState(error = "Invalid credentials"),
                    onEmailChange = {}, onPasswordChange = {}, onLoginClick = {}, onLoginSuccess = {}
                )
            }
        }
        composeRule.onNodeWithText("Invalid credentials").assertExists()
    }
}
