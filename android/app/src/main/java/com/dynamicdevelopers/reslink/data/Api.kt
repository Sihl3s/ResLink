package com.dynamicdevelopers.reslink.data

// Retrofit maps HTTP verbs to Kotlin suspend functions for the ASP.NET API (Square, 2024).

import com.dynamicdevelopers.reslink.BuildConfig
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import okhttp3.Interceptor
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path

interface ResLinkApi {
    @GET("api/auth/residences")
    suspend fun residences(): List<ResidenceDto>

    @POST("api/auth/login")
    suspend fun login(@Body body: LoginRequest): AuthResponse

    @POST("api/auth/register/student")
    suspend fun registerStudent(@Body body: StudentRegisterRequest): AuthResponse

    @POST("api/auth/register/staff")
    suspend fun registerStaff(@Body body: StaffRegisterRequest): AuthResponse

    @GET("api/auth/me")
    suspend fun me(): AuthResponse

    @GET("api/dashboard")
    suspend fun dashboard(): DashboardDto

    @GET("api/profile")
    suspend fun profile(): UserSummaryDto

    @GET("api/posts")
    suspend fun posts(): List<PostDto>

    @POST("api/posts")
    suspend fun createPost(@Body body: CreatePostRequest): PostDto

    @POST("api/posts/{id}/comments")
    suspend fun comment(@Path("id") id: String, @Body body: CreateCommentRequest): CommentDto

    @POST("api/posts/{id}/vote")
    suspend fun vote(@Path("id") id: String, @Body body: VoteRequest): Response<Unit>

    @DELETE("api/posts/{id}")
    suspend fun deletePost(@Path("id") id: String): Response<Unit>

    @GET("api/events")
    suspend fun events(): List<EventDto>

    @POST("api/events")
    suspend fun createEvent(@Body body: CreateEventRequest): EventDto

    @POST("api/events/{id}/rsvp")
    suspend fun rsvp(@Path("id") id: String): Response<Unit>

    @DELETE("api/events/{id}/rsvp")
    suspend fun cancelRsvp(@Path("id") id: String): Response<Unit>

    @GET("api/marketplace")
    suspend fun marketplace(): List<MarketplaceItemDto>

    @POST("api/marketplace")
    suspend fun createListing(@Body body: CreateMarketplaceItemRequest): MarketplaceItemDto

    @PUT("api/marketplace/{id}/sold")
    suspend fun markSold(@Path("id") id: String): Response<Unit>

    @PUT("api/marketplace/{id}/unsold")
    suspend fun markAvailable(@Path("id") id: String): Response<Unit>

    @GET("api/maintenance")
    suspend fun tickets(): List<MaintenanceTicketDto>

    @POST("api/maintenance")
    suspend fun createTicket(@Body body: CreateMaintenanceRequest): MaintenanceTicketDto

    @PUT("api/maintenance/{id}/status")
    suspend fun updateTicket(@Path("id") id: String, @Body body: UpdateTicketStatusRequest): Response<Unit>

    @GET("api/noise")
    suspend fun noise(): List<NoiseComplaintDto>

    @POST("api/noise")
    suspend fun createNoise(@Body body: CreateNoiseRequest): Response<Unit>

    @PUT("api/noise/{id}/status")
    suspend fun updateNoise(@Path("id") id: String, @Body body: UpdateStatusRequest): Response<Unit>

    @GET("api/emergencies")
    suspend fun emergencies(): List<EmergencyAlertDto>

    @POST("api/emergencies")
    suspend fun panic(@Body body: CreateEmergencyRequest): Response<Unit>

    @PUT("api/emergencies/{id}/status")
    suspend fun updateEmergency(@Path("id") id: String, @Body body: UpdateStatusRequest): Response<Unit>

    @GET("api/study-groups")
    suspend fun studyGroups(): List<StudyGroupDto>

    @POST("api/study-groups")
    suspend fun createGroup(@Body body: CreateStudyGroupRequest): StudyGroupDto

    @POST("api/study-groups/{id}/join")
    suspend fun joinGroup(@Path("id") id: String): Response<Unit>

    @DELETE("api/study-groups/{id}/leave")
    suspend fun leaveGroup(@Path("id") id: String): Response<Unit>

    @GET("api/rewards")
    suspend fun rewards(): List<RewardDto>

    @GET("api/rewards/redemptions")
    suspend fun redemptions(): List<RedemptionDto>

    @POST("api/rewards/{id}/redeem")
    suspend fun redeem(@Path("id") id: String): Response<Unit>

    @POST("api/rewards/redemptions/{id}/cancel")
    suspend fun cancelRedemption(@Path("id") id: String): Response<Unit>

    @GET("api/visitors")
    suspend fun visitors(): List<VisitorDto>

    @PUT("api/visitors/{id}/status")
    suspend fun updateVisitor(@Path("id") id: String, @Body body: UpdateStatusRequest): Response<Unit>

    @GET("api/emergencies/mine")
    suspend fun myEmergencies(): List<EmergencyAlertDto>

    @PUT("api/emergencies/{id}/withdraw")
    suspend fun withdrawEmergency(@Path("id") id: String): Response<Unit>

    @GET("api/noise/mine")
    suspend fun myNoise(): List<NoiseComplaintDto>

    @PUT("api/noise/{id}/withdraw")
    suspend fun withdrawNoise(@Path("id") id: String): Response<Unit>

    @GET("api/notifications")
    suspend fun notifications(): List<NotificationDto>

    @PUT("api/notifications/{id}/read")
    suspend fun readNotification(@Path("id") id: String): Response<Unit>

    @GET("api/admin/analytics")
    suspend fun analytics(): AnalyticsDto

    @GET("api/admin/users")
    suspend fun users(): List<UserSummaryDto>

    @PUT("api/admin/users/{id}/active")
    suspend fun setActive(@Path("id") id: String, @Body body: UpdateActiveRequest): Response<Unit>
}

object ApiFactory {
    fun create(session: SessionStore): ResLinkApi {
        val moshi = Moshi.Builder().add(KotlinJsonAdapterFactory()).build()
        val logging = HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BASIC }
        val auth = Interceptor { chain ->
            val token = session.token()
            val request = if (token.isNullOrBlank()) {
                chain.request()
            } else {
                chain.request().newBuilder().header("Authorization", "Bearer $token").build()
            }
            chain.proceed(request)
        }
        val client = OkHttpClient.Builder()
            .addInterceptor(auth)
            .addInterceptor(logging)
            .build()
        return Retrofit.Builder()
            .baseUrl(BuildConfig.API_BASE_URL)
            .client(client)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
            .create(ResLinkApi::class.java)
    }
}
