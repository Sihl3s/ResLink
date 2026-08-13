package com.dynamicdevelopers.reslink.data

data class AuthResponse(
    val token: String,
    val userId: String,
    val fullName: String,
    val email: String,
    val role: String,
    val points: Int,
    val residenceId: String?,
    val residenceName: String?,
    val room: String?,
    val studentNumber: String?,
    val staffId: String?
)

data class LoginRequest(val email: String, val password: String)

data class StudentRegisterRequest(
    val fullName: String,
    val email: String,
    val password: String,
    val studentNumber: String,
    val residenceId: String,
    val room: String
)

data class StaffRegisterRequest(
    val fullName: String,
    val email: String,
    val password: String,
    val staffId: String,
    val role: String,
    val accessCode: String,
    val residenceId: String
)

data class ResidenceDto(val id: String, val name: String, val campus: String, val address: String)

data class UserSummaryDto(
    val id: String,
    val fullName: String,
    val email: String,
    val role: String,
    val points: Int,
    val isActive: Boolean,
    val room: String?,
    val studentNumber: String?,
    val staffId: String?,
    val residenceId: String?,
    val residenceName: String?
)

data class CommentDto(val id: String, val authorName: String, val body: String, val createdAt: String)
data class PollOptionDto(val index: Int, val label: String, val votes: Int)
data class PostDto(
    val id: String,
    val authorId: String = "",
    val authorName: String,
    val authorRole: String = "Student",
    val title: String,
    val body: String,
    val kind: String = "Post",
    val isPoll: Boolean,
    val likeCount: Int = 0,
    val pollOptions: List<PollOptionDto>,
    val comments: List<CommentDto>,
    val createdAt: String
)

data class CreatePostRequest(
    val title: String,
    val body: String,
    val kind: String? = null,
    val isPoll: Boolean,
    val pollOptions: List<String>?
)

data class CreateCommentRequest(val body: String)
data class VoteRequest(val optionIndex: Int)

data class EventDto(
    val id: String,
    val title: String,
    val description: String,
    val location: String,
    val category: String = "Social",
    val isFeatured: Boolean = false,
    val startsAt: String,
    val checkInCode: String,
    val rsvpCount: Int,
    val hasRsvp: Boolean
)

data class CreateEventRequest(
    val title: String,
    val description: String,
    val location: String,
    val startsAt: String,
    val category: String? = "Social"
)

data class MarketplaceItemDto(
    val id: String,
    val sellerName: String,
    val sellerId: String,
    val title: String,
    val description: String,
    val category: String,
    val condition: String = "Good",
    val sellerRoom: String? = null,
    val price: Double,
    val isSold: Boolean,
    val createdAt: String
)

data class CreateMarketplaceItemRequest(
    val title: String,
    val description: String,
    val category: String,
    val price: Double,
    val condition: String? = "Good"
)

data class MaintenanceTicketDto(
    val id: String,
    val reporterName: String,
    val title: String,
    val description: String,
    val location: String,
    val category: String = "General",
    val priority: String = "Medium",
    val photoUrl: String?,
    val status: String,
    val resolutionNotes: String?,
    val createdAt: String,
    val updatedAt: String
)

data class CreateMaintenanceRequest(
    val title: String,
    val description: String,
    val location: String,
    val photoUrl: String?
)

data class UpdateTicketStatusRequest(val status: String, val resolutionNotes: String?)
data class UpdateStatusRequest(val status: String)
data class UpdateActiveRequest(val isActive: Boolean)

data class NoiseComplaintDto(
    val id: String,
    val reporterName: String,
    val location: String,
    val description: String,
    val isAnonymous: Boolean,
    val status: String,
    val createdAt: String
)

data class CreateNoiseRequest(val location: String, val description: String, val isAnonymous: Boolean)

data class EmergencyAlertDto(
    val id: String,
    val reporterName: String,
    val location: String,
    val message: String,
    val status: String,
    val createdAt: String
)

data class CreateEmergencyRequest(val location: String?, val message: String?)

data class StudyGroupDto(
    val id: String,
    val name: String,
    val topic: String,
    val courseCode: String = "",
    val description: String,
    val schedule: String = "",
    val location: String = "",
    val memberCount: Int,
    val maxMembers: Int = 12,
    val isMember: Boolean,
    val createdAt: String
)

data class CreateStudyGroupRequest(val name: String, val topic: String, val description: String)

data class RewardDto(val id: String, val name: String, val description: String, val category: String = "Food", val pointsCost: Int, val stock: Int = 0)
data class RedemptionDto(
    val id: String,
    val rewardId: String = "",
    val rewardName: String,
    val pointsCost: Int = 0,
    val createdAt: String
)

data class VisitorDto(
    val id: String,
    val visitorName: String,
    val hostName: String,
    val hostRoom: String,
    val purpose: String,
    val status: String,
    val createdAt: String
)
data class NotificationDto(
    val id: String,
    val title: String,
    val body: String,
    val type: String,
    val isRead: Boolean,
    val createdAt: String
)

data class AnalyticsDto(
    val totalUsers: Int,
    val students: Int,
    val openTickets: Int,
    val openComplaints: Int,
    val openEmergencies: Int,
    val totalRsvps: Int,
    val pointsIssued: Int,
    val marketplaceListings: Int
)

data class DashboardDto(
    val role: String,
    val fullName: String,
    val points: Int,
    val highlights: List<String>,
    val analytics: AnalyticsDto?
)
