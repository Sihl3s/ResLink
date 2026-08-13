package com.dynamicdevelopers.reslink

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.dynamicdevelopers.reslink.ui.navigation.ResLinkNav
import com.dynamicdevelopers.reslink.ui.theme.ResLinkTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        val app = application as ResLinkApp
        setContent {
            ResLinkTheme {
                ResLinkNav(app.api, app.session)
            }
        }
    }
}
