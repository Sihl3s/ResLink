package com.dynamicdevelopers.reslink.data

import android.content.Context

class SessionStore(context: Context) {
    private val prefs = context.getSharedPreferences("reslink_session", Context.MODE_PRIVATE)

    fun save(auth: AuthResponse) {
        prefs.edit()
            .putString("token", auth.token)
            .putString("userId", auth.userId)
            .putString("fullName", auth.fullName)
            .putString("email", auth.email)
            .putString("role", auth.role)
            .putInt("points", auth.points)
            .putString("residenceName", auth.residenceName)
            .putString("room", auth.room)
            .apply()
    }

    fun updatePoints(points: Int) {
        prefs.edit().putInt("points", points).apply()
    }

    fun token(): String? = prefs.getString("token", null)
    fun userId(): String = prefs.getString("userId", "") ?: ""
    fun role(): String? = prefs.getString("role", null)
    fun fullName(): String = prefs.getString("fullName", "Resident") ?: "Resident"
    fun email(): String = prefs.getString("email", "") ?: ""
    fun points(): Int = prefs.getInt("points", 0)
    fun residenceName(): String = prefs.getString("residenceName", "Campus Flow Residence") ?: "Campus Flow Residence"
    fun room(): String = prefs.getString("room", "") ?: ""
    fun isLoggedIn(): Boolean = !token().isNullOrBlank()

    fun clear() {
        prefs.edit().clear().apply()
    }
}
