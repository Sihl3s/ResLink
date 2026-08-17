package com.dynamicdevelopers.reslink.ui.theme

// Light Material 3 scheme uses the same forest-green brand as the browser UI (Android Developers, 2025b).

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.material3.Typography
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp

val Forest = Color(0xFF2D6A4F)
val ForestDark = Color(0xFF1B4332)
val Mint = Color(0xFFE8F5E9)
val Page = Color(0xFFF7F8F6)
val CardWhite = Color(0xFFFFFFFF)
val Ink = Color(0xFF1F2933)
val Muted = Color(0xFF6B7280)
val Danger = Color(0xFFDC2626)

private val Scheme = lightColorScheme(
    primary = Forest,
    onPrimary = Color.White,
    secondary = ForestDark,
    onSecondary = Color.White,
    background = Page,
    onBackground = Ink,
    surface = CardWhite,
    onSurface = Ink,
    error = Danger,
    onError = Color.White
)

private val Type = Typography(
    headlineLarge = TextStyle(fontWeight = FontWeight.Bold, fontSize = 32.sp, color = Ink),
    headlineMedium = TextStyle(fontWeight = FontWeight.SemiBold, fontSize = 24.sp, color = Ink),
    titleLarge = TextStyle(fontWeight = FontWeight.SemiBold, fontSize = 20.sp, color = Ink),
    bodyLarge = TextStyle(fontSize = 16.sp, color = Ink),
    bodyMedium = TextStyle(fontSize = 14.sp, color = Muted),
    labelLarge = TextStyle(fontWeight = FontWeight.Medium, fontSize = 14.sp, color = Ink)
)

@Composable
fun ResLinkTheme(content: @Composable () -> Unit) {
    MaterialTheme(colorScheme = Scheme, typography = Type, content = content)
}
