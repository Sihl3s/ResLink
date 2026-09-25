package com.dynamicdevelopers.reslink.ui.auth

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.draw.clip
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.MenuAnchorType
import androidx.compose.material3.OutlinedTextField
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
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dynamicdevelopers.reslink.R
import com.dynamicdevelopers.reslink.data.ResidenceDto
import com.dynamicdevelopers.reslink.data.ResLinkApi
import com.dynamicdevelopers.reslink.data.StaffRegisterRequest
import com.dynamicdevelopers.reslink.data.StudentRegisterRequest
import com.dynamicdevelopers.reslink.data.LoginRequest
import com.dynamicdevelopers.reslink.data.SessionStore
import com.dynamicdevelopers.reslink.ui.common.ErrorText
import com.dynamicdevelopers.reslink.ui.common.ResButton
import com.dynamicdevelopers.reslink.ui.common.ResCard
import com.dynamicdevelopers.reslink.ui.common.ResField
import com.dynamicdevelopers.reslink.ui.theme.Muted
import com.dynamicdevelopers.reslink.ui.theme.Forest
import com.dynamicdevelopers.reslink.ui.theme.Ink
import kotlinx.coroutines.launch
import retrofit2.HttpException

// The same logo file used on the web sign-in screen (Android Developers, 2025a).
@Composable
private fun BrandLockup() {
    Image(
        painter = painterResource(R.drawable.reslink_mark),
        contentDescription = "ResLink",
        modifier = Modifier.size(168.dp).clip(RoundedCornerShape(16.dp))
    )
}

@Composable
fun LoginScreen(
    api: ResLinkApi,
    session: SessionStore,
    onLoggedIn: (String) -> Unit,
    onStudentSignUp: () -> Unit,
    onStaffSignUp: () -> Unit
) {
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var loading by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()

    Column(Modifier.fillMaxSize().padding(24.dp).verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.Center) {
        BrandLockup()
        Text("Sign in and the app opens the dashboard for your role.", color = Muted, modifier = Modifier.padding(top = 12.dp, bottom = 24.dp))
        ResCard {
            ResField(email, { email = it }, "Email")
            Spacer(Modifier.height(12.dp))
            ResField(password, { password = it }, "Password", password = true)
            ErrorText(error)
            Spacer(Modifier.height(16.dp))
            ResButton("Log in", loading) {
                scope.launch {
                    loading = true
                    error = null
                    try {
                        val auth = api.login(LoginRequest(email.trim(), password))
                        session.save(auth)
                        onLoggedIn(auth.role)
                    } catch (ex: Exception) {
                        error = userMessage(ex)
                    } finally {
                        loading = false
                    }
                }
            }
        }
        Spacer(Modifier.height(16.dp))
        TextButton(onClick = onStudentSignUp) { Text("Create a student account", color = Forest) }
        TextButton(onClick = onStaffSignUp) { Text("Staff sign-up (access code required)", color = Ink) }
        Text("Demo: student@reslink.app / Student123!", color = Muted, fontSize = 12.sp, modifier = Modifier.padding(top = 12.dp))
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StudentRegisterScreen(api: ResLinkApi, session: SessionStore, onDone: (String) -> Unit, onBack: () -> Unit) {
    var fullName by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var studentNumber by remember { mutableStateOf("") }
    var room by remember { mutableStateOf("") }
    var residences by remember { mutableStateOf(listOf<ResidenceDto>()) }
    var selected by remember { mutableStateOf<ResidenceDto?>(null) }
    var expanded by remember { mutableStateOf(false) }
    var loading by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        residences = runCatching { api.residences() }.getOrDefault(emptyList())
        selected = residences.firstOrNull()
    }

    Column(Modifier.fillMaxSize().padding(24.dp).verticalScroll(rememberScrollState())) {
        TextButton(onClick = onBack) { Text("Back", color = Forest) }
        BrandLockup()
        Text("Student sign-up", color = Ink, fontSize = 28.sp, fontWeight = FontWeight.Bold)
        Text("Residence, room and student number are required.", color = Muted, modifier = Modifier.padding(bottom = 16.dp))
        ResField(fullName, { fullName = it }, "Full name")
        Spacer(Modifier.height(10.dp))
        ResField(email, { email = it }, "Email")
        Spacer(Modifier.height(10.dp))
        ResField(password, { password = it }, "Password", password = true)
        Spacer(Modifier.height(10.dp))
        ResField(studentNumber, { studentNumber = it }, "Student number")
        Spacer(Modifier.height(10.dp))
        ResField(room, { room = it }, "Room")
        Spacer(Modifier.height(10.dp))
        ExposedDropdownMenuBox(expanded = expanded, onExpandedChange = { expanded = it }) {
            OutlinedTextField(
                value = selected?.name ?: "Select residence",
                onValueChange = {},
                readOnly = true,
                label = { Text("Residence") },
                trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded) },
                modifier = Modifier.menuAnchor(MenuAnchorType.PrimaryNotEditable).fillMaxWidth()
            )
            ExposedDropdownMenu(expanded = expanded, onDismissRequest = { expanded = false }) {
                residences.forEach {
                    DropdownMenuItem(text = { Text(it.name) }, onClick = { selected = it; expanded = false })
                }
            }
        }
        ErrorText(error)
        Spacer(Modifier.height(16.dp))
        ResButton("Create student account", loading) {
            val residence = selected
            if (residence == null) {
                error = "Choose a residence."
                return@ResButton
            }
            scope.launch {
                loading = true
                error = null
                try {
                    val auth = api.registerStudent(
                        StudentRegisterRequest(fullName, email, password, studentNumber, residence.id, room)
                    )
                    session.save(auth)
                    onDone(auth.role)
                } catch (ex: Exception) {
                    error = userMessage(ex)
                } finally {
                    loading = false
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun StaffRegisterScreen(api: ResLinkApi, session: SessionStore, onDone: (String) -> Unit, onBack: () -> Unit) {
    val roles = listOf("Admin", "Security", "Maintenance")
    var fullName by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var staffId by remember { mutableStateOf("") }
    var accessCode by remember { mutableStateOf("") }
    var role by remember { mutableStateOf("Security") }
    var residences by remember { mutableStateOf(listOf<ResidenceDto>()) }
    var selected by remember { mutableStateOf<ResidenceDto?>(null) }
    var roleExpanded by remember { mutableStateOf(false) }
    var resExpanded by remember { mutableStateOf(false) }
    var loading by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        residences = runCatching { api.residences() }.getOrDefault(emptyList())
        selected = residences.firstOrNull()
    }

    Column(Modifier.fillMaxSize().padding(24.dp).verticalScroll(rememberScrollState())) {
        TextButton(onClick = onBack) { Text("Back", color = Forest) }
        BrandLockup()
        Text("Staff sign-up", color = Ink, fontSize = 28.sp, fontWeight = FontWeight.Bold)
        Text("Admins, security and maintenance need the staff access code.", color = Muted, modifier = Modifier.padding(bottom = 16.dp))
        ResField(fullName, { fullName = it }, "Full name")
        Spacer(Modifier.height(10.dp))
        ResField(email, { email = it }, "Email")
        Spacer(Modifier.height(10.dp))
        ResField(password, { password = it }, "Password", password = true)
        Spacer(Modifier.height(10.dp))
        ResField(staffId, { staffId = it }, "Staff ID")
        Spacer(Modifier.height(10.dp))
        ResField(accessCode, { accessCode = it }, "Staff access code", password = true)
        Spacer(Modifier.height(10.dp))
        ExposedDropdownMenuBox(expanded = roleExpanded, onExpandedChange = { roleExpanded = it }) {
            OutlinedTextField(
                value = role,
                onValueChange = {},
                readOnly = true,
                label = { Text("Role") },
                trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(roleExpanded) },
                modifier = Modifier.menuAnchor(MenuAnchorType.PrimaryNotEditable).fillMaxWidth()
            )
            ExposedDropdownMenu(expanded = roleExpanded, onDismissRequest = { roleExpanded = false }) {
                roles.forEach {
                    DropdownMenuItem(text = { Text(it) }, onClick = { role = it; roleExpanded = false })
                }
            }
        }
        Spacer(Modifier.height(10.dp))
        ExposedDropdownMenuBox(expanded = resExpanded, onExpandedChange = { resExpanded = it }) {
            OutlinedTextField(
                value = selected?.name ?: "Select residence",
                onValueChange = {},
                readOnly = true,
                label = { Text("Residence") },
                trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(resExpanded) },
                modifier = Modifier.menuAnchor(MenuAnchorType.PrimaryNotEditable).fillMaxWidth()
            )
            ExposedDropdownMenu(expanded = resExpanded, onDismissRequest = { resExpanded = false }) {
                residences.forEach {
                    DropdownMenuItem(text = { Text(it.name) }, onClick = { selected = it; resExpanded = false })
                }
            }
        }
        ErrorText(error)
        Spacer(Modifier.height(16.dp))
        ResButton("Create staff account", loading) {
            val residence = selected
            if (residence == null) {
                error = "Choose a residence."
                return@ResButton
            }
            scope.launch {
                loading = true
                error = null
                try {
                    val auth = api.registerStaff(
                        StaffRegisterRequest(fullName, email, password, staffId, role, accessCode, residence.id)
                    )
                    session.save(auth)
                    onDone(auth.role)
                } catch (ex: Exception) {
                    error = userMessage(ex)
                } finally {
                    loading = false
                }
            }
        }
        Text("Demo staff code: RESLINK-STAFF-2026", color = Muted, fontSize = 12.sp, modifier = Modifier.padding(top = 12.dp))
    }
}

fun userMessage(ex: Throwable): String {
    return when (ex) {
        is HttpException -> when (ex.code()) {
            401 -> "Invalid credentials or access code."
            403 -> "Your role cannot use this action."
            409 -> "That email is already registered."
            else -> "Request failed (${ex.code()})."
        }
        else -> ex.message ?: "Could not reach the ResLink API. Is it running on port 5180?"
    }
}
