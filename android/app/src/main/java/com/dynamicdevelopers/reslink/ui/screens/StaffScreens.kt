package com.dynamicdevelopers.reslink.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.FilterChip
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.dynamicdevelopers.reslink.data.EmergencyAlertDto
import com.dynamicdevelopers.reslink.data.NoiseComplaintDto
import com.dynamicdevelopers.reslink.data.ResLinkApi
import com.dynamicdevelopers.reslink.data.VisitorDto
import com.dynamicdevelopers.reslink.data.UpdateActiveRequest
import com.dynamicdevelopers.reslink.data.UpdateStatusRequest
import com.dynamicdevelopers.reslink.data.UserSummaryDto
import com.dynamicdevelopers.reslink.ui.common.ResCard
import com.dynamicdevelopers.reslink.ui.common.ScreenHeader
import com.dynamicdevelopers.reslink.ui.common.StatusChip
import com.dynamicdevelopers.reslink.ui.theme.Muted
import com.dynamicdevelopers.reslink.ui.theme.Forest
import com.dynamicdevelopers.reslink.ui.theme.Ink
import kotlinx.coroutines.launch

@Composable
fun UsersScreen(api: ResLinkApi) {
    var users by remember { mutableStateOf(listOf<UserSummaryDto>()) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { users = runCatching { api.users() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item { ScreenHeader("Residents & staff", "Activate or deactivate accounts") }
        items(users) { user ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(user.fullName, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(if (user.isActive) user.role else "Inactive")
                }
                Text(user.email, color = Muted)
                user.room?.let { Text("Room $it", color = Muted) }
                TextButton(onClick = {
                    scope.launch { api.setActive(user.id, UpdateActiveRequest(!user.isActive)); reload() }
                }) { Text(if (user.isActive) "Deactivate" else "Activate", color = Forest) }
            }
        }
    }
}

@Composable
fun SecurityQueuesScreen(api: ResLinkApi) {
    var alerts by remember { mutableStateOf(listOf<EmergencyAlertDto>()) }
    var complaints by remember { mutableStateOf(listOf<NoiseComplaintDto>()) }
    var visitors by remember { mutableStateOf(listOf<VisitorDto>()) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch {
        alerts = runCatching { api.emergencies() }.getOrDefault(emptyList())
        complaints = runCatching { api.noise() }.getOrDefault(emptyList())
        visitors = runCatching { api.visitors() }.getOrDefault(emptyList())
    }
    LaunchedEffect(Unit) { reload() }

    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item { ScreenHeader("Security desk", "Panic alerts first, then noise complaints") }
        item { Text("Visitors", color = Ink, fontWeight = FontWeight.SemiBold) }
        items(visitors) { visitor ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(visitor.visitorName, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(visitor.status)
                }
                Text("Host ${visitor.hostName} · ${visitor.hostRoom}", color = Muted)
                if (visitor.status == "OnSite") {
                    TextButton(onClick = { scope.launch { api.updateVisitor(visitor.id, UpdateStatusRequest("Departed")); reload() } }) {
                        Text("Check out", color = Forest)
                    }
                }
            }
        }
        item { Text("Panic alerts", color = Ink, fontWeight = FontWeight.SemiBold) }
        items(alerts) { alert ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(alert.reporterName, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(alert.status)
                }
                Text(alert.location, color = Muted)
                Text(alert.message, modifier = Modifier.padding(top = 8.dp))
                Row {
                    listOf("Open", "Acknowledged", "Resolved", "Withdrawn").forEach { status ->
                        FilterChip(
                            selected = alert.status == status,
                            onClick = { scope.launch { api.updateEmergency(alert.id, UpdateStatusRequest(status)); reload() } },
                            label = { Text(if (status == "Withdrawn") "False alarm" else status) },
                            modifier = Modifier.padding(end = 6.dp)
                        )
                    }
                }
            }
        }
        item {
            Spacer(Modifier.height(8.dp))
            Text("Noise complaints", color = Ink, fontWeight = FontWeight.SemiBold)
        }
        items(complaints) { item ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(item.location, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(item.status)
                }
                Text("${item.reporterName} · ${if (item.isAnonymous) "Anonymous" else "Named"}", color = Muted)
                Text(item.description, modifier = Modifier.padding(top = 8.dp))
                Row {
                    listOf("Open", "Resolved", "Withdrawn").forEach { status ->
                        FilterChip(
                            selected = item.status == status,
                            onClick = { scope.launch { api.updateNoise(item.id, UpdateStatusRequest(status)); reload() } },
                            label = { Text(if (status == "Withdrawn") "Withdraw" else status) },
                            modifier = Modifier.padding(end = 6.dp)
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun MoreMenuScreen(role: String, onOpen: (String) -> Unit) {
    val links = when (role) {
        "Student" -> listOf(
            "maintenance" to "Maintenance tickets",
            "panic" to "Emergency panic button",
            "noise" to "Noise complaint",
            "rewards" to "Rewards & points",
            "groups" to "Study groups",
            "notifications" to "Notifications",
            "profile" to "Profile"
        )
        "Admin" -> listOf(
            "feed" to "Moderate community feed",
            "events" to "Manage events",
            "market" to "Marketplace oversight",
            "tickets" to "All maintenance tickets",
            "users" to "Users",
            "notifications" to "Notifications",
            "profile" to "Profile"
        )
        else -> listOf("notifications" to "Notifications", "profile" to "Profile")
    }
    Column(Modifier.fillMaxSize().padding(20.dp)) {
        ScreenHeader("More", "Role tools for $role")
        links.forEach { (route, label) ->
            ResCard(Modifier.padding(bottom = 10.dp).then(Modifier)) {
                Text(label, color = Ink, fontWeight = FontWeight.SemiBold, modifier = Modifier.fillMaxWidth())
                TextButton(onClick = { onOpen(route) }) { Text("Open", color = Forest) }
            }
        }
    }
}
