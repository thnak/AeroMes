package com.iotviet.aeromes.navigation

sealed class Screen(val route: String) {
    data object Login : Screen("login")
    data object Dashboard : Screen("dashboard")
    data object Jobs : Screen("jobs")
    data object JobDetail : Screen("jobs/{jobId}") {
        fun createRoute(jobId: Long) = "jobs/$jobId"
    }
    data object Production : Screen("production/{jobId}") {
        fun createRoute(jobId: Long) = "production/$jobId"
    }
    data object Quality : Screen("quality")
    data object QualityDetail : Screen("quality/{orderId}") {
        fun createRoute(orderId: Int) = "quality/$orderId"
    }
    data object Inventory : Screen("inventory")
}
