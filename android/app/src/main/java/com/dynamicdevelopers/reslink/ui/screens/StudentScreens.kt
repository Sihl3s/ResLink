package com.dynamicdevelopers.reslink.ui.screens

import androidx.compose.foundation.clickable
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
import androidx.compose.material3.Checkbox
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
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dynamicdevelopers.reslink.data.CreateCommentRequest
import com.dynamicdevelopers.reslink.data.CreateEmergencyRequest
import com.dynamicdevelopers.reslink.data.CreateMaintenanceRequest
import com.dynamicdevelopers.reslink.data.CreateMarketplaceItemRequest
import com.dynamicdevelopers.reslink.data.CreateNoiseRequest
import com.dynamicdevelopers.reslink.data.CreatePostRequest
import com.dynamicdevelopers.reslink.data.CreateStudyGroupRequest
import com.dynamicdevelopers.reslink.data.DashboardDto
import com.dynamicdevelopers.reslink.data.EventDto
import com.dynamicdevelopers.reslink.data.MaintenanceTicketDto
import com.dynamicdevelopers.reslink.data.MarketplaceItemDto
import com.dynamicdevelopers.reslink.data.NotificationDto
import com.dynamicdevelopers.reslink.data.PostDto
import com.dynamicdevelopers.reslink.data.ResLinkApi
import com.dynamicdevelopers.reslink.data.RewardDto
import com.dynamicdevelopers.reslink.data.SessionStore
import com.dynamicdevelopers.reslink.data.StudyGroupDto
import com.dynamicdevelopers.reslink.data.VoteRequest
import com.dynamicdevelopers.reslink.ui.auth.userMessage
import com.dynamicdevelopers.reslink.ui.common.ErrorText
import com.dynamicdevelopers.reslink.ui.common.ResButton
import com.dynamicdevelopers.reslink.ui.common.ResCard
import com.dynamicdevelopers.reslink.ui.common.ResField
import com.dynamicdevelopers.reslink.ui.common.ScreenHeader
import com.dynamicdevelopers.reslink.ui.common.StatusChip
import com.dynamicdevelopers.reslink.ui.theme.Muted
import com.dynamicdevelopers.reslink.ui.theme.Forest
import com.dynamicdevelopers.reslink.ui.theme.Ink
import kotlinx.coroutines.launch

@Composable
fun HomeScreen(api: ResLinkApi, session: SessionStore, role: String) {
    var dash by remember { mutableStateOf<DashboardDto?>(null) }
    var announcements by remember { mutableStateOf(listOf<PostDto>()) }
    var error by remember { mutableStateOf<String?>(null) }
    LaunchedEffect(Unit) {
        runCatching { api.dashboard() }
            .onSuccess { dash = it; session.updatePoints(it.points) }
            .onFailure { error = userMessage(it) }
        announcements = runCatching { api.posts() }.getOrDefault(emptyList()).filter { it.kind == "Announcement" }
    }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp)) {
        item {
            ScreenHeader("Good morning, ${session.fullName().substringBefore(" ")}", "Here's what's happening at the residence today.")
            ErrorText(error)
            ResCard {
                Text("${session.points()} pts", color = Forest, fontSize = 28.sp, fontWeight = FontWeight.Bold)
                Text("Reward points you can redeem in the app.", color = Muted)
            }
            Spacer(Modifier.height(12.dp))
        }
        if (announcements.isNotEmpty()) {
            item { Text("Announcements", color = Ink, fontWeight = FontWeight.SemiBold) }
        }
        items(announcements) { post ->
            ResCard(Modifier.padding(bottom = 10.dp, top = 8.dp)) {
                Text(post.title, color = Ink, fontWeight = FontWeight.SemiBold)
                Text(post.body, color = Muted, modifier = Modifier.padding(top = 6.dp))
                Text(post.createdAt.take(10), color = Muted, fontSize = 12.sp)
            }
        }
        items(dash?.highlights.orEmpty()) { tip ->
            ResCard(Modifier.padding(bottom = 10.dp)) {
                Text(tip, color = Ink)
            }
        }
        dash?.analytics?.let { stats ->
            item {
                Spacer(Modifier.height(8.dp))
                Text("Live residence pulse", color = Ink, fontWeight = FontWeight.SemiBold)
                Spacer(Modifier.height(8.dp))
                ResCard {
                    Text("Open tickets ${stats.openTickets} · Complaints ${stats.openComplaints} · Panic ${stats.openEmergencies}")
                    Text("Users ${stats.totalUsers} · RSVPs ${stats.totalRsvps}", color = Muted)
                }
            }
        }
    }
}

@Composable
fun FeedScreen(api: ResLinkApi, canPost: Boolean, canModerate: Boolean, userId: String = "") {
    var posts by remember { mutableStateOf(listOf<PostDto>()) }
    var title by remember { mutableStateOf("") }
    var body by remember { mutableStateOf("") }
    var isPoll by remember { mutableStateOf(false) }
    var options by remember { mutableStateOf("Braai, Movie night") }
    var comment by remember { mutableStateOf("") }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { posts = runCatching { api.posts() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }

    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Community Feed", "Residence discussions and polls")
            if (canPost) {
                ResCard {
                    ResField(title, { title = it }, "Title")
                    Spacer(Modifier.height(8.dp))
                    ResField(body, { body = it }, "What's happening?")
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Checkbox(isPoll, { isPoll = it })
                        Text("This is a poll")
                    }
                    if (isPoll) ResField(options, { options = it }, "Options, comma separated")
                    ErrorText(error)
                    Spacer(Modifier.height(8.dp))
                    ResButton("Publish") {
                        scope.launch {
                            try {
                                api.createPost(CreatePostRequest(title, body, isPoll, if (isPoll) options.split(",").map { it.trim() } else null))
                                title = ""; body = ""; reload()
                            } catch (ex: Exception) { error = userMessage(ex) }
                        }
                    }
                }
            }
        }
        items(posts) { post ->
            ResCard {
                Text(post.title, color = Ink, fontWeight = FontWeight.SemiBold)
                Text(post.authorName, color = Muted, fontSize = 12.sp)
                Text(post.body, modifier = Modifier.padding(top = 8.dp))
                post.pollOptions.forEach { option ->
                    Text(
                        "${option.label} · ${option.votes}",
                        color = Forest,
                        modifier = Modifier.padding(top = 6.dp).clickable {
                            scope.launch { api.vote(post.id, VoteRequest(option.index)); reload() }
                        }
                    )
                }
                post.comments.forEach { Text("${it.authorName}: ${it.body}", color = Muted, fontSize = 13.sp) }
                if (canPost) {
                    ResField(comment, { comment = it }, "Comment")
                    TextButton(onClick = {
                        scope.launch { api.comment(post.id, CreateCommentRequest(comment)); comment = ""; reload() }
                    }) { Text("Reply", color = Forest) }
                }
                if (canModerate || post.authorId == userId) {
                    TextButton(onClick = { scope.launch { api.deletePost(post.id); reload() } }) { Text("Remove post") }
                }
            }
        }
    }
}

@Composable
fun EventsScreen(api: ResLinkApi, isStudent: Boolean, isAdmin: Boolean) {
    var events by remember { mutableStateOf(listOf<EventDto>()) }
    var title by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var location by remember { mutableStateOf("") }
    var startsAt by remember { mutableStateOf("2026-08-20T17:00:00Z") }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { events = runCatching { api.events() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }

    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Residence events", "RSVP and demo check-in codes")
            if (isAdmin) {
                ResCard {
                    ResField(title, { title = it }, "Event title")
                    Spacer(Modifier.height(8.dp))
                    ResField(description, { description = it }, "Description")
                    Spacer(Modifier.height(8.dp))
                    ResField(location, { location = it }, "Location")
                    Spacer(Modifier.height(8.dp))
                    ResField(startsAt, { startsAt = it }, "Starts at (ISO)")
                    Spacer(Modifier.height(8.dp))
                    ResButton("Create event") {
                        scope.launch { api.createEvent(com.dynamicdevelopers.reslink.data.CreateEventRequest(title, description, location, startsAt)); reload() }
                    }
                }
            }
        }
        items(events) { event ->
            ResCard {
                Text(event.title, color = Ink, fontWeight = FontWeight.SemiBold)
                Text("${event.location} · ${event.startsAt.take(16).replace('T', ' ')}", color = Muted)
                Text(event.description, modifier = Modifier.padding(top = 8.dp))
                Text("RSVPs: ${event.rsvpCount}", color = Forest, modifier = Modifier.padding(top = 8.dp))
                Text("Check-in code: ${event.checkInCode}", color = Muted, fontSize = 12.sp)
                if (isStudent) {
                    TextButton(onClick = {
                        scope.launch {
                            if (event.hasRsvp) api.cancelRsvp(event.id) else api.rsvp(event.id)
                            reload()
                        }
                    }) { Text(if (event.hasRsvp) "Cancel RSVP" else "RSVP (+10 pts)", color = Forest) }
                }
            }
        }
    }
}

@Composable
fun MarketplaceScreen(api: ResLinkApi, canSell: Boolean) {
    var items by remember { mutableStateOf(listOf<MarketplaceItemDto>()) }
    var title by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var category by remember { mutableStateOf("Textbooks") }
    var price by remember { mutableStateOf("150") }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { items = runCatching { api.marketplace() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }

    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Student marketplace", "Textbooks, furniture and extras")
            if (canSell) {
                ResCard {
                    ResField(title, { title = it }, "Item")
                    Spacer(Modifier.height(8.dp))
                    ResField(description, { description = it }, "Description")
                    Spacer(Modifier.height(8.dp))
                    ResField(category, { category = it }, "Category")
                    Spacer(Modifier.height(8.dp))
                    ResField(price, { price = it }, "Price (R)")
                    Spacer(Modifier.height(8.dp))
                    ResButton("List item") {
                        scope.launch {
                            api.createListing(CreateMarketplaceItemRequest(title, description, category, price.toDoubleOrNull() ?: 0.0))
                            reload()
                        }
                    }
                }
            }
        }
        items(items) { item ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(item.title, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(if (item.isSold) "Sold" else "R${item.price.toInt()}")
                }
                Text("${item.category} · ${item.sellerName}", color = Muted)
                Text(item.description, modifier = Modifier.padding(top = 8.dp))
                if (canSell && !item.isSold) {
                    TextButton(onClick = { scope.launch { api.markSold(item.id); reload() } }) { Text("Mark sold", color = Forest) }
                }
                if (canSell && item.isSold) {
                    TextButton(onClick = { scope.launch { api.markAvailable(item.id); reload() } }) { Text("Mark available", color = Forest) }
                }
            }
        }
    }
}

@Composable
fun MaintenanceScreen(api: ResLinkApi, isStudent: Boolean, canUpdate: Boolean) {
    var tickets by remember { mutableStateOf(listOf<MaintenanceTicketDto>()) }
    var title by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var location by remember { mutableStateOf("") }
    var photo by remember { mutableStateOf("") }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { tickets = runCatching { api.tickets() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }

    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Maintenance", if (isStudent) "Photo-based reporting with live status" else "Ticket board")
            if (isStudent) {
                ResCard {
                    ResField(title, { title = it }, "Issue")
                    Spacer(Modifier.height(8.dp))
                    ResField(description, { description = it }, "Details")
                    Spacer(Modifier.height(8.dp))
                    ResField(location, { location = it }, "Location")
                    Spacer(Modifier.height(8.dp))
                    ResField(photo, { photo = it }, "Photo URL (optional)")
                    Spacer(Modifier.height(8.dp))
                    ResButton("Submit ticket") {
                        scope.launch {
                            api.createTicket(CreateMaintenanceRequest(title, description, location, photo.ifBlank { null }))
                            reload()
                        }
                    }
                }
            }
        }
        items(tickets) { ticket ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(ticket.title, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(ticket.status)
                }
                Text("${ticket.location} · ${ticket.reporterName}", color = Muted)
                Text(ticket.description, modifier = Modifier.padding(top = 8.dp))
                ticket.resolutionNotes?.let { Text("Notes: $it", color = Muted) }
                if (canUpdate) {
                    Row {
                        listOf("Open", "InProgress", "Resolved").forEach { status ->
                            FilterChip(selected = ticket.status == status, onClick = {
                                scope.launch { api.updateTicket(ticket.id, com.dynamicdevelopers.reslink.data.UpdateTicketStatusRequest(status, "Updated by staff")); reload() }
                            }, label = { Text(status) }, modifier = Modifier.padding(end = 6.dp))
                        }
                    }
                }
                if (isStudent && ticket.status == "Open") {
                    TextButton(onClick = {
                        scope.launch { api.updateTicket(ticket.id, com.dynamicdevelopers.reslink.data.UpdateTicketStatusRequest("Cancelled", "Cancelled by student")); reload() }
                    }) { Text("Cancel request", color = Forest) }
                }
            }
        }
    }
}

@Composable
fun PanicScreen(api: ResLinkApi, session: SessionStore) {
    var sent by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }
    var mine by remember { mutableStateOf(listOf<com.dynamicdevelopers.reslink.data.EmergencyAlertDto>()) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { mine = runCatching { api.myEmergencies() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Emergency", "One tap alerts residence security")
            ResCard {
                Text("Your location will be sent as ${session.room().ifBlank { "your room" }}, ${session.residenceName()}.")
                Spacer(Modifier.height(16.dp))
                ResButton(if (sent) "Alert sent" else "Hold to send panic alert") {
                    scope.launch {
                        try {
                            api.panic(CreateEmergencyRequest(null, "Panic button activated"))
                            sent = true
                            reload()
                        } catch (ex: Exception) {
                            error = userMessage(ex)
                        }
                    }
                }
                ErrorText(error)
            }
        }
        items(mine) { alert ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(alert.location, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(alert.status)
                }
                if (alert.status == "Open") {
                    TextButton(onClick = { scope.launch { api.withdrawEmergency(alert.id); reload() } }) {
                        Text("False alarm", color = Forest)
                    }
                }
            }
        }
    }
}

@Composable
fun NoiseScreen(api: ResLinkApi) {
    var location by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    var anonymous by remember { mutableStateOf(true) }
    var sent by remember { mutableStateOf(false) }
    var mine by remember { mutableStateOf(listOf<com.dynamicdevelopers.reslink.data.NoiseComplaintDto>()) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { mine = runCatching { api.myNoise() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Noise complaint", "Anonymous reporting to security")
            ResCard {
                ResField(location, { location = it }, "Location")
                Spacer(Modifier.height(8.dp))
                ResField(description, { description = it }, "What is happening?")
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Checkbox(anonymous, { anonymous = it })
                    Text("Report anonymously")
                }
                ResButton(if (sent) "Sent to security" else "Submit complaint") {
                    scope.launch {
                        api.createNoise(CreateNoiseRequest(location, description, anonymous))
                        sent = true
                        reload()
                    }
                }
            }
        }
        items(mine) { item ->
            ResCard {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(item.location, color = Ink, fontWeight = FontWeight.SemiBold)
                    StatusChip(item.status)
                }
                if (item.status == "Open") {
                    TextButton(onClick = { scope.launch { api.withdrawNoise(item.id); reload() } }) {
                        Text("Withdraw", color = Forest)
                    }
                }
            }
        }
    }
}

@Composable
fun RewardsScreen(api: ResLinkApi, session: SessionStore) {
    var rewards by remember { mutableStateOf(listOf<RewardDto>()) }
    var redemptions by remember { mutableStateOf(listOf<com.dynamicdevelopers.reslink.data.RedemptionDto>()) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch {
        rewards = runCatching { api.rewards() }.getOrDefault(emptyList())
        redemptions = runCatching { api.redemptions() }.getOrDefault(emptyList())
    }
    LaunchedEffect(Unit) { reload() }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Rewards", "You have ${session.points()} points")
            ErrorText(error)
        }
        items(rewards) { reward ->
            ResCard {
                Text(reward.name, color = Ink, fontWeight = FontWeight.SemiBold)
                Text(reward.description, color = Muted)
                Text("${reward.pointsCost} pts", color = Forest, modifier = Modifier.padding(top = 8.dp))
                TextButton(onClick = {
                    scope.launch {
                        try {
                            api.redeem(reward.id)
                            session.updatePoints(session.points() - reward.pointsCost)
                            reload()
                        } catch (ex: Exception) {
                            error = userMessage(ex)
                        }
                    }
                }) { Text("Redeem", color = Forest) }
            }
        }
        if (redemptions.isNotEmpty()) {
            item { Text("Recent redemptions", color = Ink, fontWeight = FontWeight.SemiBold) }
        }
        items(redemptions) { item ->
            ResCard {
                Text(item.rewardName, color = Ink, fontWeight = FontWeight.SemiBold)
                Text("${item.pointsCost} pts", color = Muted)
                TextButton(onClick = {
                    scope.launch {
                        api.cancelRedemption(item.id)
                        session.updatePoints(session.points() + item.pointsCost)
                        reload()
                    }
                }) { Text("Undo redeem", color = Forest) }
            }
        }
    }
}

@Composable
fun StudyGroupsScreen(api: ResLinkApi) {
    var groups by remember { mutableStateOf(listOf<StudyGroupDto>()) }
    var name by remember { mutableStateOf("") }
    var topic by remember { mutableStateOf("") }
    var description by remember { mutableStateOf("") }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { groups = runCatching { api.studyGroups() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item {
            ScreenHeader("Study groups", "Find partners by module or interest")
            ResCard {
                ResField(name, { name = it }, "Group name")
                Spacer(Modifier.height(8.dp))
                ResField(topic, { topic = it }, "Topic")
                Spacer(Modifier.height(8.dp))
                ResField(description, { description = it }, "Description")
                Spacer(Modifier.height(8.dp))
                ResButton("Create group") {
                    scope.launch { api.createGroup(CreateStudyGroupRequest(name, topic, description)); reload() }
                }
            }
        }
        items(groups) { group ->
            ResCard {
                Text(group.name, color = Ink, fontWeight = FontWeight.SemiBold)
                Text("${group.topic} · ${group.memberCount} members", color = Muted)
                Text(group.description)
                if (!group.isMember) {
                    TextButton(onClick = { scope.launch { api.joinGroup(group.id); reload() } }) { Text("Join (+5 pts)", color = Forest) }
                } else {
                    Text("You are a member", color = Forest)
                    TextButton(onClick = { scope.launch { api.leaveGroup(group.id); reload() } }) { Text("Leave", color = Forest) }
                }
            }
        }
    }
}

@Composable
fun NotificationsScreen(api: ResLinkApi) {
    var items by remember { mutableStateOf(listOf<NotificationDto>()) }
    val scope = rememberCoroutineScope()
    fun reload() = scope.launch { items = runCatching { api.notifications() }.getOrDefault(emptyList()) }
    LaunchedEffect(Unit) { reload() }
    LazyColumn(Modifier.fillMaxSize().padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item { ScreenHeader("Notifications", "Residence alerts and ticket updates") }
        items(items) { item ->
            ResCard(Modifier.clickable { scope.launch { api.readNotification(item.id); reload() } }) {
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text(item.title, color = Ink, fontWeight = FontWeight.SemiBold)
                    if (!item.isRead) StatusChip("New")
                }
                Text(item.body, color = Muted)
            }
        }
    }
}

@Composable
fun ProfileScreen(session: SessionStore, onLogout: () -> Unit) {
    Column(Modifier.fillMaxSize().padding(20.dp)) {
        ScreenHeader("Profile", session.role() ?: "")
        ResCard {
            Text(session.fullName(), color = Ink, fontSize = 22.sp, fontWeight = FontWeight.Bold)
            Text(session.email(), color = Muted)
            Text(session.residenceName(), modifier = Modifier.padding(top = 8.dp))
            if (session.room().isNotBlank()) Text("Room ${session.room()}", color = Muted)
            Text("${session.points()} reward points", color = Forest, modifier = Modifier.padding(top = 8.dp))
        }
        Spacer(Modifier.height(20.dp))
        ResButton("Log out", onClick = onLogout)
    }
}
