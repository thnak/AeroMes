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
import com.iotviet.aeromes.ui.production.ProductionLogScreen
import com.iotviet.aeromes.ui.quality.QualityDetailScreen
import com.iotviet.aeromes.ui.quality.QualityListScreen

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
    }
}
