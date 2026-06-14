package com.iotviet.aeromes.ui.dashboard

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Build
import androidx.compose.material.icons.filled.Description
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material.icons.filled.Inventory
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.MoveDown
import androidx.compose.material.icons.filled.QrCodeScanner
import androidx.compose.material.icons.filled.Task
import androidx.compose.material.icons.filled.VerifiedUser
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.iotviet.aeromes.R

private data class DashboardTile(
    val title: String,
    val icon: ImageVector,
    val onClick: () -> Unit
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    onNavigateToJobs: () -> Unit,
    onNavigateToQuality: () -> Unit,
    onNavigateToInventory: () -> Unit,
    onNavigateToLotLookup: () -> Unit,
    onNavigateToLotHold: () -> Unit,
    onNavigateToMachineFault: () -> Unit,
    onNavigateToSopViewer: () -> Unit,
    onNavigateToMaterialIssue: () -> Unit,
    onNavigateToStockTransfer: () -> Unit,
    onLogout: () -> Unit
) {
    val tiles = listOf(
        DashboardTile("My Jobs", Icons.Default.Task, onNavigateToJobs),
        DashboardTile("Quality", Icons.Default.VerifiedUser, onNavigateToQuality),
        DashboardTile("Inventory", Icons.Default.Inventory, onNavigateToInventory),
        DashboardTile("Lot Lookup", Icons.Default.QrCodeScanner, onNavigateToLotLookup),
        DashboardTile("Lot Hold", Icons.Default.Lock, onNavigateToLotHold),
        DashboardTile("Machine Fault", Icons.Default.Build, onNavigateToMachineFault),
        DashboardTile("SOP Docs", Icons.Default.Description, onNavigateToSopViewer),
        DashboardTile("Issue Material", Icons.Default.MoveDown, onNavigateToMaterialIssue),
        DashboardTile("Stock Transfer", Icons.Default.Inventory, onNavigateToStockTransfer),
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("AeroMes", fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primary,
                    titleContentColor = MaterialTheme.colorScheme.onPrimary,
                    actionIconContentColor = MaterialTheme.colorScheme.onPrimary
                ),
                actions = {
                    IconButton(onClick = onLogout) {
                        Icon(Icons.Default.ExitToApp, contentDescription = stringResource(R.string.action_logout))
                    }
                }
            )
        }
    ) { padding ->
        LazyVerticalGrid(
            columns = GridCells.Fixed(2),
            contentPadding = PaddingValues(16.dp),
            horizontalArrangement = Arrangement.spacedBy(12.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp),
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            items(tiles) { tile ->
                DashboardTileCard(tile)
            }
        }
    }
}

@Composable
private fun DashboardTileCard(tile: DashboardTile) {
    Card(
        onClick = tile.onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(120.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Icon(
                imageVector = tile.icon,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.padding(bottom = 8.dp)
            )
            Text(tile.title, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.SemiBold)
        }
    }
}
