package com.dynamicdevelopers.reslink

import android.app.Application
import com.dynamicdevelopers.reslink.data.ApiFactory
import com.dynamicdevelopers.reslink.data.ResLinkApi
import com.dynamicdevelopers.reslink.data.SessionStore

class ResLinkApp : Application() {
    lateinit var session: SessionStore
        private set
    lateinit var api: ResLinkApi
        private set

    override fun onCreate() {
        super.onCreate()
        session = SessionStore(this)
        api = ApiFactory.create(session)
    }
}
