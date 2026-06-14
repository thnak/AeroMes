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

    // Feature A: Lot Lookup
    data object LotLookup : Screen("lot-lookup?lotNumber={lotNumber}") {
        fun createRoute(lotNumber: String? = null) =
            if (lotNumber != null) "lot-lookup?lotNumber=$lotNumber" else "lot-lookup"
    }

    // Feature B: Lot Hold/Release
    data object LotHold : Screen("lot-hold?lotNumber={lotNumber}") {
        fun createRoute(lotNumber: String? = null) =
            if (lotNumber != null) "lot-hold?lotNumber=$lotNumber" else "lot-hold"
    }

    // Feature C: Machine Fault Report
    data object MachineFault : Screen("machine-fault")

    // Feature D: SOP Viewer
    data object SopViewer : Screen("sop-viewer?woId={woId}") {
        fun createRoute(woId: String? = null) =
            if (woId != null) "sop-viewer?woId=$woId" else "sop-viewer"
    }
    data object SopDetail : Screen("sop-viewer/{sopId}") {
        fun createRoute(sopId: String) = "sop-viewer/$sopId"
    }

    // Feature E: Material Issue
    data object MaterialIssue : Screen("material-issue")

    // Feature F: Stock Transfer
    data object StockTransfer : Screen("stock-transfer?lotNumber={lotNumber}") {
        fun createRoute(lotNumber: String? = null) =
            if (lotNumber != null) "stock-transfer?lotNumber=$lotNumber" else "stock-transfer"
    }
}
