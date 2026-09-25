package com.dynamicdevelopers.reslink.ui.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.outlined.Build
import androidx.compose.material.icons.outlined.Event
import androidx.compose.material.icons.outlined.Forum
import androidx.compose.material.icons.outlined.Home
import androidx.compose.material.icons.outlined.MoreHoriz
import androidx.compose.material.icons.outlined.Notifications
import androidx.compose.material.icons.outlined.Person
import androidx.compose.material.icons.outlined.Security
import androidx.compose.material.icons.outlined.Storefront
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.dynamicdevelopers.reslink.data.ResLinkApi
import com.dynamicdevelopers.reslink.data.SessionStore
import com.dynamicdevelopers.reslink.ui.auth.LoginScreen
import com.dynamicdevelopers.reslink.ui.auth.StaffRegisterScreen
import com.dynamicdevelopers.reslink.ui.auth.StudentRegisterScreen
import com.dynamicdevelopers.reslink.ui.screens.EventsScreen
import com.dynamicdevelopers.reslink.ui.screens.FeedScreen
import com.dynamicdevelopers.reslink.ui.screens.HomeScreen
import com.dynamicdevelopers.reslink.ui.screens.MaintenanceScreen
import com.dynamicdevelopers.reslink.ui.screens.MarketplaceScreen
import com.dynamicdevelopers.reslink.ui.screens.MoreMenuScreen
import com.dynamicdevelopers.reslink.ui.screens.NoiseScreen
import com.dynamicdevelopers.reslink.ui.screens.NotificationsScreen
import com.dynamicdevelopers.reslink.ui.screens.PanicScreen
import com.dynamicdevelopers.reslink.ui.screens.ProfileScreen
import com.dynamicdevelopers.reslink.ui.screens.RewardsScreen
import com.dynamicdevelopers.reslink.ui.screens.SecurityQueuesScreen
import com.dynamicdevelopers.reslink.ui.screens.StudyGroupsScreen
import com.dynamicdevelopers.reslink.ui.screens.UsersScreen
import com.dynamicdevelopers.reslink.ui.theme.CardWhite
import com.dynamicdevelopers.reslink.ui.theme.Forest
import com.dynamicdevelopers.reslink.ui.theme.Page

data class Tab(val route: String, val label: String, val icon: ImageVector)

// Bottom navigation and role shells follow Jetpack Compose Navigation (Android Developers, 2025a).

@Composable
fun ResLinkNav(api: ResLinkApi, session: SessionStore) {
    val nav = rememberNavController()
    val start = if (session.isLoggedIn()) roleHome(session.role()) else "login"

    NavHost(navController = nav, startDestination = start) {
        composable("login") {
            LoginScreen(
                api, session,
                onLoggedIn = { role -> nav.navigate(roleHome(role)) { popUpTo("login") { inclusive = true } } },
                onStudentSignUp = { nav.navigate("registerStudent") },
                onStaffSignUp = { nav.navigate("registerStaff") }
            )
        }
        composable("registerStudent") {
            StudentRegisterScreen(api, session, onDone = { role -> nav.navigate(roleHome(role)) { popUpTo("login") { inclusive = true } } }, onBack = { nav.popBackStack() })
        }
        composable("registerStaff") {
            StaffRegisterScreen(api, session, onDone = { role -> nav.navigate(roleHome(role)) { popUpTo("login") { inclusive = true } } }, onBack = { nav.popBackStack() })
        }
        composable("student") { RoleShell("Student", studentTabs, api, session, onLogout = { logout(session, nav) }) }
        composable("admin") { RoleShell("Admin", adminTabs, api, session, onLogout = { logout(session, nav) }) }
        composable("security") { RoleShell("Security", securityTabs, api, session, onLogout = { logout(session, nav) }) }
        composable("maintenance") { RoleShell("Maintenance", maintenanceTabs, api, session, onLogout = { logout(session, nav) }) }
    }
}

private val studentTabs = listOf(
    Tab("home", "Home", Icons.Outlined.Home),
    Tab("feed", "Community", Icons.Outlined.Forum),
    Tab("events", "Events", Icons.Outlined.Event),
    Tab("market", "Market", Icons.Outlined.Storefront),
    Tab("more", "More", Icons.Outlined.MoreHoriz)
)

private val adminTabs = listOf(
    Tab("home", "Home", Icons.Outlined.Home),
    Tab("users", "Users", Icons.Outlined.Person),
    Tab("events", "Events", Icons.Outlined.Event),
    Tab("tickets", "Tickets", Icons.Outlined.Build),
    Tab("more", "More", Icons.Outlined.MoreHoriz)
)

private val securityTabs = listOf(
    Tab("home", "Home", Icons.Outlined.Home),
    Tab("desk", "Desk", Icons.Outlined.Security),
    Tab("notifications", "Alerts", Icons.Outlined.Notifications),
    Tab("profile", "Profile", Icons.Outlined.Person)
)

private val maintenanceTabs = listOf(
    Tab("home", "Home", Icons.Outlined.Home),
    Tab("tickets", "Tickets", Icons.Outlined.Build),
    Tab("notifications", "Alerts", Icons.Outlined.Notifications),
    Tab("profile", "Profile", Icons.Outlined.Person)
)

@Composable
private fun RoleShell(
    role: String,
    tabs: List<Tab>,
    api: ResLinkApi,
    session: SessionStore,
    onLogout: () -> Unit
) {
    val nav = rememberNavController()
    val backStack by nav.currentBackStackEntryAsState()
    val current = backStack?.destination?.route ?: tabs.first().route
    val showBar = tabs.any { it.route == current }

    Scaffold(
        containerColor = Page,
        bottomBar = {
            if (showBar) {
                NavigationBar(containerColor = CardWhite) {
                    tabs.forEach { tab ->
                        NavigationBarItem(
                            selected = current == tab.route,
                            onClick = {
                                nav.navigate(tab.route) {
                                    popUpTo(nav.graph.findStartDestination().id) { saveState = true }
                                    launchSingleTop = true
                                    restoreState = true
                                }
                            },
                            icon = { Icon(tab.icon, tab.label, tint = if (current == tab.route) Forest else androidx.compose.ui.graphics.Color.Gray) },
                            label = { Text(tab.label) }
                        )
                    }
                }
            }
        }
    ) { padding ->
        NavHost(navController = nav, startDestination = "home", modifier = Modifier.padding(padding)) {
            composable("home") { HomeScreen(api, session, role) }
            composable("feed") { FeedScreen(api, canPost = role == "Student" || role == "Admin", canModerate = role == "Admin", userId = session.userId()) }
            composable("events") { EventsScreen(api, isStudent = role == "Student", isAdmin = role == "Admin") }
            composable("market") { MarketplaceScreen(api, canSell = role == "Student" || role == "Admin") }
            composable("tickets") { MaintenanceScreen(api, isStudent = role == "Student", canUpdate = role == "Maintenance" || role == "Admin") }
            composable("maintenance") { MaintenanceScreen(api, isStudent = true, canUpdate = false) }
            composable("panic") { PanicScreen(api, session) }
            composable("noise") { NoiseScreen(api) }
            composable("rewards") { RewardsScreen(api, session) }
            composable("groups") { StudyGroupsScreen(api) }
            composable("notifications") { NotificationsScreen(api) }
            composable("users") { UsersScreen(api) }
            composable("desk") { SecurityQueuesScreen(api) }
            composable("profile") { ProfileScreen(session, onLogout) }
            composable("more") {
                MoreMenuScreen(role) { route -> nav.navigate(route) }
            }
        }
    }
}

private fun roleHome(role: String?): String = when (role) {
    "Admin" -> "admin"
    "Security" -> "security"
    "Maintenance" -> "maintenance"
    else -> "student"
}

private fun logout(session: SessionStore, nav: androidx.navigation.NavHostController) {
    session.clear()
    nav.navigate("login") { popUpTo(0) { inclusive = true } }
}
