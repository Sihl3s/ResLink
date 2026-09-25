package com.dynamicdevelopers.reslink.ui.common

// Shared cards and fields use the ivory, white, and forest-green palette (Android Developers, 2025b).

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dynamicdevelopers.reslink.ui.theme.CardWhite
import com.dynamicdevelopers.reslink.ui.theme.Forest
import com.dynamicdevelopers.reslink.ui.theme.Ink
import com.dynamicdevelopers.reslink.ui.theme.Mint
import com.dynamicdevelopers.reslink.ui.theme.Muted

@Composable
fun ResCard(modifier: Modifier = Modifier, content: @Composable ColumnScope.() -> Unit) {
    Column(
        modifier = modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(18.dp))
            .background(CardWhite)
            .border(1.dp, Color(0xFFE7E2D8), RoundedCornerShape(18.dp))
            .padding(16.dp),
        content = content
    )
}

@Composable
fun ResField(
    value: String,
    onValueChange: (String) -> Unit,
    label: String,
    password: Boolean = false,
    keyboardType: KeyboardType = KeyboardType.Text
) {
    OutlinedTextField(
        value = value,
        onValueChange = onValueChange,
        label = { Text(label) },
        modifier = Modifier.fillMaxWidth(),
        singleLine = true,
        visualTransformation = if (password) PasswordVisualTransformation() else VisualTransformation.None,
        keyboardOptions = KeyboardOptions(keyboardType = keyboardType),
        colors = OutlinedTextFieldDefaults.colors(
            focusedBorderColor = Forest,
            unfocusedBorderColor = Color(0xFFE7E2D8),
            focusedLabelColor = Forest,
            cursorColor = Forest
        )
    )
}

@Composable
fun ResButton(text: String, loading: Boolean = false, enabled: Boolean = true, onClick: () -> Unit) {
    Button(
        onClick = onClick,
        enabled = enabled && !loading,
        modifier = Modifier.fillMaxWidth().height(52.dp),
        colors = ButtonDefaults.buttonColors(containerColor = Forest, contentColor = Color.White),
        shape = RoundedCornerShape(12.dp)
    ) {
        if (loading) CircularProgressIndicator(color = Color.White, strokeWidth = 2.dp) else Text(text, fontWeight = FontWeight.Bold)
    }
}

@Composable
fun ScreenHeader(title: String, subtitle: String, action: String? = null, onAction: (() -> Unit)? = null) {
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
        Column(Modifier.weight(1f)) {
            Text(title, color = Ink, fontSize = 26.sp, fontWeight = FontWeight.Bold)
            Text(subtitle, color = Muted, fontSize = 14.sp)
        }
        if (action != null && onAction != null) {
            TextButton(onClick = onAction) { Text(action, color = Forest, fontWeight = FontWeight.SemiBold) }
        }
    }
    Spacer(Modifier.height(16.dp))
}

@Composable
fun StatusChip(text: String) {
    val color = when (text.lowercase()) {
        "open", "pending" -> Color(0xFFB45309)
        "inprogress", "acknowledged" -> Color(0xFF1D4ED8)
        "resolved", "sold" -> Forest
        else -> Muted
    }
    Box(
        Modifier
            .clip(RoundedCornerShape(999.dp))
            .background(if (text.lowercase() in listOf("resolved", "sold")) Mint else color.copy(alpha = 0.14f))
            .padding(horizontal = 10.dp, vertical = 4.dp)
    ) {
        Text(text, color = color, fontSize = 12.sp, fontWeight = FontWeight.Medium)
    }
}

@Composable
fun ErrorText(message: String?) {
    if (!message.isNullOrBlank()) {
        Text(message, color = Color(0xFF9F2D2D), modifier = Modifier.padding(top = 8.dp))
    }
}
