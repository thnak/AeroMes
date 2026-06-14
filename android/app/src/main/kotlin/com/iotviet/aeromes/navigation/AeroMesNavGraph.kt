package com.iotviet.aeromes.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.iotviet.aeromes.ui.auth.LoginScreen
import com.iotviet.aeromes.ui.auth.LoginViewModel
import com.iotviet.aeromes.ui.dashboard.DashboardScreen
import com.iotviet.aeromes.ui.inventory.InventoryScreen
import com.iotviet.aeromes.ui.jobs.JobDetailScreen
import com.iotviet.aeromes.ui.jobs.JobListScreen
import com.iotviet.aeromes.ui.jobs.JobViewModel
import com.iotviet.aeromes.ui.lot.LotHoldScreen
import com.iotviet.aeromes.ui.lot.LotLookupScreen
import com.iotviet.aeromes.ui.maintenance.MachineFaultScreen
import com.iotviet.aeromes.ui.material.MaterialIssueScreen
import com.iotviet.aeromes.ui.material.StockTransferScreen
import com.iotviet.aeromes.ui.production.ProductionLogScreen
import com.iotviet.aeromes.ui.quality.QualityDetailScreen
import com.iotviet.aeromes.ui.quality.QualityListScreen
import com.iotviet.aeromes.ui.sop.SopDetailScreen
import com.iotviet.aeromes.ui.sop.SopListScreen

@Composable
fun AeroMesNavGraph() {
    val navController = rememberNavController()

    NavHost(navController = navController, startDestination = Screen.Login.route) {

        composable(Screen.Login.route) {
            val vm: LoginViewModel = hiltViewModel()
            val state by vm.uiState.collectAsStateWithLifecycle()
            LoginScreen(
                state = state,
                onEmailChange = vm::onEmailChange,
                onPasswordChange = vm::onPasswordChange,
                onLoginClick = vm::login,
                onLoginSuccess = {
                    navController.navigate(Screen.Dashboard.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                }
            )
        }

        composable(Screen.Dashboard.route) {
            DashboardScreen(
                onNavigateToJobs = { navController.navigate(Screen.Jobs.route) },
                onNavigateToQuality = { navController.navigate(Screen.Quality.route) },
                onNavigateToInventory = { navController.navigate(Screen.Inventory.route) },
                onNavigateToLotLookup = { navController.navigate(Screen.LotLookup.createRoute()) },
                onNavigateToLotHold = { navController.navigate(Screen.LotHold.createRoute()) },
                onNavigateToMachineFault = { navController.navigate(Screen.MachineFault.route) },
                onNavigateToSopViewer = { navController.navigate(Screen.SopViewer.createRoute()) },
                onNavigateToMaterialIssue = { navController.navigate(Screen.MaterialIssue.route) },
                onNavigateToStockTransfer = { navController.navigate(Screen.StockTransfer.createRoute()) },
                onLogout = {
                    navController.navigate(Screen.Login.route) {
                        popUpTo(0) { inclusive = true }
                    }
                }
            )
        }

        composable(Screen.Jobs.route) {
            val vm: JobViewModel = hiltViewModel()
            val state by vm.uiState.collectAsStateWithLifecycle()
            JobListScreen(
                state = state,
                onJobClick = { jobId -> navController.navigate(Screen.JobDetail.createRoute(jobId)) },
                onRefresh = vm::refresh,
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.JobDetail.route,
            arguments = listOf(navArgument("jobId") { type = NavType.LongType })
        ) { backStack ->
            val jobId = backStack.arguments!!.getLong("jobId")
            JobDetailScreen(
                jobId = jobId,
                onLogProduction = { navController.navigate(Screen.Production.createRoute(jobId)) },
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.Production.route,
            arguments = listOf(navArgument("jobId") { type = NavType.LongType })
        ) {
            ProductionLogScreen(
                onSubmitted = { navController.popBackStack() },
                onBack = { navController.popBackStack() }
            )
        }

        composable(Screen.Quality.route) {
            QualityListScreen(
                onOrderClick = { orderId -> navController.navigate(Screen.QualityDetail.createRoute(orderId)) },
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.QualityDetail.route,
            arguments = listOf(navArgument("orderId") { type = NavType.IntType })
        ) { backStack ->
            val orderId = backStack.arguments!!.getInt("orderId")
            QualityDetailScreen(
                orderId = orderId,
                onBack = { navController.popBackStack() }
            )
        }

        composable(Screen.Inventory.route) {
            InventoryScreen(onBack = { navController.popBackStack() })
        }

        // Feature A: Lot Lookup
        composable(
            route = Screen.LotLookup.route,
            arguments = listOf(navArgument("lotNumber") { type = NavType.StringType; nullable = true; defaultValue = null })
        ) { backStack ->
            val lotNumber = backStack.arguments?.getString("lotNumber")
            LotLookupScreen(
                initialLotNumber = lotNumber,
                onNavigateToHold = { lot -> navController.navigate(Screen.LotHold.createRoute(lot)) },
                onNavigateToTransfer = { lot -> navController.navigate(Screen.StockTransfer.createRoute(lot)) },
                onBack = { navController.popBackStack() }
            )
        }

        // Feature B: Lot Hold/Release
        composable(
            route = Screen.LotHold.route,
            arguments = listOf(navArgument("lotNumber") { type = NavType.StringType; nullable = true; defaultValue = null })
        ) { backStack ->
            val lotNumber = backStack.arguments?.getString("lotNumber")
            LotHoldScreen(
                initialLotNumber = lotNumber,
                onBack = { navController.popBackStack() }
            )
        }

        // Feature C: Machine Fault Report
        composable(Screen.MachineFault.route) {
            MachineFaultScreen(onBack = { navController.popBackStack() })
        }

        // Feature D: SOP Viewer (list)
        composable(
            route = Screen.SopViewer.route,
            arguments = listOf(navArgument("woId") { type = NavType.StringType; nullable = true; defaultValue = null })
        ) { backStack ->
            val woId = backStack.arguments?.getString("woId")
            SopListScreen(
                initialWoId = woId,
                onSopClick = { sopId -> navController.navigate(Screen.SopDetail.createRoute(sopId)) },
                onBack = { navController.popBackStack() }
            )
        }

        // Feature D: SOP Detail
        composable(
            route = Screen.SopDetail.route,
            arguments = listOf(navArgument("sopId") { type = NavType.StringType })
        ) { backStack ->
            val sopId = backStack.arguments!!.getString("sopId")!!
            SopDetailScreen(
                sopId = sopId,
                onBack = { navController.popBackStack() }
            )
        }

        // Feature E: Material Issue
        composable(Screen.MaterialIssue.route) {
            MaterialIssueScreen(onBack = { navController.popBackStack() })
        }

        // Feature F: Stock Transfer
        composable(
            route = Screen.StockTransfer.route,
            arguments = listOf(navArgument("lotNumber") { type = NavType.StringType; nullable = true; defaultValue = null })
        ) { backStack ->
            val lotNumber = backStack.arguments?.getString("lotNumber")
            StockTransferScreen(
                initialLotNumber = lotNumber,
                onBack = { navController.popBackStack() }
            )
        }
    }
}
