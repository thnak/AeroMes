package com.iotviet.aeromes.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val Primary = Color(0xFF1565C0)      // deep blue — industrial feel
private val Secondary = Color(0xFF0097A7)    // teal
private val Error = Color(0xFFD32F2F)
private val Surface = Color(0xFFF5F5F5)

private val LightColors = lightColorScheme(
    primary = Primary,
    onPrimary = Color.White,
    secondary = Secondary,
    onSecondary = Color.White,
    error = Error,
    surface = Surface,
    background = Color(0xFFEEEEEE)
)

@Composable
fun AeroMesTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = LightColors,
        content = content
    )
}
